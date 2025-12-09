using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

/// <summary>
/// EnemyMovementCommands: Se encarga de generar los comandos de movimiento general del enemigo.
/// </summary>
public class EnemyMovementCommands 
{
    #region Internal States
    //Rotación suavizada
    Vector3 angularVelocity;

    float stopTimer;
    float currentStopDistance;

    //Control de destino
    Vector3 lastSetDestination = Vector3.positiveInfinity;
    bool destinationLocked;

    float stopStartDistance;
    float stopHardDistance;
    bool stopLogicEnabled;
    #endregion

    #region References
    NavMeshAgent agent;
    Transform transform;
    #endregion

    public Transform Transform => transform;
    const float DESTINATION_EPS = 0.01f;

    public EnemyMovementCommands(NavMeshAgent agent, Transform transform)
    {
        this.agent = agent;
        this.transform = transform;

        //Desactivar la rotación automática del NavMeshAgent para evitar conflictos
        if(this.agent != null) this.agent.updateRotation = false;
    }

    #region MoveToDestination
    public void MoveTo(Vector3 targetPosition, float speed, float updateThreshold, float rotationStiffness = 0f, float rotationDamping = 0f)
    {
        if (agent == null) return;

        // Actualizar destino solo si cambió suficiente
        float sqThreshold = updateThreshold * updateThreshold;
        if (lastSetDestination == Vector3.positiveInfinity || Vector3.SqrMagnitude(lastSetDestination - targetPosition) > sqThreshold)
        {
            agent.SetDestination(targetPosition);
            lastSetDestination = targetPosition;
            destinationLocked = false;
        }

        // Lógica de rotación
        RotateTowards(targetPosition, rotationStiffness, rotationDamping);

        // Calculamos un factor de velocidad influenciado por la rotación
        float finalSpeed = speed;
        if (rotationStiffness > 0f)
        {
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;
                float alignment = Vector3.Dot(transform.forward, dir);

                float responsiveness = rotationStiffness / Mathf.Max(rotationStiffness + rotationDamping, 0.0001f);
                responsiveness = Mathf.Clamp01(responsiveness);

                float minFactor = Mathf.Lerp(0.4f, 0.8f, responsiveness);

                float align01 = (alignment + 1f) * 0.5f; // Tunable

                float angVelMag = angularVelocity.magnitude;
                float angPenalty = Mathf.Clamp01(angVelMag * 0.5f);

                float speedFactor = Mathf.Lerp(minFactor, 1f, align01);
                speedFactor *= (1f - 0.5f * angPenalty); // Penalización por rotación dinámica

                finalSpeed = speed * Mathf.Clamp01(speedFactor);
            }
        }

        // Aplicamos la velocidad objetivo cada frame (evita lentitud en reinicio)
        agent.speed = finalSpeed;

        // Si el agente había sido detenido, aseguramos que vuelva a caminar
        agent.isStopped = false;
    }
    #endregion

    #region Stop Logic (Hard / Soft Stop)
    public void ConfigureStopArea(float startDist, float hardDist)
    {
        stopStartDistance = startDist;
        stopHardDistance = hardDist;
    }

    public void EnableStopLogic(bool enabled)
    {
        stopLogicEnabled = enabled;
    }

    public void ApplyStopLogic(float distanceToTarget, float speed, float acceleration)
    {
        if (!stopLogicEnabled || agent == null) return; 

        //HARD STOP
        if(distanceToTarget <= stopHardDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        //SOFT STOP
        if(distanceToTarget > stopStartDistance)
        {
            agent.speed = Mathf.Lerp(0f, speed, Mathf.InverseLerp(stopHardDistance, stopStartDistance, distanceToTarget));

            agent.acceleration = acceleration * 0.5f;
            return;
        }

        //FULL SPEED
        agent.speed = speed;
        agent.acceleration = acceleration;
    }
    #endregion

    #region Rotation (Controlada)
    //Rotación suavizada basada en un integrador angular.
    //stiffnes controla la fuerza de corrección; damping la disipación
    public void RotateTowards(Vector3 targetPosition, float stiffness, float damping)
    {
        if(transform == null) return;

        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        Quaternion deltaRot = targetRotation * Quaternion.Inverse(transform.rotation);

        deltaRot.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;

        if (float.IsNaN(axis.x)) return;

        axis.Normalize();

        //Integrador angular
        float angleRad = angleDeg * Mathf.Deg2Rad;
        angularVelocity += axis * (angleRad * stiffness * Time.deltaTime);
        
        //Aplicamos damping exponencial
        angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        //Rotamos en base a la magnitud del integrador
        float rotateAmountDeg = angularVelocity.magnitude * Mathf.Rad2Deg * Time.deltaTime;
        if(rotateAmountDeg > 0.0001f)
        {
            transform.rotation = Quaternion.AngleAxis(rotateAmountDeg, angularVelocity.normalized) * transform.rotation;
        }
    }

    public void ResetAngularVelocity()
    {
        angularVelocity = Vector3.zero;
    }
    #endregion

    #region Reset
    //Resetea el destino del agente y desbloquea el control para permitir SetDestination inmediato.
    public void ResetDestination()
    {
        lastSetDestination = Vector3.positiveInfinity;
        destinationLocked = false;

        if(agent !=  null && !agent.isStopped)
        {
            //Forzamos que NavMeshAgent acepte nuevo destino
            agent.ResetPath();
        }
    }

    //Reanuda el movimiento del agente, restaurando velocidad, aceleración y desbloqueando el destino
    public void ResumeMovement(float targetSpeed, float normalAcceleration)
    {

        if(agent != null)
        {
            //Restauramos parámetros
            agent.speed = targetSpeed;
            agent.acceleration = normalAcceleration;
            agent.isStopped = false;

            //Reiniciamos destino para aceptar nuevo SetDestination
            ResetDestination();
        }

        //Restauramos velocidad angular para rotación
        ResetAngularVelocity();
    }
    #endregion
}
