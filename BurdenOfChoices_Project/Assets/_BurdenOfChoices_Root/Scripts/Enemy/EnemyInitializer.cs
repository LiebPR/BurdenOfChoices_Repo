using UnityEngine;
using UnityEngine.AI;

public class EnemyInitializer : MonoBehaviour
{
    #region References
    PatrolState patrol;
    IdleState idle;
    ChaseState chase;
    TurnToTargetState turnState;
    DeathState death;
    StunState stun;
    InvestigateSoundState investigateSound;

    EnemyFSM fsm;
    EnemyMovementCommands commands;
    VisionSystem vision;
    EnemyMotionContext moveContext;
    EnemyHealth health;
    Rigidbody rb;
    NavMeshAgent agent;
    EnemyPerceptionHandler perceptionHandler;
    EnemyAnimationHandler animatorHandler;
    #endregion

    private void Awake()
    {
        patrol = GetComponent<PatrolState>();
        idle = GetComponent<IdleState>();
        chase = GetComponent<ChaseState>();
        turnState = GetComponent<TurnToTargetState>();
        death = GetComponent<DeathState>();
        stun = GetComponent<StunState>();
        investigateSound = GetComponent<InvestigateSoundState>();

        fsm = GetComponent<EnemyFSM>();
        vision = GetComponent<VisionSystem>();
        commands = GetComponent<EnemyMotionContext>().Commands;
        moveContext = GetComponent<EnemyMotionContext>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        perceptionHandler = GetComponent<EnemyPerceptionHandler>();
        animatorHandler = GetComponent<EnemyAnimationHandler>();

        // Inicializamos cada estado
        patrol.Initialize(fsm, commands, turnState, moveContext, animatorHandler);
        idle.Initialize(fsm, commands, patrol, turnState, animatorHandler);
        chase.Initialize(fsm, commands, vision, animatorHandler);
        turnState.Initialize(fsm, commands, animatorHandler);
        death.Initialize(fsm, commands);
        stun.Initialize(fsm, commands, health, rb, agent);
        investigateSound.Initialize(fsm, commands, vision, perceptionHandler, moveContext, animatorHandler);

        // Registramos estados en la FSM
        fsm.RegisterState(EnemyState.Patrol, patrol);
        fsm.RegisterState(EnemyState.Idle, idle);
        fsm.RegisterState(EnemyState.Chase, chase);
        fsm.RegisterState(EnemyState.TurnToTarget, turnState);
        fsm.RegisterState(EnemyState.Death, death);
        fsm.RegisterState(EnemyState.Stun, stun);
        fsm.RegisterState(EnemyState.InvestigateSound, investigateSound);

        // Activamos el primer estado
        fsm.ResetState(); // Esto llamará a ChangeState(EnemyState.Patrol) y ejecutará Enter()
    }
}