using UnityEngine;
using UnityEngine.AI;

public class StunState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    EnemyHealth health;
    Rigidbody rb;
    NavMeshAgent agent;
    #endregion

    //Internal States
    bool isStunned;

    //Getter
    public bool IsStunned => isStunned;

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, EnemyHealth enemyHeatlth, Rigidbody rigidbody, NavMeshAgent navAgent)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        health = enemyHeatlth;
        rb = rigidbody;
        agent = navAgent;
    }

    public void Enter()
    {
        isStunned = true;
    }

    public void Handle()
    {
        if(rb != null)
            rb.isKinematic = false;
    }

    public void Exit()
    {
        agent.enabled = true;
        rb.isKinematic = true;
        isStunned = false;
    }
}
