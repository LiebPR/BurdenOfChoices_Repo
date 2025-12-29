using UnityEngine;

public class AlertState : MonoBehaviour, IEnemyState
{
    [SerializeField] EnemyData enemyData;

    #region Internal States
    Vector3 alertPoint; // último punto donde oyó algo
    float waitTimer; // temporizador interno
    float waitDuration; // duración según si el jugador corre o camina
    float threshold = 2f; // precisión del giro en grados

    bool waitingPhase;
    bool rotatingPhase;
    bool movingPhase;

    bool hasReachedAlertPoint; // bandera para verificar si el enemigo ha llegado al último punto
    #endregion

    #region References
    EnemyFSM fsm;
    EnemyMovementCommands movementCommands;
    #endregion

    public void Initialize(EnemyFSM enemyFsm, EnemyMovementCommands movement)
    {
        fsm = enemyFsm;
        movementCommands = movement;
    }

    #region State Flow
    public void Enter()
    {
        // Se detiene al entrar en el estado de alerta
        movementCommands.ResumeMovement(0f, enemyData.breackAcceleration);
        movementCommands.ResetRotation();

        waitTimer = 0f;
        waitingPhase = true;
        rotatingPhase = false;
        movingPhase = false;

        hasReachedAlertPoint = false; // resetear la bandera

        // Seguridad, asigna un valor por defecto
        if (waitDuration <= 0)
            waitDuration = enemyData.hearingDelayWalk;
    }

    public void Handle()
    {
        // Fase de espera
        if (waitingPhase)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitDuration)
            {
                waitingPhase = false;
                rotatingPhase = true;
            }
            else
            {
                return;
            }
        }

        // Fase de rotación
        if (rotatingPhase)
        {
            Vector3 dir = alertPoint - movementCommands.Transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
            {
                movementCommands.RotateTowards(alertPoint, enemyData.rotationStiffness, enemyData.rotationDamping);
            }

            float angle = Vector3.Angle(movementCommands.Transform.forward, dir.normalized);

            if (angle <= threshold)
            {
                rotatingPhase = false;
                movingPhase = true;

                movementCommands.ResumeMovement(enemyData.patrolSpeed, enemyData.normalAcceleration);
                movementCommands.SetMoveTarget(alertPoint, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);
            }
        }

        // Fase de movimiento
        if (movingPhase)
        {
            movementCommands.SetMoveTarget(alertPoint, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);

            float dist = Vector3.Distance(movementCommands.Transform.position, alertPoint);

            // Comprobamos si el enemigo ha llegado al punto asignado
            if (dist <= 0.5f)
            {
                hasReachedAlertPoint = true;
                fsm.OnIdle(); // Si llega al punto, pasa al estado Idle o realiza la acción correspondiente.
            }
            else
            {
                if (hasReachedAlertPoint)
                {
                    hasReachedAlertPoint = false; // Aseguramos que solo se marque como "llegado" una vez
                }
            }
        }
    }

    public void Exit()
    {
        movementCommands.ResetRotation();
    }
    #endregion

    #region External API
    // Asigna el último punto escuchado
    public void SetAlertPoint(Vector3 point)
    {
        if (hasReachedAlertPoint)
        {
            alertPoint = point; // Solo actualiza si el enemigo ha llegado al punto actual
        }
    }

    // Ajusta la duración de espera dependiendo de si el jugador estaba corriendo o caminando
    public void SetIsPlayerRunning(bool running)
    {
        waitDuration = running ? enemyData.hearingDelayRun : enemyData.hearingDelayWalk;
    }
    #endregion
}
