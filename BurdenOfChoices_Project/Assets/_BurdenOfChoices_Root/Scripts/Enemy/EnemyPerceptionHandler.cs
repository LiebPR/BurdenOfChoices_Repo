using UnityEngine;

/// <summary>
/// Gestiona la percepción del enemigo: visión, audición del jugador y ruido puntual.
/// Todos los delays pasan por un único PerceptionDelayController.
/// </summary>
public class EnemyPerceptionHandler : MonoBehaviour
{
    [SerializeField] EnemyData data;

    #region Internal States
    Transform lastTarget;
    Vector3 lastHeardPoint;
    PlayerNoiseEmitter.MovementState playerState;
    #endregion

    #region References
    EnemyFSM fsm;
    VisionSystem visionSystem;
    HearingSystem hearingSystem;
    TurnToTargetState turnToTargetState;
    AlertState alertState;
    PerceptionDelayController delayController;
    #endregion

    void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        visionSystem = GetComponent<VisionSystem>();
        hearingSystem = GetComponent<HearingSystem>();
        turnToTargetState = GetComponent<TurnToTargetState>();
        alertState = GetComponent<AlertState>();
        delayController = gameObject.AddComponent<PerceptionDelayController>();
        delayController.enemyData = data;
        delayController.DelayCompleted += OnDelayCompleted;
    }

    #region Subscription Events
    void OnEnable()
    {
        visionSystem.OnSeeTarget += HandleSeeTarget;
        visionSystem.OnLoseTarget += HandleLoseTarget;
        visionSystem.OnEnterPerception += HandleEnterPerception;
        hearingSystem.OnHearTarget += HandleHearTarget;
        hearingSystem.OnHearNoisePoint += HandleHearNoisePoint;
        hearingSystem.OnLoseNoise += HandleLoseNoise;
    }

    void OnDisable()
    {
        visionSystem.OnSeeTarget -= HandleSeeTarget;
        visionSystem.OnLoseTarget -= HandleLoseTarget;
        visionSystem.OnEnterPerception -= HandleEnterPerception;
        hearingSystem.OnHearTarget -= HandleHearTarget;
        hearingSystem.OnHearNoisePoint -= HandleHearNoisePoint;
        hearingSystem.OnLoseNoise -= HandleLoseNoise;
    }
    #endregion

    #region Handlers
    void HandleEnterPerception(Transform target)
    {
        if (fsm.CurrentState == EnemyState.Chase) return; // ignorar percepción mientras persigue
        lastTarget = target;
        delayController.StartOrContinueDelay(PerceptionDelayController.PerceptionType.Perception, target.position);
    }

    void HandleSeeTarget(Transform target)
    {
        delayController.StartOrContinueDelay(PerceptionDelayController.PerceptionType.Visual, target.position);
    }

    void HandleLoseTarget(Transform target)
    {
        // Activamos el delay de pérdida usando el controller
        delayController.StartLostDelay(target.position);
        delayController.SetStimulusActive(false); // empieza a decaer
    }

    void HandleHearTarget(Transform target)
    {
        lastTarget = target;
        lastHeardPoint = target.position;
        playerState = target.GetComponent<PlayerNoiseEmitter>().currentState;
        var type = (playerState == PlayerNoiseEmitter.MovementState.Running)
            ? PerceptionDelayController.PerceptionType.SoundRun
            : PerceptionDelayController.PerceptionType.SoundWalk;
        delayController.StartOrContinueDelay(type, lastHeardPoint);
    }

    void HandleHearNoisePoint(Vector3 position)
    {
        lastHeardPoint = position;
        delayController.StartOrContinueDelay(PerceptionDelayController.PerceptionType.Hit, position);
    }

    void HandleLoseNoise(Transform target)
    {
        // Activamos el delay de pérdida usando el controller
        delayController.StartLostDelay(target.position);
        delayController.SetStimulusActive(false); // empieza a decaer
    }
    #endregion

    #region Callbacks
    void OnDelayCompleted(PerceptionDelayController.PerceptionType type, Vector3 pos)
    {
        Debug.Log($"[PerceptionHandler] Delay Completed: {type} at {pos}");
        switch (type)
        {
            case PerceptionDelayController.PerceptionType.Perception: // PERCIBE ALGO
                turnToTargetState.SetTarget(lastTarget);
                fsm.OnTurnTuTarget();
                break;
            case PerceptionDelayController.PerceptionType.Visual: // VE ALGO
                fsm.OnChase();
                break;
            case PerceptionDelayController.PerceptionType.SoundWalk: // ESCUCHA MOVIMIENTO
            case PerceptionDelayController.PerceptionType.SoundRun:
                turnToTargetState.SetTargetPoint(pos);
                fsm.OnChase();
                break;
            case PerceptionDelayController.PerceptionType.Hit: // ESCUCHA ALGO GOLPEAR
                alertState.SetAlertPoint(pos);
                fsm.OnAlert();
                break;
            case PerceptionDelayController.PerceptionType.Lost: // PIERDE
                Debug.Log("[PerceptionHandler] Lost -> Volviendo a patrulla");
                fsm.OnPatrol();
                break;
        }
    }
    #endregion
}