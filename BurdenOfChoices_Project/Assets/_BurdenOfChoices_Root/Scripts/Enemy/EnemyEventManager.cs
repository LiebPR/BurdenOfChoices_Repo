using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    #region Refereces
    EnemyFSM fsm;

    //Perceptions
    VisionSystem visionSystem;

    //States
    TurnToTargetState turnPatrolState;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        visionSystem = GetComponentInChildren<VisionSystem>(); 
        turnPatrolState = GetComponent<TurnToTargetState>();
    }

    #region Subscription Events
    private void OnEnable()
    {
        //Vision System
        visionSystem.OnSeeTarget += HandleTargetSee;
        visionSystem.OnLoseTarget += HandleTargetLost;
        visionSystem.OnEnterPerception += HandleEnterPerception;
    }

    private void OnDisable()
    {
        //Vision System
        visionSystem.OnSeeTarget -= HandleTargetSee;
        visionSystem.OnLoseTarget -= HandleTargetLost;
        visionSystem.OnEnterPerception -= HandleEnterPerception;
    }
    #endregion

    #region Vision Handlers
    void HandleTargetSee(Transform target)
    {
        fsm.OnChase();
    }
    void HandleTargetLost(Transform target)
    {
        fsm.OnPatrol();
    }
    void HandleEnterPerception(Transform target)
    {
        turnPatrolState.SetTarget(target);
        fsm.OnTurnTuTarget();
    }
    #endregion

}
