using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    #region Inspector Variables
    [Header("Health Config")]
    [SerializeField] float knockbackForce = 5f; // fuerza del impulso hacia atrás

    [Header("Respawn System")]
    [SerializeField] Transform defaultRespawnPoint; // punto donde reaparece en la primera muerte

    [Header("Fade Settings")]
    [SerializeField] float fadeInDelay = 0.7f; // tiempo configurable antes de permitir fade

    [Header("Cinemachine Cameras")]
    [SerializeField] CinemachineCamera deathCamera;
    #endregion

    #region Internal State
    bool isAlive = true;
    bool respawnConsummed = false;
    int cameraHighPriority = 20;
    Vector3 lastHitDirection;

    Transform overrideRespawnPoint; //respawn temporal (puzzle)
    CinemachineCamera overrideRespawnCamera; //cámara temporal (puzzle)
    #endregion

    #region Getters
    public bool IsAlive => isAlive;
    #endregion

    #region References
    FadeController fadeController;
    Rigidbody rb;
    AnimatorManager animatorManager;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (fadeController == null)
        {
            fadeController = FindAnyObjectByType<FadeController>();
        }

        animatorManager = GetComponent<AnimatorManager>();
    }

    #region API - Core
    public void TakeHit(Vector3 hitDirection)
    {
        if (!isAlive) return;

        isAlive = false;
        lastHitDirection = hitDirection;

        //1. Pusar juego (estado global)
        GameStopManager.Instance.PauseGame();

        //2.Bloquear input del player (estado local)
        var controller = GetComponent<PlayerController>();
        if(controller != null)
            controller.PausePlayer();

        // Aplicar knockback inmediatamente
        ApplyKnockback(hitDirection);

        // Manejar muerte (animación y lógica)
        HandleDeath();
    }

    /// <summary>
    /// Llamado exclusivamente desde un ANimation Event al finalizar la animación de muerte. 
    /// </summary>
    public void OnDeathAnimationFinished()
    {
        if (!respawnConsummed)
        {
            Transform respawnPoint = GetCurrentRespawnPoint();
            if (respawnPoint == null) respawnPoint = defaultRespawnPoint; 

            if (respawnPoint != null)
            {
                respawnConsummed = true;
                StartCoroutine(RespawnRoutine(respawnPoint));
                return;
            }
            else
                Debug.LogWarning("No hay respawn point asignado. Se cargará la LoseScene");
        }

        //Si ya no hay respawn disponible -> perder
        SceneController.Instance.LoadScene("SCN_LoseMenu");
    }
    #endregion

    #region API - Respawn Point Override (Puzzle)
    public void SetOverrideRespawnPoint(Transform newRespawn, CinemachineCamera newCamera = null)
    {
        overrideRespawnPoint = newRespawn;
        overrideRespawnCamera = newCamera;
    }

    public void ClearOverrideRespawn()
    {
        overrideRespawnPoint = null;
        overrideRespawnCamera = null;
    }
    #endregion

    #region Internal Logic
    void ApplyKnockback(Vector3 hitDirection)
    {
        Vector3 planDir = new Vector3(hitDirection.x, 0f, hitDirection.z).normalized;
        float knockbackStrength = knockbackForce;

        // Enviar el knockback al PlayerController
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.AddExternalImpulse(planDir * knockbackForce);
        }
    }

    void HandleDeath()
    {
        //Girar al jugador hacia la dirección contraria del golpe
        RotateAwayFromHit();
        animatorManager.DeathAnim();

        var vision = FindAnyObjectByType<VisionSystem>();
        if (vision != null) vision.ResetVisionToDefault();

        var enemyFsm = FindAnyObjectByType<EnemyFSM>();
        if (enemyFsm != null) enemyFsm.ForceResetToPatrol();
    }

    Transform GetCurrentRespawnPoint()
    {
        return overrideRespawnPoint != null ? overrideRespawnPoint : defaultRespawnPoint;
    }

    void ReappearAtPoint(Transform respawnPoint)
    {
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;
        isAlive = true;
    }

    void RotateAwayFromHit()
    {
        // El vector del golpe ya se planea en el plano XZ
        Vector3 hitDir = new Vector3(lastHitDirection.x, 0f, lastHitDirection.z);
        hitDir.y = 0f;

        if (hitDir.sqrMagnitude < 0.01f) return; // evitar NaN

        // Rotación hacia el opuesto del golpe
        Vector3 oppositeDir = -hitDir.normalized;
        Quaternion targetRot = Quaternion.LookRotation(oppositeDir, Vector3.up);

        // Aplicar la rotación instantánea
        transform.rotation = targetRot;
    }
    #endregion

    #region Routine
    IEnumerator RespawnRoutine(Transform respawnPoint)
    {
        if (fadeController != null)
            yield return fadeController.FadeOut();

        ReappearAtPoint(respawnPoint);

        // Activar cámara del override si existe, sino la default
        CinemachineCamera camToActivate = overrideRespawnCamera != null ? overrideRespawnCamera : deathCamera;
        if (camToActivate != null)
            CameraManager.Instance.ActivateCamera(camToActivate, cameraHighPriority);


        yield return new WaitForSeconds(fadeInDelay);

        // 5. Fade in
        if (fadeController != null)
            yield return fadeController.FadeIn();
        GameStopManager.Instance.ResumeGame();
    }

    #endregion
}
