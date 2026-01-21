using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] float attackRange = 2f; // distancia máxima para golpear
    [SerializeField] Transform attackPoint;  // punto desde donde se mide el rango de ataque
    [SerializeField] float attackCooldown = 1f;
    #endregion

    #region Internal States
    Transform player;
    EnemyFSM fsm;
    EnemyAnimationHandler animHandler;

    bool isAttacking;
    float lastAttackTime;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        animHandler = GetComponent<EnemyAnimationHandler>();
    }

    private void Update()
    {
        if (GameStopManager.Instance != null && GameStopManager.Instance.isGamePaused)
            return;

        if (player == null) return;

        if (fsm.CurrentState == EnemyState.Chase)
        {
            // Rotación suave hacia jugador
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f;

            float distance = Vector3.Distance(attackPoint.position, player.position);
            bool canAttack = !isAttacking && Time.time - lastAttackTime >= attackCooldown;

            // Solo atacamos si estamos en rango, no estamos atacando y pasó el cooldown
            if (distance <= attackRange && canAttack)
            {
                isAttacking = true;
                lastAttackTime = Time.time;

                // Detenemos movimiento y animaciones
                animHandler.SetVelocityBody(0f);
                animHandler.SetVelocityLegs(0f);
                animHandler.SetIsRunningBody(false);
                animHandler.SetIsRunningLegs(false);
                animHandler.SetTurnningBody(false);
                animHandler.SetStunnedBody(false);
                animHandler.SetStunnedLegs(false);

                // Lanza animación de ataque
                animHandler.SetAttackBody();
            }
        }
    }

    #region Public Methods
    public void SetTarget(Transform target)
    {
        player = target;
    }

    /// <summary>
    /// Llamado desde Animation Event en el momento del golpe
    /// </summary>
    public void ResolveAttackHit()
    {
        if (player == null) return;

        float distance = Vector3.Distance(attackPoint.position, player.position);
        if (distance > attackRange) return;

        Vector3 hitDirection = (player.position - transform.position).normalized;
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null && playerHealth.IsAlive)
        {
            playerHealth.TakeHit(hitDirection);
        }

        // Desbloquea para el próximo ataque
        isAttacking = false;
    }
    #endregion
}
