using UnityEngine;

public class DeathState : MonoBehaviour, IEnemyState
{
    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    EnemyLightHandler enemyLight;
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, EnemyLightHandler lightHandler)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        enemyLight = lightHandler;
    }

    public void Enter()
    {
        // Detener cualquier movimiento controlado
        movementCommands.PauseMovement();

        // FSM ya no debe procesar lógica
        fsm.enabled = false;

        enemyLight.TurnOff();

        // Desactivar todos los componentes innecesarios
        DisableEnemyLogic();
    }

    public void Handle() { }

    public void Exit() { }

    #region Utilities
    void DisableEnemyLogic()
    {
        foreach (var mb in GetComponents<MonoBehaviour>())
        {
            if (mb == this) continue;

            mb.enabled = false;
        }
    }
    #endregion
}
