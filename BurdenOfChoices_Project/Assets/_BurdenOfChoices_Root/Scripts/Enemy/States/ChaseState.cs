using System;
using UnityEngine;

public class ChaseState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    VisionSystem visionSystem;
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, VisionSystem vision)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        visionSystem = vision;
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
    }

    public void Handle()
    {
        if (visionSystem == null || visionSystem.Target == null) return;

        Vector3 targetPos = visionSystem.Target.position;
        float distance = Vector3.Distance(transform.position, targetPos);

        // Movimiento hacia el jugador
        movementCommands.SetMoveTarget(targetPos, enemyData.chaseSpeed, enemyData.destinationUpdateThreshold);

        // Rotación hacia el objetivo
        movementCommands.RotateTowards(targetPos, enemyData.rotationChaseStiffness, enemyData.rotationChaseDamping);

    }

    public void Exit() 
    {
        // Desactivamos la lógica de frenado
        movementCommands.EnableStopLogic(false);

        // Aseguramos que el agente quede listo para el siguiente estado
        movementCommands.ResumeMovement(enemyData.chaseSpeed, enemyData.normalAcceleration);
    }
    #endregion
}
