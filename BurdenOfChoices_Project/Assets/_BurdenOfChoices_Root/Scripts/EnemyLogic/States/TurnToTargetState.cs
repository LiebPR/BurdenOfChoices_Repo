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
    EnemyAnimationHandler animatorHandle;


    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands movement, EnemyAnimationHandler enemyAnimation)
    {
        fsm = enemyFsm;
        movementCommands = movement;
        animatorHandle = enemyAnimation;
    }

    #region State Flow
    public void Enter()
    {
        //Reanuda movimeinto para no bloquear NavMeshAgent
        movementCommands.ResumeMovement(enemyData.patrolSpeed, enemyData.breackAcceleration);

        //Animator
        animatorHandle.SetVelocityBody(0f);
        animatorHandle.SetVelocityLegs(0f);
        animatorHandle.SetIsRunningBody(false);
        animatorHandle.SetIsRunningLegs(false);
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

        Vector3 forward = movementCommands.Transform.forward;
        Vector3 dirNormalized = dir.normalized;

        // Producto cruzado para saber el lado del giro
        float crossY = Vector3.Cross(movementCommands.Transform.forward, dir.normalized).y;

        // 0 = Left | 1 = Right
        float turnDir = crossY < 0f ? 0f : 1f;
        animatorHandle.SetTurnDirection(turnDir);
        animatorHandle.SetTurnningBody(true);

        movementCommands.RotateTowards(targetPos, enemyData.turnStiffness, enemyData.turnDamping);

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