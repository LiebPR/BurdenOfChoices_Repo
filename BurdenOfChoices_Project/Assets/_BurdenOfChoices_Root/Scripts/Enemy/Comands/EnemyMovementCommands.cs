using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyMovementCommands;
/// Esté modulo se encarga de generar, controlar y modular los comandos de movimiento del enemigo.
/// Su responsabilidad incluye: navegación por NavMesh, control de rotación manual suavizada, 
/// lógica de frenado (hard/soft stop), y sincronización entre movimiento cinemático y físico.
/// No decide comportamientos, solo ejecuta órdenes de movimiento.
/// </summary>
public class EnemyMovementCommands 
{
    #region Internal States
    Vector3 angularVelocity; // velocidad angular acumulada usada como integrador para suavizar la rotación

    Vector3 lastSetDestination = Vector3.positiveInfinity; // último destino asignado al NAvMeshAgent (para evitar SetDestination innecesarios)

    // Distancias de control de parada
    float stopStartDistance;
    float stopHardDistance;
    bool stopLogicEnabled;
    #endregion

    #region References
    NavMeshAgent agent;
    Transform transform;
    Rigidbody rb;
    #endregion

    public Transform Transform => transform; //exposición controlada del Transform

    /// <summary>
    /// Inicializa el sistema de movimeinto y desactiva la rotación automátomática del NavMeshAgent.
    /// </summary>
    public EnemyMovementCommands(NavMeshAgent agent, Transform transform)
    {
        this.agent = agent;
        this.transform = transform;

        // Intentamos obtener Rigidbody para detectar estados físicos
        rb = transform.GetComponent<Rigidbody>();

        //Evita que el NavMeshAgent controle la rotación automáticamente
        if (this.agent != null) this.agent.updateRotation = false;
    }

    #region MoveToDestination
    /// <summary>
    /// Este método se encarga de mover al enemigo hacia un destino específico,
    /// controlando velocidad, actualizada de destino y rotación suavizada.
    /// </summary>
    public void MoveTo(Vector3 targetPosition, float speed, float updateThreshold, float rotationStiffness = 0f, float rotationDamping = 0f)
    {
        //Seguridad: sin agente no hay movimeinto
        if (agent == null) return;

        //ACTUALIZACIÓN DE DESTINO
        //Evita llamr a SetDestination cada frame si el objetivo no cambió lo suficiente
        float sqThreshold = updateThreshold * updateThreshold;
        if (lastSetDestination == Vector3.positiveInfinity || Vector3.SqrMagnitude(lastSetDestination - targetPosition) > sqThreshold)
        {
            agent.SetDestination(targetPosition);
            lastSetDestination = targetPosition;
        }

        //ROTACIÓN
        //Orienta al enemigo hacia el objetivo usando rotación física suavizada
        RotateTowards(targetPosition, rotationStiffness, rotationDamping);

        //MODULACIÓN DE VELOCIDAD SEGÚN ROTACIÓN
        //Reduce la velocidad cuando el enemigo no está alineado con el objetivo
        float finalSpeed = speed;

        if (rotationStiffness > 0f)
        {
            //Dirección plana hacia el objetivo
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                //Dirección normalizada
                Vector3 dir = toTarget.normalized;

                //Grado de alineación frontal (-1 espalda, 1 frente)
                float alignment = Vector3.Dot(transform.forward, dir);

                //Cálculo de respuesta de rotación (stiffness vs damping)
                float responsiveness = rotationStiffness / Mathf.Max(rotationStiffness + rotationDamping, 0.0001f);
                responsiveness = Mathf.Clamp01(responsiveness);

                //Velocidad mínima permitida según control rotacional
                float minFactor = Mathf.Lerp(0.4f, 0.8f, responsiveness);

                //Normalizamos alineación a rango 0-1
                float align01 = (alignment + 1f) * 0.5f;

                //Pensalización por velocidad angular (giros bruscos)
                float angVelMag = angularVelocity.magnitude;
                float angPenalty = Mathf.Clamp01(angVelMag * 0.5f);

                //Factor final de velocidad
                float speedFactor = Mathf.Lerp(minFactor, 1f, align01);
                speedFactor *= (1f - 0.5f * angPenalty); // Penalización por rotación dinámica

                finalSpeed = speed * Mathf.Clamp01(speedFactor);
            }
        }

        //APLICACIÖN DE VELOCIDAD
        //Se asigna cada frame para evitar estados lentos al reanudar movimiento
        agent.speed = finalSpeed;

