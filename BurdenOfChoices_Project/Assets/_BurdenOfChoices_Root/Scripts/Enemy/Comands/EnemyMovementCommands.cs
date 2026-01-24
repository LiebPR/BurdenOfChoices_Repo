using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyMovementCommands;
/// Módulo de ejecución de movimiento del enemigo. 
/// 
/// Centaliza navegación, rotación manual, frenado progresivo
/// y transición entre modo cinemático y físico.
/// 
/// No contiene lógica de estados ni decisiones de comportamiento.
/// </summary>
public class EnemyMovementCommands 
{
    #region Internal States
    Vector3 angularVelocity; //integrador angular para rotación suavizada
    Vector3 lastSetDestination = Vector3.positiveInfinity; //cache de destino para evitar SetDestination redundantes

    float softStopDistance; //distancia de frenado progresivo
    float hardStopDistance; //distancia de parada total
    bool stopControlEnabled; //control de activación del frenado
    #endregion

    #region References
    NavMeshAgent agent;
    Transform transform;
    Rigidbody rb;
    #endregion

    #region Getter
    public Transform Transform => transform; //exposición controlada del Transform
    #endregion

    /// <summary>
    /// Inicializa el controlador y desactiva la rotación automática del NavMEshAgent.
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

    #region Movement
    /// <summary>
    /// Ejecuta movimiento hacia un destino con control de velocidad.
    /// </summary>
    public void SetMoveTarget(Vector3 position, float speed, float updateThreshold)
    {
        if (agent == null) return;

        //Actualiza destino solo si cambió lo suficiente
        UpdateDestinationIfNeeded(position, updateThreshold);

        //Aplica parámetros cinemáticos
        agent.speed = speed;
        agent.isStopped = false;    
    }

    /// <summary>
    /// Punto de extensión para lógica futura de movimiento.
    /// </summary>
    public void UpdateMovement(){ }

    /// <summary>
    /// Evita recalcular path si el destino no cambió significativamente.
    /// </summary>
    void UpdateDestinationIfNeeded(Vector3 target, float threshold)
    {
        float sqr = threshold * threshold;

        if (lastSetDestination == Vector3.positiveInfinity || Vector3.SqrMagnitude(lastSetDestination - target) > sqr)
        {
            agent.SetDestination(target);
            lastSetDestination = target;
        }
    }
    #endregion

    #region Aligment
    /// <summary>
    /// Devuelve el ángulo plano (en grados) entre el forward actual y el objetivo.
    /// </summary>
    public float GetFlatAngleToTarget(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if(dir.sqrMagnitude < 0.0001f) return 0f;

        return Vector3.Angle(transform.forward, dir.normalized);
    }

    /// <summary>
    /// Penalización de velocidad basada en velocidad angular actual.
    /// </summary>
    public bool IsAlignedTo(Vector3 targetPosition, float maxAngle)
    {
        return GetFlatAngleToTarget(targetPosition) <= maxAngle;
    }
    #endregion

    #region Speed Modifiers
    public float GetAlignmentSpeedFactor(Vector3 targetPosition, float minFactor)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return 1f;

        float dot = Vector3.Dot(transform.forward, dir.normalized);
        float normalized = (dot + 1f) * 0.5f;

