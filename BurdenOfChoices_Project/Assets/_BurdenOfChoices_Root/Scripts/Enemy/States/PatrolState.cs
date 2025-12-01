using JetBrains.Annotations;
using System;
using UnityEngine;

public class PatrolState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;
    [SerializeField] Transform[] patrolPoints;

    int currentIndex = 0;
    float idleTimer;
    bool idleInProgress;
    string stopOwnerId;

    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    TurnToTargetState turnState;
    EnemyMoveController moveController;

    public void Initialize(EnemyFSM enemyfsm, EnemyMovementCommands command, TurnToTargetState turn, EnemyMoveController move)
    {
        fsm = enemyfsm;
        movementCommands = command;
        turnState = turn;
        moveController = move;
    }

    public void Enter()
    {
        idleInProgress = false;


        movementCommands.ResetDestination();
        movementCommands.ResumeMovement(stopOwnerId, enemyData.patrolSpeed, enemyData.normalAcceleration);

        //Reasigna destino al punto actual
        movementCommands.MoveTo(patrolPoints[currentIndex].position, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);
    }

    public void Handle()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentIndex];

        if (!idleInProgress)
        {
            movementCommands.MoveTo(target.position, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);
            movementCommands.RotateTowards(target.position, enemyData.rotationStiffness, enemyData.rotationDamping);

            //Llegada al punto
            if (!moveController.Agent.pathPending && moveController.Agent.remainingDistance <= moveController.Agent.stoppingDistance + 0.1f)
            {
                idleInProgress = true;
                idleTimer = enemyData.idleTime;
                fsm.OnIdle();
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if(idleTimer <= 0)
            {
                idleInProgress = false;
                currentIndex = (currentIndex + 1) % patrolPoints.Length;
            }
        }

    }

    public void Exit()
    {
        idleInProgress = false;    
    }

    #region Utilities

    public void AdvanceIndex()
    {
        currentIndex = (currentIndex + 1) % patrolPoints.Length;
    }

    public Transform GetCurrentPoint()
    {
        return patrolPoints[currentIndex];
    }
    #endregion
}
