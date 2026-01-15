using Mono.Cecil.Cil;
using UnityEngine;

public class InvestigateSoundState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region Internal States
    float reactionTimer;
    float inspectTimer;
    float soundMemoryTimer;

    bool hasLockedPoint;

    bool isReacting;
    bool isRotating;
    bool isWaitingAlignment;
    bool isMoving;

    Vector3 lockedInvestigatePoint;
    #endregion

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    VisionSystem vision;
    EnemyPerceptionHandler perception;
    EnemyMotionContext moveContext;
    EnemyAnimationHandler animatorHandle;
    #endregion

    public void Initialize(EnemyFSM enemyFSM, EnemyMovementCommands command, VisionSystem visionSystem, EnemyPerceptionHandler perceptionHandler, EnemyMotionContext move, EnemyAnimationHandler enemyAnimator)
    {
        fsm = enemyFSM;
        movementCommands = command;
        vision = visionSystem;
        perception = perceptionHandler;
        moveContext = move;
        animatorHandle = enemyAnimator;
    }

    public void Enter()
    {
        reactionTimer = enemyData.soundReactionDelay;
        inspectTimer = enemyData.soundInspectTime;
        soundMemoryTimer = enemyData.noiseMemoryTime;

        hasLockedPoint = false;

        isReacting = true;
        isRotating = false;
        isWaitingAlignment = false;
        isMoving = false;   

        movementCommands.PauseMovement();


        animatorHandle.SetHearBody();
        animatorHandle.SetVelocityBody(0f);
        animatorHandle.SetVelocityLegs(0f);
        animatorHandle.SetIsRunningBody(false);
        animatorHandle.SetIsRunningLegs(false);
        animatorHandle.SetTurnningBody(true);

    }

    public void Handle()
    {
        //Reacción / Duda
        if (isReacting)
        {
            reactionTimer -= Time.deltaTime;

            if (perception.IsHearingNoise)
            {

                UpdateTurnDirection(perception.LastTargetPosition);

                movementCommands.RotateTowards(
                    perception.LastTargetPosition,
                    enemyData.rotationSoundStiffness,
                    enemyData.rotationSoundDamping
                );
            }

            if (reactionTimer <= 0f)
            {
                isReacting = false;
                isRotating = true;
            }

            return;
        }

        //Rotación + Memoria
        if (isRotating)
        {
            if (perception.IsHearingNoise)
            {
                soundMemoryTimer = enemyData.noiseMemoryTime;

                UpdateTurnDirection(perception.LastTargetPosition);

                movementCommands.RotateTowards(
                    perception.LastTargetPosition,
                    enemyData.rotationSoundStiffness,
                    enemyData.rotationSoundDamping
                );
            }
            else
            {
                soundMemoryTimer -= Time.deltaTime;
            }

            if (soundMemoryTimer <= 0f && !hasLockedPoint)
            {
                lockedInvestigatePoint = perception.LastTargetPosition;
                hasLockedPoint = true;

                //IMPORTANTE
                perception.ForgetSound();

                isRotating = false;
                isWaitingAlignment = true;
            }

            return;
        }

        //Espera alineación
        if (isWaitingAlignment)
        {
            if (!movementCommands.IsAlignedTo(
                    lockedInvestigatePoint,
                    enemyData.soundTurnAlignmentAngle))
            {
                movementCommands.RotateTowards(
                    lockedInvestigatePoint,
                    enemyData.rotationSoundStiffness,
                    enemyData.rotationSoundDamping
                );
                return;
            }

            animatorHandle.SetTurnningBody(false);

            // Ya está alineado → ahora sí puede moverse
            movementCommands.ResumeMovement(
                enemyData.investigateSpeed,
                enemyData.normalAcceleration
            );

            isWaitingAlignment = false;
            isMoving = true;
        }

        //Movimiento (Sin Rotar)
        if (isMoving)
        {
            animatorHandle.SetVelocityBody(1f);
            animatorHandle.SetVelocityLegs(1f);

            movementCommands.SetMoveTarget(
                lockedInvestigatePoint,
                enemyData.investigateSpeed,
                enemyData.destinationUpdateThreshold
            );

            if (!moveContext.Agent.pathPending &&
                moveContext.Agent.remainingDistance <=
                moveContext.Agent.stoppingDistance + 0.1f)
            {
                inspectTimer -= Time.deltaTime;

                if (inspectTimer <= 0f)
                {
                    fsm.OnPatrol();
                }
            }
        }
    }

    public void Exit()
    {
        movementCommands.ResetRotation();
        animatorHandle.SetTurnningBody(false);
    }

    #region Turn Direction
    void UpdateTurnDirection(Vector3 targetPos)
    {
        Vector3 dir = targetPos - movementCommands.Transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f)
            return;

        float crossY = Vector3.Cross(
            movementCommands.Transform.forward,
            dir.normalized
        ).y;

        float turnDir = crossY < 0f ? 0f : 1f;
        animatorHandle.SetTurnDirection(turnDir);
    }
    #endregion
}
