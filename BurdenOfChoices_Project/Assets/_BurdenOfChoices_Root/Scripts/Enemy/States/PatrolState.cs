using JetBrains.Annotations;
using System;
using UnityEngine;

public class PatrolState : MonoBehaviour, IEnemyState
{
    //Inspector
    [SerializeField] EnemyData enemyData;
    [SerializeField] Transform[] patrolPoints;

    #region Internal States
    int currentIndex = 0;
    float idleTimer;
    public bool idleInProgress;
    string stopOwnerId;
    #endregion

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    TurnToTargetState turnState;
    EnemyMotionContext moveContext;
    #endregion

    public void Initialize(EnemyFSM enemyfsm, EnemyMovementCommands command, TurnToTargetState turn, EnemyMotionContext move)
    {
        fsm = enemyfsm;
        movementCommands = command;
        turnState = turn;
        moveContext = move;
    }

    #region State Flow
    public void Enter()
    {
        idleInProgress = false;


        //Reanudamos movimiento con parámetros de patrulla
        movementCommands.ResumeMovement(enemyData.patrolSpeed, enemyData.normalAcceleration);

        //Asignamos objetivo inicial
        if(patrolPoints.Length > 0)
        {
            movementCommands.SetMoveTarget(patrolPoints[currentIndex].position, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);
        }
    }

    public void Handle()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentIndex];

        if (!idleInProgress)
        {
            //Movimiento hacia el punto de patrulla
            movementCommands.SetMoveTarget(target.position, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);

            //Rotación manual suavizada
            movementCommands.RotateTowards(target.position, enemyData.rotationStiffness, enemyData.rotationDamping);

            //Comporbación de llegada al punto
            if (!moveContext.Agent.pathPending && moveContext.Agent.remainingDistance <= moveContext.Agent.stoppingDistance + 0.1f)
            {
                idleInProgress = true;
                idleTimer = enemyData.idleTime;

                //transición al estado de idle -> si llego al punto de patrulla
                fsm.OnIdle();
            }
        }
        else
        {
            idleTimer -= Time.deltaTime;
            if(idleTimer <= 0)
            {
                idleInProgress = false;
                AdvanceIndex();
            }
        }

    }

    public void Exit()
    {
        idleInProgress = false; 
        
        movementCommands.ResetRotation();
    }
    #endregion

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
