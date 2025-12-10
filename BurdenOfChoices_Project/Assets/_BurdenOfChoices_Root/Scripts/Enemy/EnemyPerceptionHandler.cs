using UnityEngine;

public class EnemyPerceptionHandler : MonoBehaviour
{
    
    [SerializeField] EnemyData data;

    #region Internal States
    Vector3 lastTargetPosition;
    Transform lastTarget;
    #endregion

    #region References
    EnemyFSM fsm;
    VisionSystem visionSystem;
    TurnToTargetState turnToTargetState;
    #endregion

    void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        visionSystem = GetComponent<VisionSystem>();
        turnToTargetState = GetComponent<TurnToTargetState>();
    }

    #region Subscription Events
    void OnEnable()
    {
        visionSystem.OnSeeTarget += HandleSeeTarget;
        visionSystem.OnLoseTarget += HandleLoseTarget;
        visionSystem.OnEnterPerception += HandleEnterPerception;
    }

    void OnDisable()
    {
        visionSystem.OnSeeTarget -= HandleSeeTarget;
        visionSystem.OnLoseTarget -= HandleLoseTarget;
        visionSystem.OnEnterPerception -= HandleEnterPerception;
    }
    #endregion

    #region Handlers
    void HandleEnterPerception(Transform target)
    {
        if (fsm.CurrentState == EnemyState.Chase) return;
        lastTarget = target;
        fsm.OnTurnTuTarget(target);
    }

    void HandleSeeTarget(Transform target)
    {
        lastTarget = target;
        fsm.OnChase();
    }

    void HandleLoseTarget(Transform target)
    {
        fsm.OnPatrol();
    }
    #endregion
}
