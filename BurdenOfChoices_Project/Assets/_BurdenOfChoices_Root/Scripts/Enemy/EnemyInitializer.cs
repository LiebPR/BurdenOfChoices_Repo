using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    [SerializeField] PatrolState patrol;
    [SerializeField] IdleState idle;
    [SerializeField] ChaseState chase;

    EnemyFSM fsm;
    EnemyMovementCommands commands;
    VisionSystem vision;
    TurnToTargetState turnState;
    EnemyMoveController moveController;

    private void Start()
    {
        fsm = GetComponent<EnemyFSM>();
        vision = GetComponentInChildren<VisionSystem>();
        commands = GetComponent<EnemyMoveController>().Commands;
        turnState = GetComponent<TurnToTargetState>();
        moveController = GetComponent<EnemyMoveController>();

        // Inicializamos cada estado
        patrol.Initialize(fsm, commands, turnState, moveController);
        idle.Initialize(fsm, commands, patrol, turnState);
        chase.Initialize(fsm, commands, vision);
        turnState.Initialize(fsm, commands);

        // Registramos estados en la FSM
        fsm.RegisterState(EnemyState.Patrol, patrol);
        fsm.RegisterState(EnemyState.Idle, idle);
        fsm.RegisterState(EnemyState.Chase, chase);
        fsm.RegisterState(EnemyState.TurnToTarget, turnState);

        // Activamos el primer estado
        fsm.ResetState(); // Esto llamará a ChangeState(EnemyState.Patrol) y ejecutará Enter()
    }
}