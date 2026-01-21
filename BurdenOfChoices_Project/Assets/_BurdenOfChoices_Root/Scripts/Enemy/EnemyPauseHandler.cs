using UnityEngine;

/// <summary>
/// EnemyPauseHandler
/// Gestiona pausa global del enemigo sin tocar la FSM ni los States.
/// Congela movimiento, navegación y animaciones mientras GameStopManager.isGamePaused = true.
/// </summary>
public class EnemyPauseHandler : MonoBehaviour
{
    #region References
    [SerializeField] EnemyData data;
    EnemyFSM fsm;
    EnemyMovementCommands commands;
    EnemyMotionContext motionContext;
    EnemyAnimationHandler animationHandler;
    #endregion

    #region Internal States
    bool wasPaused = false;
    string entryStateName = "Idle"; // nombre de animación base (Entry)
    #endregion

    private void Awake()
    {
        motionContext = GetComponent<EnemyMotionContext>();
        animationHandler = GetComponent<EnemyAnimationHandler>();
        // Auto-asignación si no se arrastró nada
        if (fsm == null) fsm = GetComponent<EnemyFSM>();
       
    }

    private void Start()
    {
        commands = motionContext.Commands;
    }

    private void Update()
    {
        if (GameStopManager.Instance == null) return;

        if (GameStopManager.Instance.isGamePaused && !wasPaused)
        {
            // Aplicamos pausa
            PauseEnemy();
            wasPaused = true;
        }
        else if (!GameStopManager.Instance.isGamePaused && wasPaused)
        {
            // Restauramos al salir de pausa
            ResumeEnemy();
            wasPaused = false;
        }
    }

    #region Pause / Resume
    void PauseEnemy()
    {
        if (commands != null)
            commands.PauseMovement(); // detiene NavMeshAgent y velocidad interna
    }

    void ResumeEnemy()
    {
        if (commands != null)
        {
            commands.ResumeMovement(data.patrolSpeed, data.normalAcceleration);
        }
    }
    #endregion
}
