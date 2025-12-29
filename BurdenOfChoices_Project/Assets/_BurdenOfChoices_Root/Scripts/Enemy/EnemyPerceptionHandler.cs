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
    StunState stunState;
    #endregion

    void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        visionSystem = GetComponent<VisionSystem>();
        turnToTargetState = GetComponent<TurnToTargetState>();
        stunState = GetComponent<StunState>();
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
        if (stunState.IsStunned) return;
        if (fsm.CurrentState == EnemyState.Chase) return;
        lastTarget = target;
        fsm.OnTurnTuTarget(target);
    }

    void HandleSeeTarget(Transform target)
    {
        if (stunState.IsStunned) return;
        lastTarget = target;
        fsm.OnChase();
        
        // Informar al EnemyAttack del objetivo
        EnemyAttack attack = GetComponent<EnemyAttack>();
        if (attack != null)
        {
            attack.SetTarget(target);
        }
    }

    void HandleLoseTarget(Transform target)
    {
        if (stunState.IsStunned) return;
        fsm.OnPatrol();
    }
    #endregion
}
