using System;
using UnityEngine;

public class TurnToTargetState : MonoBehaviour, IEnemyState
{
    //Inspector
    [SerializeField] EnemyData enemyData;

    //Internal States
    Transform target;
    float threshold = 2f; //grados para considerar que ya giró

    //References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;


    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands movement)
    {
        fsm = enemyFsm;
        movementCommands = movement;
    }

    #region State Flow
    public void Enter()
    {
        //Reanuda movimeinto para no bloquear NavMeshAgent
        movementCommands.ResumeMovement(enemyData.patrolSpeed, enemyData.breackAcceleration);
    }

    public void Handle()
    {
        if (target == null)
        {
            //Si no hay target, no tiene sentido seguir aquí
            fsm.OnPatrol();
            return;
        }

        Vector3 targetPos = target.position;
        Vector3 dir = targetPos - movementCommands.Transform.position;
        dir.y = 0;

        if (dir.sqrMagnitude < 0.0001f)
        {
            fsm.OnPatrol();
            return;
        }

        movementCommands.RotateTowards(targetPos, enemyData.rotationStiffness, enemyData.rotationDamping);

        float angle = Vector3.Angle(movementCommands.Transform.forward, dir.normalized);
        if (angle < threshold)
        {
            fsm.OnPatrol();
        }
    }

    public void Exit()
    {
        movementCommands.ResetRotation();
    }
    #endregion

    #region Utilities
    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void SetTargetPoint(Vector3 point)
    {
        //Si quieres girar hacia un punto estático, puedes implementarlo luego
    }
    #endregion
}