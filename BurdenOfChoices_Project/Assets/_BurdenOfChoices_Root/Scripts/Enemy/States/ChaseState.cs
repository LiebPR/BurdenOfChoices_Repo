using System;
using UnityEngine;

public class ChaseState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    string stopOwnerId;

    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    VisionSystem visionSystem;

    private void Awake()
    {
        stopOwnerId = Guid.NewGuid().ToString();
    }
    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands commands, VisionSystem vision)
    {
        fsm = enemyFsm;
        movementCommands = commands;
        visionSystem = vision;
    }

    public void Enter() 
    {
        // Limpiamos cualquier bloqueo previo pero SIN forzar otros propietarios (no force)
        movementCommands.ResumeMovement(stopOwnerId, enemyData.chaseSpeed, enemyData.normalAcceleration, force: true);
        movementCommands.ResetAngularVelocity();
        movementCommands.ResetDestination();
    }

    public void Handle()
    {
        if (visionSystem == null || visionSystem.Target == null) return;

        Vector3 targetPos = visionSystem.Target.position;

        //Persecucion constante
        movementCommands.ResumeMovement(stopOwnerId, enemyData.chaseSpeed, enemyData.normalAcceleration);
        movementCommands.MoveTo(targetPos, enemyData.chaseSpeed, enemyData.destinationUpdateThreshold, enemyData.rotationStiffness, enemyData.rotationDamping);

        movementCommands.RotateTowards(targetPos, enemyData.rotationChaseStiffness, enemyData.rotationChaseDamping);
    }

    public void Exit() 
    {
        // Al salir liberamos control (otros estados pueden forzar reanudar si lo requieren)
        movementCommands.ResumeMovement(stopOwnerId, enemyData.chaseSpeed, enemyData.normalAcceleration, force: true);
    }
}
