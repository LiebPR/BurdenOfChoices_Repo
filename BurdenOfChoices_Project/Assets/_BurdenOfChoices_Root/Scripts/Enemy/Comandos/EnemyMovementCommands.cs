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

    string stopOwnerId = null; //quien controla la fase de frenado 
    bool isFullyStopped = false; //indicador interno parada total
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

        //Actualizar destino solo si cambió suficiente
        float sqThershold = updateThreshold * updateThreshold;
        if(lastSetDestination == Vector3.positiveInfinity || Vector3.SqrMagnitude(lastSetDestination - targetPosition) > sqThershold)
        {
            agent.SetDestination(targetPosition);
            lastSetDestination = targetPosition;
            destinationLocked = false;
        }

        //Calculamos un factor de velocidad influenciado por la rotación para acompasar movimiento y giro.
        float finalSpeed = speed;
        if(rotationStiffness > 0f)
        {
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;
            if(toTarget.sqrMagnitude > 0.0001f)
            {
                Vector3 dir = toTarget.normalized;
                float alignmet = Vector3.Dot(transform.forward, dir);

                //Responsiveness entre 0 y 1
                float responsivness = rotationStiffness / Mathf.Max(rotationStiffness + rotationDamping, 0.0001f);
                responsivness = Mathf.Clamp01(responsivness);

                float minFactor = Mathf.Lerp(0.4f, 0.8f, responsivness);

                float align01 = (alignmet + 1f) * 0.5f; //tunable

                float angVelMag = angularVelocity.magnitude;
                float angPenalty = Mathf.Clamp01(angVelMag * 0.5f);

                float speedFactor = Mathf.Lerp(minFactor, 1f, align01);
                speedFactor *= (1f - 0.5f * angPenalty); //penalización por rotación dinámica

                finalSpeed = speed * Mathf.Clamp01(speedFactor);
             }
        }
        //Aplicamos la velocidad objetivo cada frame (evita lentitud en reinicio
        agent.speed = speed;

        //Si el agente había sido detenido, aseguramos que vuelva a caminar
        agent.isStopped = false;
    }

    public void ResetDestination()
    {
        lastSetDestination = Vector3.positiveInfinity;
        destinationLocked = false;
    }
    #endregion

    #region Stopping (Frenado progresivo)
    //Inicializa la fase de frenado. minStopDistance/maxStopDistance son radios, breakeAcceleration es la aceleración de frenado
    public void StartStopping(string ownerId, float minStopDistance, float maxStopDistance, float brakeAcceleration)
    {
        //si hay otro propietario y no es el mismo, no iniciamos (a menos que se fuerce externamente)
        if (stopOwnerId != null && stopOwnerId != ownerId) return;

        stopOwnerId = ownerId;
        isFullyStopped = false;

        currentStopDistance = Random.Range(minStopDistance, maxStopDistance);
        stopTimer = 0f;
        destinationLocked = false;
        lastSetDestination = Vector3.positiveInfinity;

        if(agent != null)
        {
            agent.acceleration = brakeAcceleration;

            //No paramos al agente.
            agent.isStopped = false;
        }
    }

    //Actualiza la fase de frenado. Devuelve true si todavía está en fase de frenado
    public bool UpdateStopping(string ownerId, Vector3 targetPosition, float chaseSpeed, float stopTransitionTime)
    {
        //Si no eres el propietario activo, no haces nada
        if(stopOwnerId == null || stopOwnerId != ownerId) return false;
        if(agent == null) return false;

        float distance = Vector3.Distance(transform.position, targetPosition);

        //Frenado progresivo
        if(distance < currentStopDistance)
        {
            stopTimer += Time.deltaTime;
            float t = Mathf.Clamp01(stopTimer / stopTransitionTime);

            //Frenado progresivo
            agent.speed = Mathf.Lerp(chaseSpeed, 0f, t);

            //Actualizar destino miientras se reduce velocidad
            if (t < 1f)
            {
                //Actualizamos destino mientras reducimos velocidad
                if(lastSetDestination == Vector3.positiveInfinity || Vector3.SqrMagnitude(lastSetDestination - targetPosition) > DESTINATION_EPS)
                {
                    agent.SetDestination(targetPosition);
                    lastSetDestination = targetPosition;
                    destinationLocked = false;
                }
                isFullyStopped = true;
            }
            else
            {
                //Al llegar a velocidad 0 fijamos la meta en la posición acual una única vez
                if(!destinationLocked || Vector3.SqrMagnitude(lastSetDestination - transform.position) > Mathf.Epsilon)
                {
                    agent.SetDestination(transform.position);
                    lastSetDestination = transform.position;
                    destinationLocked = true;
                }

                //Marcamos parada completa internamente y aplicamos agent.isStopped true
                if (!isFullyStopped)
                {
                    agent.isStopped = true;
                    isFullyStopped = true;
                }
            }
            return true; //aún frenado
        }

        //Fuera del radio de parada: terminamos la fase de freando
        //Liberamos propietario para que otros estados puedan tomar control si procede
        stopOwnerId = null;
        isFullyStopped = false;
        return false; //fuera del radio de parada, no paramos
    }

    //Reanuda la persecución: reestablece aceleración/velocidad y desbloquea destino para permitir SetDestination inmediato.
    public void ResumeMovement(string ownerId, float targetSpeed, float normalAcceleration, bool force = false)
    {
        //Sólo el propietario o u caller con force podrá reanudar
        if (!force && stopOwnerId != null && stopOwnerId != ownerId) return;

        //Liberamos control del stop
        stopOwnerId = null;
        isFullyStopped = false;

        if (agent == null) return;

        //Restauramos parámetros 
        agent.speed = targetSpeed;
        agent.acceleration = normalAcceleration;
        agent.isStopped = false;

        destinationLocked = false;
        lastSetDestination = Vector3.positiveInfinity;
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
}