        //Garantiza que el agente esté activo
        agent.isStopped = false;
    }
    #endregion

    #region Stop Logic (Hard / Soft Stop)
    /// <summary>
    /// Configura las distancias de inicio y de frenado y parada total.
    /// </summary>
    public void ConfigureStopArea(float startDist, float hardDist)
    {
        stopStartDistance = startDist;
        stopHardDistance = hardDist;
    }

    /// <summary>
    /// Activa o desactiva la lógica de parada progresiva.
    /// </summary>
    public void EnableStopLogic(bool enabled)
    {
        stopLogicEnabled = enabled;
    }

    /// <summary>
    /// Aplica lógica de frenado basada en la distancía al objetivo.
    /// Incluye hard stop, ssoft stop y velocidad normal.
    /// </summary>
    public void ApplyStopLogic(float distanceToTarget, float speed, float acceleration)
    {
        if (!stopLogicEnabled || agent == null) return; 

        //HARD STOP
        //Detención inmediata al entrar en zona crítica
        if(distanceToTarget <= stopHardDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        //SOFT STOP
        //reducción progresiva de velocidad al acercarse
        if(distanceToTarget > stopStartDistance)
        {
            agent.speed = Mathf.Lerp(0f, speed, Mathf.InverseLerp(stopHardDistance, stopStartDistance, distanceToTarget));

            //Aceleración reducida para suavizar el frenado
            agent.acceleration = acceleration * 0.5f;
            return;
        }

        //FULL SPEED
        //Movimiento normal fuera del área de frenado
        agent.speed = speed;
        agent.acceleration = acceleration;
    }
    #endregion

    #region Rotation (Controlada)
    /// <summary>
    /// Este método encarga de rotar suavemente al enemigo hacia un objetivo.
    /// Usa un integrador angular con stiffness y damping para lograr naturalidad.
    /// </summary>
    public void RotateTowards(Vector3 targetPosition, float stiffness, float damping)
    {
        //Cancelamos rotación si el agente no puede moverse
        if (agent == null || !agent.enabled || agent.isStopped)
        {
            angularVelocity = Vector3.zero;
            return;
        }

        //Si el rigidbody está activo, estamos en modo físico (knockback, stun)
        if(rb != null && !rb.isKinematic)
        {
            angularVelocity = Vector3.zero;
            return;
        }

        if (transform == null) return;

        //Dirección plana hacia el objetivo
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        //Rotación objetivo
        Quaternion targetRotation = Quaternion.LookRotation(dir);

        //Diferencia entre rotación actual y deseada
        Quaternion deltaRot = targetRotation * Quaternion.Inverse(transform.rotation);

        //Convetimos a eje-ángulo
        deltaRot.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;

        if (float.IsNaN(axis.x)) return;

        axis.Normalize();

        //INTEGRADOR ANGULAR
        //Acumulamos velocidad angular función del error
        float angleRad = angleDeg * Mathf.Deg2Rad;
        angularVelocity += axis * (angleRad * stiffness * Time.deltaTime);

        //DAMPING
        //Disipación exponencial para suavizar la rotación
        angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        //APLICACIÓN DE ROTACIÓN
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
    /// <summary>
    /// Resetea el destino dela agente para permitir un SetDestination inmediato.
    /// </summary>
    public void ResetDestination()
    {
        lastSetDestination = Vector3.positiveInfinity;

        if(agent !=  null && !agent.isStopped)
        {
            //Fuerza al NavMeshAgent a aceptar un nuevo camino
            agent.ResetPath();
        }
    }

    /// <summary>
    /// Reanuda el movimiento normal del enemigo tras una interrupción.
    /// </summary>
    public void ResumeMovement(float targetSpeed, float normalAcceleration)
    {

        if(agent != null)
        {
            //Restauramos parámetros cinemáticos
            agent.speed = targetSpeed;
            agent.acceleration = normalAcceleration;
            agent.isStopped = false;

            ResetDestination();
        }

        //Reiniciamos rotación
        ResetAngularVelocity();
    }
    #endregion

    #region StopMovement
    /// <summary>
    /// Detiene completamente al enemigo: 
    /// navegación, rotación y estado cinemático´. 
    /// </summary>
    public void StopEnemy()
    {
        if (agent != null)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            agent.acceleration = 0f;
            agent.velocity = Vector3.zero;
            agent.ResetPath();
            agent.enabled = false;
        }

        lastSetDestination = Vector3.positiveInfinity;

        //CRÍTICO: limpiamos rotación acumulada
        angularVelocity = Vector3.zero;

        if (rb != null)
        {
            //Pasamos a modo físico
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            //Evita vuelcos laterales
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }

    /// <summary>
    /// Restaura al enemigo a su estado de navegación normal.
    /// </summary>
    public void RestoreEnemy(float targetSpeed, float normalAcceleration)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = targetSpeed;
            agent.acceleration = normalAcceleration;
            agent.ResetPath();
        }

        angularVelocity = Vector3.zero;
        lastSetDestination = Vector3.positiveInfinity;
    }
    #endregion
}