        return Mathf.Lerp(minFactor, 1f, normalized);
    }

    public float GetAngularVelocityPenalty(float strength)
    {
        float penalty = Mathf.Clamp01(angularVelocity.magnitude * strength);
        return 1f - penalty;
    }
    #endregion

    #region Rotation
    /// <summary>
    /// Rotación manual suavizada con stiffness y damping.
    /// </summary>
    public void RotateTowards(Vector3 targetPosition, float stiffness, float damping)
    {
        //Cancelación de rotación en estados inválidos
        if (agent == null || !agent.enabled)
        {
            angularVelocity = Vector3.zero;
            return;
        }

        //No rotamos si estamos en modo físico
        if (rb != null && !rb.isKinematic)
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
        Quaternion deltaRot = targetRotation * Quaternion.Inverse(transform.rotation);

        //Convetimos a eje-ángulo
        deltaRot.ToAngleAxis(out float angleDeg, out Vector3 axis);
        if (angleDeg > 180f) angleDeg -= 360f;
        if (float.IsNaN(axis.x)) return;

        axis.Normalize();

        //Integración angular (stiffness)
        float angleRad = angleDeg * Mathf.Deg2Rad;
        angularVelocity += axis * (angleRad * stiffness * Time.deltaTime);

        //Damping exponencial
        angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        //Aplicación de rotación
        float deltaDeg = angularVelocity.magnitude * Mathf.Rad2Deg * Time.deltaTime;
        if (deltaDeg > 0.0001f)
        {
            transform.rotation = Quaternion.AngleAxis(deltaDeg, angularVelocity.normalized) * transform.rotation;
        }
    }

    /// <summary>
    /// Limpia el integrador angular.
    /// </summary>
    public void ResetRotation()
    {
        angularVelocity = Vector3.zero;
    }
    #endregion

    #region Stop Control
    /// <summary>
    /// Define el área de frenado progresivo.
    /// </summary>
    public void ConfigureStopArea(float softStop, float hardStop)
    {
        softStopDistance = softStop;
        hardStopDistance = hardStop;
    }

    /// <summary>
    /// Activa o desactiva la lógica de frenado.
    /// </summary>
    public void EnableStopLogic(bool enabled)
    {
        stopControlEnabled = enabled;
    }

    /// <summary>
    /// Aplica lógica de frenado suave o parada total según distancia al objetivo.
    /// </summary>
    public void UpdateStopControl(float distanceToTarget, float maxSpeed, float acceleration)
    {
        if (!stopControlEnabled || agent == null) return; 

        //HARD STOP
        //Detención inmediata al entrar en zona crítica
        if(distanceToTarget <= hardStopDistance)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        //SOFT STOP
        //reducción progresiva de velocidad al acercarse
        if(distanceToTarget > softStopDistance)
        {
            float t = Mathf.InverseLerp(hardStopDistance, softStopDistance, distanceToTarget);
            agent.speed = Mathf.Lerp(0f, maxSpeed, t);
            agent.acceleration = acceleration * 0.5f;
            return;
        }

        //FULL SPEED
        //Movimiento normal fuera del área de frenado
        agent.speed = maxSpeed;
        agent.acceleration = acceleration;
    }
    #endregion

    #region State Control
    /// <summary>
    /// Detiene el movimiento manteniendo navegación activa.
    /// </summary>
    public void PauseMovement()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
    }

    /// <summary>
    /// Reactiva movimiento cinemático.
    /// </summary>
    public void ResumeMovement(float speed, float acceleration)
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;

        agent.speed = speed;
        agent.acceleration = acceleration;
        agent.isStopped = false;

        ResetInternalPath();
        ResetRotation();
    }

    /// <summary>
    /// Limpia el path interno del agente.
    /// </summary>
    void ResetInternalPath()
    {
        lastSetDestination = Vector3.positiveInfinity;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.isStopped)
        {
            agent.ResetPath();
        }
    }

    /// <summary>
    /// Transición a modo físico (stun / knockback).
    /// </summary>
    public void EnterPhysicalMode()
    {
        if(agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        lastSetDestination = Vector3.positiveInfinity;
        angularVelocity = Vector3.zero;

        if (rb != null)
        {
            rb.isKinematic = false;

            // HARD RESET
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // BLOQUEA ROTACIÓN COMPLETA
            rb.constraints =
                RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationY |
                RigidbodyConstraints.FreezeRotationZ;
        }
    }

    /// <summary>
    /// Retorno a navegación por NavMesh.
    /// </summary>
    public void ExitPhysicalMode(float speed, float acceleration)
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        }

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.speed = speed;
            agent.acceleration = acceleration;
            agent.ResetPath();
        }

        angularVelocity = Vector3.zero;
        lastSetDestination = Vector3.positiveInfinity;
    }
    #endregion
}
