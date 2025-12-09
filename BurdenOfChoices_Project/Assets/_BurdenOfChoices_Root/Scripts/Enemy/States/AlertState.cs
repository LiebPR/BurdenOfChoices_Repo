using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

public class AlertState : MonoBehaviour, IEnemyState
{
    //Inspector
    [SerializeField] EnemyData enemyData;

    #region Internal States
    Vector3 alertPoint; //ultimo punto donde oyó algo
    float waitTimer; //temporizador interno
    float waitDuration; //duración según si jugador corre o camina
    float threshold = 2f; //precisión del giro en grados

    bool waitingPhase;
    bool rotatingPhase;
    bool movingPhase;
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

    public void Enter()
    {
        //Se detiene al entrar en el estado de alerta
        movementCommands.ResumeMovement(0f, enemyData.breackAcceleration);
        movementCommands.ResetAngularVelocity();

        waitTimer = 0f;
        waitingPhase = true;
        rotatingPhase = false;
        movingPhase = false;

        //Seguridad, asigna un valor por defecto
        if(waitDuration <= 0)
            waitDuration = enemyData.hearingDelayWalk;
        
    }

    public void Handle()
    {
        //ESPERA
        if (waitingPhase)
        {
            waitTimer += Time.deltaTime;
            if(waitTimer >= waitDuration)
            {
                waitingPhase = false;
                rotatingPhase = true;
            }
            else
            {
                return;
            }
        }

        //ROTAR HACIA EL PUNTO
        if (rotatingPhase)
        {
            Vector3 dir = alertPoint - movementCommands.Transform.position;
            dir.y = 0f;

            if(dir.sqrMagnitude > 0.001f)
            {
                movementCommands.RotateTowards(alertPoint, enemyData.rotationStiffness, enemyData.rotationDamping);
            }

            float angle = Vector3.Angle(movementCommands.Transform.forward, dir.normalized);

            if(angle <= threshold)
            {
                rotatingPhase = false;

                //Activa movimiento hacia el punto del ruido
                movementCommands.ResumeMovement(enemyData.patrolSpeed, enemyData.normalAcceleration);
                movementCommands.MoveTo(alertPoint, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);

                movingPhase = true;
            }
        }

        //IR HACIA EL PUNTO
        if (movingPhase)
        {
            movementCommands.MoveTo(alertPoint, enemyData.patrolSpeed, enemyData.destinationUpdateThreshold);

            float dist = Vector3.Distance(movementCommands.Transform.position, alertPoint);

            if (dist <= 0.5f)
            {
                fsm.OnIdle();
            }

            return;
        }
    }

    public void Exit()
    {
        movementCommands.ResetAngularVelocity();
    }

    #region External API
    //Donde el sistema de percepción asigna el último punto escuchado
    public void SetAlertPoint(Vector3 point)
    {
        alertPoint = point;
    }

    //Según si el jugador caminaba o corría
    public void SetIsPlayerRunning(bool running)
    {
        waitDuration = running ? enemyData.hearingDelayRun : enemyData.hearingDelayWalk;
    }
    #endregion
}
