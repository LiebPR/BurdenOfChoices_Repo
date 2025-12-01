using System;
using UnityEngine;

public class TurnToTargetState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;

    Vector3 targetPos;
    float threshold = 2f; //grados para considerar que ya giró

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands movement)
    {
        fsm = enemyFsm;
        movementCommands = movement;
    }

    public void Enter()
    {

    }

    public void Handle()
    {
        Vector3 dir = targetPos - movementCommands.Transform.position;
        dir.y = 0;

        if(dir.sqrMagnitude > 0.0001f)
        {
            movementCommands.RotateTowards(targetPos, enemyData.rotationStiffness, enemyData.rotationDamping);
        }

        float angle = Vector3.Angle(movementCommands.Transform.forward, dir.normalized);
        if(angle < threshold)
        {
            fsm.OnPatrol();
        }
    }

    public void Exit()
    {
        //Reset del integrador de rotación
        movementCommands.ResetAngularVelocity();
    }

    #region Utilities
    public void SetTarget(Transform t)
    {
        targetPos = t.position;
    }
    #endregion
}
