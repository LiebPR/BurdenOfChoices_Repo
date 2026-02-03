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
    EnemyPerceptionFeedback perceptionFeedback;
    EnemyAnimationHandler animatorHandler;
    EnemyLightHandler lightHandler;
    #endregion

    private void Awake()
    {

        // Obtenemos referencias que dependen de otros componentes inicializados en Awake()
        moveContext = GetComponent<EnemyMotionContext>();
        fsm = GetComponent<EnemyFSM>();
        vision = GetComponent<VisionSystem>();
        health = GetComponent<EnemyHealth>();
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        perceptionHandler = GetComponent<EnemyPerceptionHandler>();

        animatorHandler = GetComponent<EnemyAnimationHandler>();
        lightHandler = GetComponent<EnemyLightHandler>();
        perceptionFeedback = GetComponent<EnemyPerceptionFeedback>();
        patrol = GetComponent<PatrolState>();
        idle = GetComponent<IdleState>();
        chase = GetComponent<ChaseState>();
        turnState = GetComponent<TurnToTargetState>();
        death = GetComponent<DeathState>();
        stun = GetComponent<StunState>();
        investigateSound = GetComponent<InvestigateSoundState>();
    }

    private void Start()
    {

        commands = moveContext.Commands; // Ahora Commands NO será null

        patrol.Initialize(fsm, commands, turnState, moveContext, animatorHandler);
        idle.Initialize(fsm, commands, patrol, turnState, animatorHandler);
        chase.Initialize(fsm, commands, vision, animatorHandler);
        turnState.Initialize(fsm, commands, animatorHandler);
        death.Initialize(fsm, commands, lightHandler, vision, perceptionFeedback);
        stun.Initialize(fsm, commands, health, rb, agent, lightHandler);
        investigateSound.Initialize(fsm, commands, vision, perceptionHandler, moveContext, animatorHandler);

        // Registramos estados en la FSM
        fsm.RegisterState(EnemyState.Patrol, patrol);
        fsm.RegisterState(EnemyState.Idle, idle);
        fsm.RegisterState(EnemyState.Chase, chase);
        fsm.RegisterState(EnemyState.TurnToTarget, turnState);
        fsm.RegisterState(EnemyState.Death, death);
        fsm.RegisterState(EnemyState.Stun, stun);
        fsm.RegisterState(EnemyState.InvestigateSound, investigateSound);
        // 3️⃣ Inicializamos la FSM de forma segura (no entra Enter todavía)
        fsm.InitializeFSM(EnemyState.Patrol);

        // 4️⃣ Finalmente arrancamos la FSM
        fsm.StartFSM(); // PatrolState.Enter() ahora corre con movementCommands ya asignado
    }
}