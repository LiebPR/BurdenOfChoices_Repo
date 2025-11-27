using UnityEngine;

public class EnemyEventManager : MonoBehaviour
{
    #region Refereces
    EnemyFSM fsm;

    //Perceptions
    VisionSystem visionSystem;

    //States
    EnemyMoveController enemyMove;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        visionSystem = GetComponentInChildren<VisionSystem>();
        enemyMove = GetComponent<EnemyMoveController>();
    }

    #region Subscription Events
    private void OnEnable()
    {
        //Vision System
        visionSystem.OnSeeTarget += HandleTargetSee;
        visionSystem.OnLoseTarget += HandleTargetLost;

        //Move Controller
        enemyMove.OnIdleStarted += HandleIdleStart;
        enemyMove.OnIdleEnded += HandleIdleEnd;
    }

    private void OnDisable()
    {
        //Vision System
        visionSystem.OnSeeTarget -= HandleTargetSee;
        visionSystem.OnLoseTarget -= HandleTargetLost;

        //Move Controller
        enemyMove.OnIdleStarted -= HandleIdleStart;
        enemyMove.OnIdleEnded -= HandleIdleEnd;
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
    #endregion

    #region Move Handlers
    void HandleIdleStart()
    {
        fsm.OnIdle();
    }

    void HandleIdleEnd()
    {
        fsm.OnPatrol();
    }
    #endregion
}
