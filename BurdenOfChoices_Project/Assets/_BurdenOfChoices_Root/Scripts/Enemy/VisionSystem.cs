using System;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// VisionSystem: Script responsable de la visión del enemigo;
///     - Cone Vision: Detecta al Player si esta dentro.
///     - Obstacle Raycast: Detecta los obstaculos entre el enemigo y el Player.
///     - Perception Radius: Radio de percepción del enemigo.
/// </summary>
public class VisionSystem : MonoBehaviour
{
    [SerializeField] Transform visionPoint; // punto desde donde se emite la vision.

    [SerializeField] EnemyData enemyData; // datos del enemigo

    #region Internal States
    float lostTimer = 0f; // temporizador para perder al jugador
    float perceptionTimer; // temporizador para la percepción

    bool canSeePlayer; // indica si el enemigo puede ver al jugador
    bool isPlayerInPerceptionArea; // indica si el jugador está en el área de parada

    RaycastHit rayObstacleDetector; // rayo para detectar obstáculos
    #endregion

    #region References
    NavMeshAgent agent;
    EnemyFSM fsm;
    #endregion

    #region Getters
    public Transform Target { get; private set; } //player
    public Vector3 LastKnownPosition { get; private set; } // última posición conocida del jugador
    public bool IsPlayerInStopArea { get; private set; } // indica si el jugador está en el área de parada
    #endregion

    #region Events
    public event Action<Transform> OnSeeTarget; // evento que se dispara al ver al jugador
    public event Action<Transform> OnLoseTarget; // evento que se dispara al perder al jugador
    #endregion

    private void Awake()
    {
        //Referencias: 
        agent = GetComponent<NavMeshAgent>();
        fsm = GetComponent<EnemyFSM>();

        FindPlayer(); //busca al jugador automaticamente
    }

    private void Update()
    {
        if(Target != null)
        {
            EvaluateVision();
        }
    }

    #region Vision Evaluation
    void EvaluateVision()
    {
        Vector3 dirToTarget = (Target.position - visionPoint.position).normalized;
        float distToTarget = Vector3.Distance(visionPoint.position, Target.position);

        bool inCone = ConeVision(dirToTarget, distToTarget);
        bool inPerception = PerceptionArea();
        bool obstacleRay = CheckObstacle(dirToTarget, distToTarget, out rayObstacleDetector);

        UpdateVisionState(inCone, inPerception, obstacleRay);
        CheckStopArea(distToTarget);
    }

    void UpdateVisionState(bool inCone, bool inPerception, bool obstacleRay)
    {
        bool previusSee = canSeePlayer;

        //Cone visión - Prioridad alta
        if(inCone && !obstacleRay)
        {
            //Si ve, resetea lostTimer y marca visión
            lostTimer = enemyData.lostDelay;
            canSeePlayer = true;
            LastKnownPosition = Target.position;
        }
        else
        {
            //Si no ve frontalmente, decrementa timer de pérdida
            lostTimer -= Time.deltaTime;
            if(lostTimer <= 0f)
            {
                canSeePlayer = false;
                lostTimer = 0f;
            }
        }

        //Perception Area - solo si no se ve en el cono
        if(!canSeePlayer && inPerception)
        {
            //Si acabamos de entrar en área de percepción, inicializamos el temporizador
            if (!isPlayerInPerceptionArea)
            {
                isPlayerInPerceptionArea = true;
                perceptionTimer = enemyData.perceptionDelay;
            }

            perceptionTimer -= Time.deltaTime;
            if(perceptionTimer <= 0f)
            {
                canSeePlayer = true;
                LastKnownPosition = Target.position;
                perceptionTimer = enemyData.perceptionDelay; //reset para la próxima vez
            }
        }
        else
        {
            //fuera del área de percepción -> reset de estado de percepción
            isPlayerInPerceptionArea = false;
            perceptionTimer = enemyData.perceptionDelay;
        }

        //Eventos - solo cuando hay cambio real
        if(canSeePlayer && !previusSee)
        {
            OnSeeTarget?.Invoke(Target);
        }
        else if(!canSeePlayer && previusSee)
        {
            OnLoseTarget?.Invoke(Target);
        }
    }
    #endregion

    #region Vision Detectors
    //Cone Vision Detector: Detecta si el jugador está dentro del cono de visión del enemigo.
    bool ConeVision(Vector3 dirToTarget, float distToTarget)
    {
        if (distToTarget > enemyData.visionRadius) return false; //fuera del rango máximo

        //Claculamos el ángulo entre el frente del enemigo y el objetivo
        float angle = Vector3.Angle(visionPoint.transform.forward, dirToTarget);

        //Comprobamos si el jugador está dentro del ángulo de visión
        return angle < enemyData.visionAngle * 0.5f;
    }

    //Obstacle Raycast Detector: Detecta si hay obstáculos entre el enemigo y el jugador
    bool CheckObstacle(Vector3 dirToTarget, float distToTarget, out RaycastHit hit)
    {
        bool hasHit = Physics.Raycast(visionPoint.position, dirToTarget, out hit, distToTarget, enemyData.obstacleMask);
        return hasHit;
    }

    //Perception Area Detector: Detecta si el jugador está dentro del área de percepción del enemigo
    bool PerceptionArea()
    {
        if (Target == null) return false;

        float perceptionRadius = enemyData.perceptionRadius;
        float distance = Vector3.Distance(visionPoint.position, Target.position);
        if (distance > perceptionRadius) return false;

        Vector3 dirToTarget = (Target.position - visionPoint.position).normalized;
        float distToTarget = distance;

        //Si hay obstáculo, consideramos que no está en percepción efectivo
        if (CheckObstacle(dirToTarget, distToTarget, out _)) return false;

        return true;
    }

    void CheckStopArea(float distToTarget)
    {
        IsPlayerInStopArea = distToTarget <= enemyData.stopAreaRadius;
    }
    #endregion

    #region Utilities
    void FindPlayer()
    {
        PlayerController player = GameObject.FindFirstObjectByType<PlayerController>();
        if (player != null) Target = player.transform;
    }
    #endregion

    #region Gizmos 
    private void OnDrawGizmosSelected()
    {
        if (visionPoint == null) return;

        //Color base según el estado
        Color baseColor;

        if (canSeePlayer)
            baseColor = Color.green; //viendo al jugador
        else
            baseColor = Color.yellow; //no viendo al jugador

        // Radio de visión
        Gizmos.color = baseColor;
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.visionRadius);

        // Área de percepción
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.perceptionRadius);

        // Cono visual
        Vector3 rightDir = Quaternion.Euler(0, enemyData.visionAngle * 0.5f, 0) * visionPoint.forward;
        Vector3 leftDir = Quaternion.Euler(0, -enemyData.visionAngle * 0.5f, 0) * visionPoint.forward;

        Gizmos.color = baseColor;
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + rightDir * enemyData.visionRadius);
        Gizmos.DrawLine(visionPoint.position, visionPoint.position + leftDir * enemyData.visionRadius);

        // Raycast de obstáculos
        if (Target != null)
        {
            Gizmos.color = rayObstacleDetector.collider != null ? Color.magenta : Color.red;
            Vector3 rayEnd = rayObstacleDetector.collider != null ? rayObstacleDetector.point : Target.position;
            Gizmos.DrawLine(visionPoint.position, rayEnd);
        }

        // Área de parada
        Gizmos.DrawWireSphere(visionPoint.position, enemyData.stopAreaRadius);
    }
    #endregion
}
