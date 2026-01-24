using System;
using UnityEngine;

public class ChaseState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    VisionSystem visionSystem;
    EnemyAnimationHandler animatorHandle;
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, VisionSystem vision, EnemyAnimationHandler enemyAnimator)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        visionSystem = vision;
        animatorHandle = enemyAnimator;
    }

    #region State Flow
    public void Enter() 
    {
        //Reanudamos movimiento limpio con parámetros de persecución
        movementCommands.ResumeMovement(enemyData.chaseSpeed, enemyData.normalAcceleration);

        //Resetea rotación acumulada y destino anterior
        movementCommands.ResetRotation();

        // Activamos la lógica de frenado progresivo
        movementCommands.EnableStopLogic(true);
        movementCommands.ConfigureStopArea(enemyData.stopStartDistance, enemyData.stopHardDistance);

        //Animator
        animatorHandle.SetVelocityBody(1f);
        animatorHandle.SetVelocityLegs(1f);
        animatorHandle.SetIsRunningBody(true);
        animatorHandle.SetIsRunningLegs(true);
        animatorHandle.SetTurnningBody(false);
    }

    public void Handle()
    {
        if (visionSystem == null || visionSystem.Target == null) return;

        Vector3 targetPos = visionSystem.Target.position;

        //Siempre rotamos hacie el objetivo
        movementCommands.RotateTowards(targetPos, enemyData.rotationChaseStiffness, enemyData.rotationChaseDamping);

        //Comprobamos alineación
        bool isAligned = movementCommands.IsAlignedTo(targetPos, enemyData.chaseAlignmentAngle);

        //Si no está alineado, nos recolocamos sin avanzar.
        if (!isAligned)
        {
            movementCommands.PauseMovement();
            return;
        }

        movementCommands.SetMoveTarget(targetPos, enemyData.chaseSpeed, enemyData.destinationUpdateThreshold);
    }

    public void Exit()
    {
        // Desactivamos la lógica de frenado
        movementCommands.EnableStopLogic(false);

        // Solo reanudamos si el agente está activo y sobre NavMesh
        movementCommands.ResumeMovement(enemyData.patrolSpeed, enemyData.normalAcceleration);
    }
    #endregion
}
