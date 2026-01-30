using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyHealth
/// Gestiona las fases de vida del enemigo y su reacción a los golpes. 
/// Coordina daño, stun, knockback y transición a estados de la FSM.
/// </summary>
[RequireComponent(typeof(EnemyMotionContext))]
public class EnemyHealth : MonoBehaviour
{
    #region Inspector Variables
    [Header("Refertences")]
    [SerializeField] EnemyData enemyData;

    [Header("Health Phases")]
    [Tooltip("Daño mínimo para provocar muerte instantánea.")]
    [SerializeField] int instantKillDamage = 2;

    [Header("Stun")]
    [Tooltip("Duración total del estado de stun.")]
    [SerializeField] float stunDuration = 3f;

    [Header("Knockback Settings")]
    [Tooltip("Curva de fuerza aplicada durente el retroceso.")]
    [SerializeField] AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [Tooltip("Duración total del knockback.")]
    [SerializeField] float knockbackDuration = 0.3f;
    #endregion

    #region Internal States
    bool firstLifeBroken;
    bool isDead;
    bool deadAfterStun;
    bool isStunned;
    float stunTimer;
    int lastHitDamage;
    #endregion

    #region References
    HealthSystem healthSystem;
    EnemyFSM fsm;
    Rigidbody rb;
    EnemyMovementCommands movementCommands;
    EnemyAnimationHandler animationHandler;
    EnemyPerceptionFeedback perceptionFeedback;
    #endregion

    #region Getters
    public Vector3 LastHitDirection { get; private set; }
    public float LastKncokBack { get; private set; }
    #endregion

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        fsm = GetComponent<EnemyFSM>();
        rb = GetComponent<Rigidbody>();
        animationHandler = GetComponent<EnemyAnimationHandler>();
        perceptionFeedback = GetComponent<EnemyPerceptionFeedback>();
    }

    private void Start()
    {
        movementCommands = GetComponent<EnemyMotionContext>().Commands;

        if (movementCommands == null)
            Debug.LogError("EnemyHealth: Commands sigue siendo NULL en Start", this);
    }

    private void OnEnable()
    {
        healthSystem.OnHit += HandleHit;
        healthSystem.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        healthSystem.OnHit -= HandleHit;
        healthSystem.OnDeath -= HandleDeath;
    }

    private void Update()
    {
        HandleStunRecovery();
    }

    #region Public API
    public void TakeHit(int damage, Vector3 hitDirection, float knockBack)
    {
        if (isDead) return;

        lastHitDamage = damage;
        LastHitDirection = hitDirection;
        LastKncokBack = knockBack;

        healthSystem.TakeHit(damage);
    }
    #endregion

    #region Event Handlers
    void HandleHit(int currentHealth)
    {
        if (isDead) return;

        if (lastHitDamage >= instantKillDamage && currentHealth <= 0)
        {
            deadAfterStun = false;
            return;
        }

        if (!firstLifeBroken && currentHealth > 0)
        {
            firstLifeBroken = true;

            movementCommands.EnterPhysicalMode();
            RotateOppositeToHit(LastHitDirection);
            StartCoroutine(KnockbackRoutine(LastHitDirection, LastKncokBack));

            EnterStun();
        }
        else if (firstLifeBroken && currentHealth <= 0)
        {
            isDead = true;
            deadAfterStun = true;
            fsm.OnDeath();
        }
    }

    void HandleDeath()
    {
        isDead = true;

        // Apagar todos los sprites de animación y viñeta
        animationHandler.SetVelocityBody(0f);
        animationHandler.SetVelocityLegs(0f);

        animationHandler.SetIsRunningBody(false);
        animationHandler.SetIsRunningLegs(false);

        animationHandler.SetStunnedBody(false);
        animationHandler.SetStunnedLegs(false);

        animationHandler.SetTurnningBody(false);

        if (deadAfterStun)
        {
            animationHandler.SetDeathAfterStunBody(true);
            animationHandler.SetDeathAfterStunLegs(true);
        }
        else
        {
            animationHandler.SetDeathBody();
            animationHandler.SetDeathLegs();
        }

        // Apagar feedback de viñeta para evitar accesos inválidos
        if (perceptionFeedback != null)
        {
            perceptionFeedback.ClearFeedback();
        }

        fsm.OnDeath();
    }
    #endregion

    #region Stun Logic
    void EnterStun()
    {
        if (isStunned) return;

        isStunned = true;
        stunTimer = stunDuration;

        animationHandler.SetStunBody();
        animationHandler.SetStunLegs();

        animationHandler.SetVelocityBody(0f);
        animationHandler.SetVelocityLegs(0f);

        animationHandler.SetIsRunningBody(false);
        animationHandler.SetIsRunningLegs(false);

        animationHandler.SetTurnningBody(false);

        animationHandler.SetStunnedBody(true);
        animationHandler.SetStunnedLegs(true);

        fsm.OnStun();
    }

    void HandleStunRecovery()
    {
        if (fsm.CurrentState != EnemyState.Stun) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            RecoverFromStun();
        }
    }

    void RecoverFromStun()
    {
        isStunned = false;
        firstLifeBroken = false;

        healthSystem.HealFull();

        animationHandler.SetStunnedBody(false);
        animationHandler.SetStunnedLegs(false);

        movementCommands.ExitPhysicalMode(enemyData.patrolSpeed, enemyData.normalAcceleration);
        fsm.OnPatrol();
    }
    #endregion

    #region Knockback Coroutine
    IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        if (rb == null) yield break;

        Vector3 planarDir = new Vector3(direction.x, 0f, direction.z).normalized;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            float multiplier = knockbackCurve.Evaluate(t);

            rb.linearVelocity = planarDir * force * multiplier;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Utilities
    void RotateOppositeToHit(Vector3 hitDirection)
    {
        Vector3 planarDir = new Vector3(hitDirection.x, 0f, hitDirection.z);

        if (planarDir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(-planarDir.normalized);
        transform.rotation = targetRotation;
    }
    #endregion
}
