using UnityEngine;
using UnityEngine.AI;

public class EnemyInitializer : MonoBehaviour
{
    #region References
    PatrolState patrol;
    IdleState idle;
    ChaseState chase;
    TurnToTargetState turnState;
    AlertState alert;
    DeathState death;
    StunState stun;
    
    EnemyFSM fsm;
    EnemyMovementCommands commands;
    VisionSystem vision;
    EnemyMotionContext moveContext;
    EnemyHealth health;
    Rigidbody rb;
    NavMeshAgent agent;
    #endregion

    private void Awake()
    {
        patrol = GetComponent<PatrolState>();
        idle = GetComponent<IdleState>();
        chase = GetComponent<ChaseState>();
        turnState = GetComponent<TurnToTargetState>();
        alert = GetComponent<AlertState>();
        death = GetComponent<DeathState>();
        stun = GetComponent<StunState>();

        fsm = GetComponent<EnemyFSM>();
        vision = GetComponent<VisionSystem>();
        commands = GetComponent<EnemyMotionContext>().Commands;
        moveContext = GetComponent<EnemyMotionContext>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();   


        // Inicializamos cada estado
        patrol.Initialize(fsm, commands, turnState, moveContext);
        idle.Initialize(fsm, commands, patrol, turnState);
        chase.Initialize(fsm, commands, vision);
        turnState.Initialize(fsm, commands);
        alert.Initialize(fsm, commands);
        death.Initialize(fsm, commands);
        stun.Initialize(fsm, commands, health, rb, agent);

        // Registramos estados en la FSM
        fsm.RegisterState(EnemyState.Patrol, patrol);
        fsm.RegisterState(EnemyState.Idle, idle);
        fsm.RegisterState(EnemyState.Chase, chase);
        fsm.RegisterState(EnemyState.TurnToTarget, turnState);
        fsm.RegisterState(EnemyState.Death, death);
        fsm.RegisterState(EnemyState.Stun, stun);

        // Activamos el primer estado
        fsm.ResetState(); // Esto llamará a ChangeState(EnemyState.Patrol) y ejecutará Enter()
    }
}