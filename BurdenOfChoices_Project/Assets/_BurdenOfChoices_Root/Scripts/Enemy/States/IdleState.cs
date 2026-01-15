using System;
using UnityEngine;

public class IdleState : MonoBehaviour, IEnemyState
{
    //Inpector 
    [SerializeField] EnemyData enemyData;

    //Internal State
    float idleTimer;

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    PatrolState patrolState;
    TurnToTargetState turnState;
    EnemyAnimationHandler animatorHandle;
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, PatrolState patrol, TurnToTargetState turn, EnemyAnimationHandler enemyAnimator)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        patrolState = patrol;
        turnState = turn;
        animatorHandle = enemyAnimator;
    }

    public void Enter()
    {
        idleTimer = enemyData.idleTime;

        //Animator
        animatorHandle.SetVelocityBody(0f);
        animatorHandle.SetVelocityLegs(0f);
        animatorHandle.SetIsRunningBody(false);
        animatorHandle.SetIsRunningLegs(false);
        animatorHandle.SetTurnningBody(false);
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

            fsm.OnTurnTuTarget(next);
        }
    }

    public void Exit()
    {
        idleTimer = 0f;
    }
}
