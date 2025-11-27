using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMoveController : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] EnemyData enemyData;
    [SerializeField] List<Transform> patrolPoints = new List<Transform>(); //lista de puntos de patrulla
    #endregion

    #region Internal States
    int currentPatrolIndex = 0; //índice del punto de patrulla actual
    
    float idleTimer = 0f; //temporizador de Idle
    float stopTimer = 0f; //temporizador de parada

    float currentStopDistance; //distancia de parada actual

    bool idleInProgress; //si el enemigo está detenido en Idle
    bool reachedPoint; //marca si llegó al punto de patrulla
    bool isStopping; //si el enemigo está deteniéndose
    bool destinationLocked = false;

    // Cache para evitar SetDestination cada frame
    Vector3 lastSetDestination = Vector3.positiveInfinity;
    Vector3 angularVelocity; //velocidad angular suavizada (dampeada)
    #endregion

    #region Events
    public event Action OnIdleStarted; //Comienza idle
    public event Action OnIdleEnded; //Termina idle
    #endregion

    #region References
    EnemyFSM fsm;
    NavMeshAgent agent;
    VisionSystem visionSystem;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        agent = GetComponent<NavMeshAgent>();
        visionSystem = GetComponentInChildren<VisionSystem>();
    }

    private void FixedUpdate()
    {
        StateHandlersControl();
    }

    void StateHandlersControl()
    {
        switch (fsm.CurrentState)
        {
            case EnemyState.Patrol:
                HandlePatrol();
                break;
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
        }
    }

    #region Patrol
    //Guiar al enemigo entre diferentes puntos de patrulla.
    void HandlePatrol()
    {
        if (patrolPoints.Count == 0) return; //si no hay puntos de patrulla, no ejecuta

        if (!reachedPoint)
        {
            Transform target = patrolPoints[currentPatrolIndex];
            agent.speed = enemyData.patrolSpeed;
            TrySetDestinationIfNeeded(target.position);      

            if(!agent.pathPending && agent.remainingDistance < 0.1f)
            {
                reachedPoint = true;
                idleInProgress = true;
                idleTimer = enemyData.idleTime;
                agent.ResetPath();
                OnIdleStarted?.Invoke();
            }
        }
    }
    #endregion

    #region Idle
    void HandleIdle()
    {
        if(!idleInProgress) return; //si no esta en idle, no ejecuta

        idleTimer -= Time.deltaTime;

        if(idleTimer <= 0)
        {
            idleInProgress = false;
            reachedPoint = false;
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            OnIdleEnded?.Invoke();
        }
    }
    #endregion

    #region Chase
    void HandleChase()
    {
        if(visionSystem == null || visionSystem.Target == null) return;

        float distance = Vector3.Distance(transform.position, visionSystem.Target.position);

        //Velocidad base de Chase
        agent.speed = enemyData.chaseSpeed;
        if (!visionSystem.IsPlayerInStopArea)
        {
            //Fuera del área de parada: aseguramos que seguimos persiguiendo
            ResumeChase();
            TrySetDestinationIfNeeded(visionSystem.Target.position);
        }
        else
        {
            //Dentro del área de parada -> gestionamos la fase de frenado
            if(!isStopping)
                StartStoppingPhase();
            UpdateStoppingPhase(distance);
        }

        //Rotación hacia el target (se mantiene fuera de la lógica de (frenado / movimiento)
        RotateTowadrsTarget();
    }

    //Inicializa la fase de frenado
    void StartStoppingPhase()
    {
        currentStopDistance = UnityEngine.Random.Range(enemyData.minStopDistance, enemyData.maxStopDistance);
        isStopping = true;
        stopTimer = 0f;

        // desbloqueamos destino para permitir la primera asignación durante el stop
        destinationLocked = false;
        lastSetDestination = Vector3.positiveInfinity;

        //Aplicar aceleración de frenado al agente
        if(agent != null && enemyData != null)
        {
            agent.acceleration = enemyData.breackAcceleration;
        }
    }

    //Actualiza la fase de frenado; si el objetivo se aleja reanuda la persecución
    void UpdateStoppingPhase(float distance)
    {
        if(distance < currentStopDistance)
        {
            stopTimer += Time.deltaTime;
            float t = Mathf.Clamp01(stopTimer / enemyData.stopTransitionTime);

            //Frenado progresivo
            float newSpeed = Mathf.Lerp(enemyData.chaseSpeed, 0f, t);
            agent.speed = newSpeed;

            if(t < 1f)
            {
                //Mientras estemos reduciendo velocidad seguimos intentando acercarnos al target
                //pero solo llamamos a SetDestination si la posición del target cambió suficiente
                TrySetDestinationIfNeeded(visionSystem.Target.position);
                destinationLocked = false;
            }
            else
            {
                // Al llegar a 0 fijamos la meta en la posición actual solo una vez para evitar recálculos continuos
                if (!destinationLocked || Vector3.SqrMagnitude(lastSetDestination - transform.position) > Mathf.Epsilon)
                {
                    TrySetDestinationIfNeeded(transform.position);
                    destinationLocked = true;
                }
            }
        }
        else
        {
            //objetivo fuera del radio de parada: reanudar presecución
            ResumeChase();
            TrySetDestinationIfNeeded(visionSystem.Target.position);
        }
    }

    //Restablece estados y parámetros para reanudar la persecución
    void ResumeChase()
    {
        isStopping = false;
        stopTimer = 0f;

        //Suavizamos la transición de velocidad hacia chaseSpeed
        agent.speed = Mathf.Lerp(agent.speed, enemyData.chaseSpeed, Time.deltaTime * 3f);

        // desbloqueamos destino para reanudar actualizaciones
        destinationLocked = false;
        TrySetDestinationIfNeeded(visionSystem != null && visionSystem.Target != null ? visionSystem.Target.position : transform.position);

        if(agent != null && enemyData != null)
        {
            agent.acceleration = enemyData.normalAcceleration;
        }
    }

    //Rotación separada de HandleChase para facilitar pruebas y ajustes
    void RotateTowadrsTarget()
    {
        if (visionSystem == null || visionSystem.Target == null) return;

        Vector3 dir = visionSystem.Target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        //Convertimos rotación actual -> error angular
        Quaternion deltaRot = targetRotation * Quaternion.Inverse(transform.rotation);
        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f) angle -= 360f;

        //Normalizar
        if (float.IsNaN(axis.x)) return;
        axis.Normalize();

        //Parámetros de dampeo
        float stiffness = enemyData.rotationStiffness; //qué tan fuerte corrige hacia el objetivo
        float damping = enemyData.rotationDamping; //qué tan rápido se disipa la velocidad

        //Aplicar amortiguación (fricción angular)
        angularVelocity *= Mathf.Exp(-damping * Time.deltaTime);

        //Aplicar rotación proporcional a la velocidad angular
        transform.rotation = Quaternion.AngleAxis(angularVelocity.magnitude * Time.deltaTime, axis) * transform.rotation;
    }
    #endregion

    #region Utilities
    // Setea agent.SetDestination solo si la posición nueva difiere más que el umbral.
    void TrySetDestinationIfNeeded(Vector3 destination)
    {
        if (agent == null) return;

        // Si aún no hemos establecido destino o cambió lo suficiente, actualizar.
        float sqThreshold = enemyData.destinationUpdateThreshold * enemyData.destinationUpdateThreshold;
        if (lastSetDestination == Vector3.positiveInfinity ||
            Vector3.SqrMagnitude(lastSetDestination - destination) > sqThreshold)
        {
            agent.SetDestination(destination);
            lastSetDestination = destination;
        }
    }
    #endregion
}
