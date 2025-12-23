using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour
{
    #region Inspector Variables
    [Header("Refertences")]
    [SerializeField] EnemyData enemyData;

    [Header("Health Phases")]
    [Tooltip("Daño suficiente para matar de un golpe")]
    [SerializeField] int instantKillDamage = 2;

    [Header("Stun")]
    [SerializeField] float stunDuration = 3f;

    [Header("Knockback Settings")]
    [SerializeField] AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] float knockbackDuration = 0.3f;
    #endregion

    #region Internal States
    bool firstLifeBroken;      // Indica si el primer golpe ya ocurrió
    bool isDead;               // Indica si el enemigo murió
    float stunTimer;           // Temporizador de stun
    #endregion

    #region References
    HealthSystem healthSystem;
    EnemyFSM fsm;
    Rigidbody rb;
    EnemyMovementCommands movementCommands;
    #endregion

    #region Getters
    public Vector3 LastHitDirection {  get; private set; }
    public float LastKncokBack {  get; private set; }
    #endregion

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        fsm = GetComponent<EnemyFSM>();
        rb = GetComponent<Rigidbody>();
        movementCommands = GetComponent<EnemyMoveController>().Commands;
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
        if(isDead) return;

        LastHitDirection = hitDirection;
        LastKncokBack = knockBack;

        healthSystem.TakeHit(damage);
    }
    #endregion

    #region Event Handlers
    void HandleHit(int currentHealth)
    {
        if (isDead) return;

        int lastDamage = healthSystem.MaxHealth - currentHealth;

        // Golpe instantáneo que mata
        if (lastDamage >= instantKillDamage)
        {
            return;
        }

        // Primer golpe -> Stun
        if (!firstLifeBroken && currentHealth == healthSystem.MaxHealth - 1)
        {
            firstLifeBroken = true;
            movementCommands.StopEnemy();
            StartCoroutine(KnockbackRoutine(LastHitDirection, LastKncokBack));

            EnterStun();
        }
        // Segundo golpe -> muerte
        else if (firstLifeBroken && currentHealth <= 0)
        {
            isDead = true;
            fsm.OnDeath();
        }
    }

    void HandleDeath()
    {
        isDead = true;
        fsm.OnDeath();
    }
    #endregion

    #region Stun Logic
    void EnterStun()
    {
        stunTimer = stunDuration;
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
        firstLifeBroken = false;
        healthSystem.HealFull(); // Recupera la primera vida

        movementCommands.RestoreEnemy(enemyData.patrolSpeed, enemyData.normalAcceleration);
        fsm.OnPatrol(); // Volver al estado de patrulla o idle
    }
    #endregion

    #region Knockback Coroutine
    IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        if (rb == null) yield break;

        Vector3 planarDir = new Vector3(direction.x, 0f, direction.z).normalized;

        float elapsed = 0f;
        while(elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            float multiplier = knockbackCurve.Evaluate(t);

            rb.linearVelocity = planarDir * force * multiplier; // Asignamos velocidad directamente

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion
}
