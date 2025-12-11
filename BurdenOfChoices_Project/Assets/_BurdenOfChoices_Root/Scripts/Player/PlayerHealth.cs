using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    #region Inspector Variables
    [Header("Health Config")]
    [SerializeField] float knockbackForce = 5f; //fuerza del impulso hacia atrás
    [SerializeField] Transform firstDeathRespawnPoint; //punto donde reaparece en la primera muerte
    #endregion

    #region Internal State
    bool isAlive = true;
    int deathCount = 0;
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    #region Public Methods
    public void TakeHit(Vector3 hitDirection)
    {
        if (!isAlive) return;

        isAlive = false; //jugador muere al primer golpe
        ApplyKnockback(hitDirection);
        HandleDeath();
    }
    #endregion

    #region Internal Logic
    //Aplicamos un impulso atrás al jugador
    void ApplyKnockback(Vector3 hitDirection)
    {
        //Solo queremos el plano z
        Vector3 planDir = new Vector3(hitDirection.x, 0f, hitDirection.z).normalized;
        rb.AddForce(planDir * knockbackForce, ForceMode.Impulse);
    }

    //Maneja la muerte del jugador
    void HandleDeath()
    {
        //Deshabilitar el controlador para que no se mueva más 
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = false;

        deathCount++;

        if(deathCount == 1)
        {
            // Primera muerte: reaparecer en un punto definido (si está asignado)
            if (firstDeathRespawnPoint != null)
            {
                ReappearAtPoint(firstDeathRespawnPoint);
            }
            else
            {
                Debug.LogWarning("PlayerHealth: firstDeathRespawnPoint no está asignado. No se puede reaparecer.");
                // Como fallback, reactiva al jugador en su posición actual
                ResetPlayerState();
            }
        }
        else
        {
            //Segunda muerte o más: ir a la LoseScene
            if (SceneController.Instance != null)
                SceneController.Instance.LoadScene("SCN_LoseMenu");
        }
    }

    void ReappearAtPoint(Transform respawnPoint)
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("ReappearAtPoint: respawnPoint es null.");
            return;
        }

        // Reset física
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Teletransportar al punto de respawn
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        // Reactivar controlador y estado de vida
        var controller = GetComponent<PlayerController>();
        if (controller != null) controller.enabled = true;

        isAlive = true;
    }

    //Método auxiliar usado para reactivar sin mover
    void ResetPlayerState()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        var controller = GetComponent<PlayerController>();
        if(controller != null) controller.enabled = true;

        isAlive = true;
    }
    #endregion
}
