using UnityEditor;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] float attackRange = 2f; //distancia máxima para golpear
    [SerializeField] float attackCooldown = 1f; //tiempo entre ataques
    [SerializeField] Transform attackPoint; //Punto desde donde se mide el rango de ataque
    #endregion

    #region Internal States
    Transform player;
    EnemyFSM fsm;
    EnemyAnimationHandler animHandler;
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
            return; // El enemigo no ejecuta lógica mientras el juego está pausado

        if (player == null) return;

        //Solo atacar si estamos persiguiendo
        if(fsm.CurrentState != EnemyState.Chase) return;

        //Verificar distancia
        float distance = Vector3.Distance(attackPoint.transform.position, player.position);
        if(distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            //Animator
            animHandler.SetAttackBody();
            lastAttackTime = Time.time;
        }
    }

    #region Public Methods
    // Asigna el objetivo del jugador
    public void SetTarget(Transform target)
    {
        player = target;
    }

    public void ResolveAttackHit()
    {
        AttackPlayer();
    }
    #endregion

    #region Internal Logic
    void AttackPlayer()
    {
        if (player == null) return;

        // Llamamos al PlayerHealth
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if(playerHealth == null || !playerHealth.IsAlive) return;

        Vector3 hitDirection = (player.position - transform.position).normalized;
        playerHealth.TakeHit(hitDirection);
    }
    #endregion
}
