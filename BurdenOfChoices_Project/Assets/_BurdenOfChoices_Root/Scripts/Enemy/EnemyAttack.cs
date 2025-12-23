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
    float lastAttackTime;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
    }

    private void Update()
    {
        if(player == null) return;

        //Solo atacar si estamos persiguiendo
        if(fsm.CurrentState != EnemyState.Chase) return;

        //Verificar distancia
        float distance = Vector3.Distance(attackPoint.transform.position, player.position);
        if(distance <= attackRange && Time.time - lastAttackTime >= attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    #region Public Methods
    // Asigna el objetivo del jugador
    public void SetTarget(Transform target)
    {
        player = target;
    }
    #endregion

    #region Internal Logic
    void AttackPlayer()
    {
        if (player == null) return;

        // Calculamos dirección hacia el jugador para el knockback
        Vector3 hitDirection = (player.position - transform.position).normalized;

        // Llamamos al PlayerHealth
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeHit(hitDirection);
        }
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        // Dibujar un círculo que representa el rango de ataque
        Handles.color = Color.red;
        Handles.DrawWireDisc(attackPoint.transform.position, Vector3.up, attackRange);
    }
}
