using UnityEngine;

public class EnemyInitializer : MonoBehaviour
{
    #region References
    PatrolState patrol;
    IdleState idle;
    ChaseState chase;
    TurnToTargetState turnState;
    AlertState alert;
    
    EnemyFSM fsm;
    EnemyMovementCommands commands;
    VisionSystem vision;
    EnemyMoveController moveController;
    #endregion

    private void Start()
    {
        patrol = GetComponent<PatrolState>();
        idle = GetComponent<IdleState>();
        chase = GetComponent<ChaseState>();
        turnState = GetComponent<TurnToTargetState>();
        alert = GetComponent<AlertState>();

        fsm = GetComponent<EnemyFSM>();
        vision = GetComponent<VisionSystem>();
        commands = GetComponent<EnemyMoveController>().Commands;
        moveController = GetComponent<EnemyMoveController>();


        // Inicializamos cada estado
        patrol.Initialize(fsm, commands, turnState, moveController);
        idle.Initialize(fsm, commands, patrol, turnState);
        chase.Initialize(fsm, commands, vision);
        turnState.Initialize(fsm, commands);
        alert.Initialize(fsm, commands);

        // Registramos estados en la FSM
        fsm.RegisterState(EnemyState.Patrol, patrol);
        fsm.RegisterState(EnemyState.Idle, idle);
        fsm.RegisterState(EnemyState.Chase, chase);
        fsm.RegisterState(EnemyState.TurnToTarget, turnState);
        fsm.RegisterState(EnemyState.Alert, alert);

        // Activamos el primer estado
        fsm.ResetState(); // Esto llamará a ChangeState(EnemyState.Patrol) y ejecutará Enter()
    }
}