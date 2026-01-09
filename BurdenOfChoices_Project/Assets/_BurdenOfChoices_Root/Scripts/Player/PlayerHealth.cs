using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    #region Inspector Variables
    [Header("Health Config")]
    [SerializeField] float knockbackForce = 5f; // fuerza del impulso hacia atrás

    [Header("Respawn System")]
    [SerializeField] Transform firstDeathRespawnPoint; // punto donde reaparece en la primera muerte
    [SerializeField] float respawnDelay = 1.2f; // tiempo en pantalla negra

    [Header("Cinemachine Cameras")]
    [SerializeField] CinemachineCamera deathCamera;
    #endregion

    #region Internal State
    bool isAlive = true;
    int deathCount = 0;
    int cameraHighPriority = 20;
    Vector3 lastHitDirection;
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

    #region Public Methods
    public void TakeHit(Vector3 hitDirection)
    {
        if (!isAlive) return;

        isAlive = false; // jugador muere al primer golpe
        lastHitDirection = hitDirection;
        ApplyKnockback(hitDirection);
        HandleDeath();
    }

    /// <summary>
    /// Llamado exclusivamente desde un ANimation Event al finalizar la animación de muerte. 
    /// </summary>
    public void OnDeathAnimationFinished()
    {
        if(deathCount == 1)
        {
            if(firstDeathRespawnPoint != null)
                StartCoroutine(RespawnRoutine(firstDeathRespawnPoint));
            else
                ResetPlayerState();
        }
        else
        {
            if (SceneController.Instance != null)
                SceneController.Instance.LoadScene("SCN_LoseMenu");
        }
    }
    #endregion

    #region Internal Logic
    void ApplyKnockback(Vector3 hitDirection)
    {
        Vector3 planDir = new Vector3(hitDirection.x, 0f, hitDirection.z).normalized;
        rb.AddForce(planDir * knockbackForce, ForceMode.Impulse);
    }

    void HandleDeath()
    {
        //Girar al jugador hacia la dirección contraria del golpe
        RotateAwayFromHit();

        animatorManager.DeathAnim();

        //Deshabilitar el controlador para que no se mueva más 
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        var vision = FindAnyObjectByType<VisionSystem>();
        if (vision != null) vision.ResetVisionToDefault();

        var enemyFsm = FindAnyObjectByType<EnemyFSM>();
        if (enemyFsm != null) enemyFsm.ForceResetToPatrol();

        deathCount++;
    }

    void ReappearAtPoint(Transform respawnPoint)
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("ReappearAtPoint: respawnPoint es null.");
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        isAlive = true;
    }

    void ResetPlayerState()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

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
        if (fadeController != null) yield return fadeController.FadeOut();
        yield return new WaitForSeconds(respawnDelay);
        ReappearAtPoint(respawnPoint);

        if (deathCamera != null) deathCamera.Priority = cameraHighPriority;
        if (fadeController != null) yield return fadeController.FadeIn();
    }
    #endregion
}
