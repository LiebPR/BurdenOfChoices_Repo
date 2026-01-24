using Unity.VisualScripting;
using UnityEngine;

public class EnemyPerceptionHandler : MonoBehaviour
{
    
    [SerializeField] EnemyData data;

    #region Internal States
    Transform lastTarget;
    float lastHeardTime;
    #endregion

    #region Getter
    public Vector3 LastTargetPosition {  get; private set; }
    public bool IsHearingNoise { get; private set; } 
    #endregion

    #region References
    EnemyFSM fsm;
    VisionSystem visionSystem;
    HearingSystem hearingSystem;
    TurnToTargetState turnToTargetState;
    StunState stunState;
    #endregion

    void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        visionSystem = GetComponent<VisionSystem>();
        hearingSystem = GetComponent<HearingSystem>();
        turnToTargetState = GetComponent<TurnToTargetState>();
        stunState = GetComponent<StunState>();
    }

    private void Update()
    {
        if(IsHearingNoise && !HasValidSound())
        {
            IsHearingNoise = false;
        }
    }

    #region Subscription Events
    void OnEnable()
    {
        visionSystem.OnSeeTarget += HandleSeeTarget;
        visionSystem.OnLoseTarget += HandleLoseTarget;
        visionSystem.OnEnterPerception += HandleEnterPerception;

        hearingSystem.OnHearSound += HandleHearSound;
    }

    void OnDisable()
    {
        visionSystem.OnSeeTarget -= HandleSeeTarget;
        visionSystem.OnLoseTarget -= HandleLoseTarget;
        visionSystem.OnEnterPerception -= HandleEnterPerception;

        hearingSystem.OnHearSound -= HandleHearSound;
    }
    #endregion

    #region Handlers Vision & Perception
    void HandleEnterPerception(Transform target)
    {
        if (stunState.IsStunned) return;
        if (target != null && IsPlayerDead(target)) return;
        if (fsm.CurrentState == EnemyState.Chase) return;

        lastTarget = target;
        fsm.OnTurnToTarget(target);
    }

    void HandleSeeTarget(Transform target)
    {
        if (stunState.IsStunned) return;
        if (target != null && IsPlayerDead(target)) return;

        lastTarget = target;

        // Iniciar coroutine para respetar visionDelay
        StartCoroutine(DelayedChase(target, data.visionDelay));
    }

    void HandleLoseTarget(Transform target)
    {
        if (stunState.IsStunned) return;
        fsm.OnPatrol();
    }
    #endregion

    #region Handlers Noise
    void HandleHearSound(Vector3 soundPosition)
    {
        if(stunState.IsStunned) return;
        if (visionSystem.CanSeeTarget()) return;
        if (lastTarget != null && IsPlayerDead(lastTarget)) return;

        LastTargetPosition = soundPosition;
        lastHeardTime = Time.time;
        IsHearingNoise = true;

        fsm.OnInvestigateSound();
    }
    #endregion

    #region Utilities
    bool IsPlayerDead(Transform target)
    {
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        return playerHealth != null && !playerHealth.IsAlive;
    }

    public bool HasValidSound()
    {
        return Time.time - lastHeardTime <= data.noiseMemoryTime;
    }

    public void ForgetSound()
    {
        IsHearingNoise = false;
    }

    #endregion

    System.Collections.IEnumerator DelayedChase(Transform target, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Verificar que todavía puede ver al jugador
        if (visionSystem.CanSeeTarget())
        {
            fsm.OnChase();

            // Actualizamos target para ataque, pero no ejecutamos golpe
            EnemyAttack attack = GetComponent<EnemyAttack>();
            if (attack != null)
            {
                attack.SetTarget(target); // solo para que la animación sepa a quién golpear
            }
        }
    }
}
