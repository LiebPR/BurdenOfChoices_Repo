using Mono.Cecil.Cil;
using UnityEngine;

public class InvestigateSoundState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region Internal States
    float reactionTimer;
    float inspectTimer;

    bool isTurning;
    bool isInvestigating;

    bool hasLockedPoint;
    Vector3 lockedInvestigatePoint;
    #endregion

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    VisionSystem vision;
    EnemyPerceptionHandler perception;
    EnemyMotionContext moveContext;
    #endregion

    public void Initialize(EnemyFSM enemyFSM, EnemyMovementCommands command, VisionSystem visionSystem, EnemyPerceptionHandler perceptionHandler, EnemyMotionContext move)
    {
        fsm = enemyFSM;
        movementCommands = command;
        vision = visionSystem;
        perception = perceptionHandler;
        moveContext = move;
    }

    public void Enter()
    {
        reactionTimer = enemyData.soundReactionDelay;
        inspectTimer = enemyData.soundInspectTime;

        hasLockedPoint = false;

        movementCommands.PauseMovement();
    }

    public void Handle()
    {
        Vector3 targetPoint = perception.HasValidSound()
                              ? perception.LastTargetPosition
                              : lockedInvestigatePoint;

        // FASE 1 — REACCIÓN / DUDA
        if (reactionTimer > 0f)
        {
            reactionTimer -= Time.deltaTime;

            if (perception.IsHearingNoise)
            {
                movementCommands.RotateTowards(
                    perception.LastTargetPosition,
                    enemyData.rotationSoundStiffness,
                    enemyData.rotationSoundDamping
                );
            }

            return;
        }

        // FASE 2 — FIJAR PUNTO DE INVESTIGACIÓN UNA SOLA VEZ
        if (!hasLockedPoint && perception.LastTargetPosition != Vector3.zero)
        {
            lockedInvestigatePoint = perception.LastTargetPosition;
            hasLockedPoint = true;

            movementCommands.ResumeMovement(
                enemyData.investigateSpeed,
                enemyData.normalAcceleration
            );
        }

        // FASE 3 — DESPLAZAMIENTO HACIA EL PUNTO FIJADO
        if (hasLockedPoint)
        {
            movementCommands.SetMoveTarget(
                lockedInvestigatePoint,
                enemyData.investigateSpeed,
                enemyData.destinationUpdateThreshold
            );

            if (!moveContext.Agent.pathPending &&
                moveContext.Agent.remainingDistance <= moveContext.Agent.stoppingDistance + 0.1f)
            {
                inspectTimer -= Time.deltaTime;

                if (inspectTimer <= 0f)
                {
                    fsm.OnPatrol();
                }
            }
        }

        // FASE 4 — GIRAR HACIA SONIDO SI SIGUE ESTANDO ACTIVO
        if (perception.IsHearingNoise)
        {
            movementCommands.RotateTowards(
                perception.LastTargetPosition,
                enemyData.rotationSoundStiffness,
                enemyData.rotationSoundDamping
            );
        }
    }

    public void Exit()
    {
        movementCommands.ResetRotation();
    }
}
