using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

public class DeathState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands)
    {
        fsm = enemyFsm;
        movementCommands = commands;
    }

    public void Enter()
    {

        Destroy(gameObject);
    }

    public void Handle()
    {
        Debug.Log("Estoy Muerto");
    }

    public void Exit()
    {

    }
}
