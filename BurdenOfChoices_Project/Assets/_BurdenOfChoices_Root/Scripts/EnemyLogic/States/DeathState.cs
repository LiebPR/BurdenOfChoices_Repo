using UnityEngine;

public class DeathState : MonoBehaviour, IEnemyState
{
    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    EnemyLightHandler enemyLight;
    VisionSystem visionSystem;                       // Nueva referencia
    EnemyPerceptionFeedback perceptionFeedback;    // Nueva referencia
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, EnemyLightHandler lightHandler, VisionSystem vision, EnemyPerceptionFeedback feedback)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        enemyLight = lightHandler;
        visionSystem = vision;
        perceptionFeedback = feedback;
    }

    public void Enter()
    {
        // Detener cualquier movimiento controlado
        movementCommands.PauseMovement();

        // Apagar luz del enemigo
        enemyLight.TurnOff();

        // Apagar visión y feedback
        if (visionSystem != null)
            visionSystem.enabled = false;

        if (perceptionFeedback != null)
            perceptionFeedback.enabled = false;

        DisablePhysicsAndColliders();
    }

    public void Handle() { }

    public void Exit() { }

    #region Utilities
    void DisablePhysicsAndColliders()
    {
        // Collider
        foreach (var col in GetComponents<Collider>())
        {
            col.enabled = false;
        }

        // Opcional: otros componentes que afecten físicas
        foreach (var joint in GetComponents<Joint>())
        {
            Destroy(joint); // si hay articulaciones, eliminarlas
        }
    }
    #endregion
}
