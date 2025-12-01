using System;
using UnityEngine;

public class IdleState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    float idleTimer;
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    PatrolState patrolState;
    TurnToTargetState turnState;

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, PatrolState patrol, TurnToTargetState turn)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        patrolState = patrol;
        turnState = turn;
    }

    public void Enter()
    {
        idleTimer = enemyData.idleTime;
    }

    public void Handle()
    {
        if (idleTimer <= 0f) return;

        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f)
        {
            patrolState.AdvanceIndex();
            Transform next = patrolState.GetCurrentPoint();
            turnState.SetTarget(next);

            fsm.OnTurnTuTarget();
        }
    }

    public void Exit()
    {
        idleTimer = 0f;
    }
}
