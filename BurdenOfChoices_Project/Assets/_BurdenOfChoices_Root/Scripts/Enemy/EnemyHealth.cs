using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// EnemyHealth
/// Gestiona las fases de vida del enemigo y su reacción a los golpes. 
/// Coordina daño, stun, knockback y transición a estados de la FSM.
/// </summary>
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
    bool firstLifeBroken; // Indica si el primer golpe ya ocurrió
    bool isDead; // Indica si el enemigo murió
    bool isStunned;
    float stunTimer; // Temporizador de stun
    #endregion

    #region References
    HealthSystem healthSystem;
    EnemyFSM fsm;
    Rigidbody rb;
    EnemyMovementCommands movementCommands;
    EnemyAnimationHandler animationHandler;
    #endregion

    #region Getters
    public Vector3 LastHitDirection {  get; private set; } //Diracción del último impacto
    public float LastKncokBack {  get; private set; } //Fuerza del último impacto.
    #endregion

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        fsm = GetComponent<EnemyFSM>();
        rb = GetComponent<Rigidbody>();
        animationHandler = GetComponent<EnemyAnimationHandler>();
        movementCommands = GetComponent<EnemyMotionContext>().Commands;
    }

    private void OnEnable()
    {
        //Escucha eventos de vida
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
        //Controla la salida del stun
        HandleStunRecovery();
    }

    #region Public API
    /// <summary>
    /// Entrada externa de daño.
    /// Registra contexto del golpe antes de delegar en HealthSystem.
    /// </summary>
    public void TakeHit(int damage, Vector3 hitDirection, float knockBack)
    {
        if(isDead) return;

        LastHitDirection = hitDirection;
        LastKncokBack = knockBack;

        healthSystem.TakeHit(damage);
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// Reacciona a un golpe recibido.
    /// Decide entre stun, muerte o ignorar.
    /// </summary>
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
            movementCommands.EnterPhysicalMode();
            RotateOppositeToHit(LastHitDirection);
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

    /// <summary>
    /// Respuesta directa a muerte confirmada.
    /// </summary>
    void HandleDeath()
    {
        isDead = true;

        animationHandler.SetVelocityBody(0f);
        animationHandler.SetVelocityLegs(0f);

        animationHandler.SetIsRunningBody(false);
        animationHandler.SetIsRunningLegs(false);

        animationHandler.SetStunnedBody(false);
        animationHandler.SetStunnedLegs(false);

        animationHandler.SetTurnningBody(false);

        animationHandler.SetDeathBody();
        animationHandler.SetDeathLegs();

        fsm.OnDeath();
    }
    #endregion

    #region Stun Logic
    /// <summary>
    /// Entrada al estado de stun.
    /// </summary>
    void EnterStun()
    {
        if (isStunned) return;

        isStunned = true;
        stunTimer = stunDuration;

        //Aniamtios
        animationHandler.SetVelocityBody(0f);
        animationHandler.SetVelocityLegs(0f);

        animationHandler.SetIsRunningBody(false);
        animationHandler.SetIsRunningLegs(false);

        animationHandler.SetTurnningBody(false);

        animationHandler.SetStunBody();
        animationHandler.SetStunLegs();

        animationHandler.SetStunnedBody(true);
        animationHandler.SetStunnedLegs(true);

        fsm.OnStun();
    }

    /// <summary>
    /// Controla el tiempo de recuperación del stun.
    /// </summary>
    void HandleStunRecovery()
    {
        if (fsm.CurrentState != EnemyState.Stun) return;

        stunTimer -= Time.deltaTime;

        if (stunTimer <= 0f)
        {
            RecoverFromStun();
        }
    }

    /// <summary>
    /// Restaura estado normal tras el stun.
    /// </summary>
    void RecoverFromStun()
    {
        isStunned = false;
        firstLifeBroken = false;

        healthSystem.HealFull(); // Recupera la primera vida

        animationHandler.SetStunnedBody(false);
        animationHandler.SetStunnedLegs(false);

        movementCommands.ExitPhysicalMode(enemyData.patrolSpeed, enemyData.normalAcceleration);
        fsm.OnPatrol(); // Volver al estado de patrulla o idle
    }
    #endregion

    #region Knockback Coroutine
    /// <summary>
    /// Aplica retroceso físico usando Rigidbody.
    /// </summary>
    IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        if (rb == null) yield break;

        Vector3 planarDir = new Vector3(direction.x, 0f, direction.z).normalized;

        float elapsed = 0f;
        while(elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            float multiplier = knockbackCurve.Evaluate(t);

            //Movimiento físico directo
            rb.linearVelocity = planarDir * force * multiplier; // Asignamos velocidad directamente

            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Utilitis
    void RotateOppositeToHit(Vector3 hitDirection)
    {
        Vector3 planarDir = new Vector3(hitDirection.x, 0f, hitDirection.z);

        if (planarDir.sqrMagnitude < 0.0001f) return;

        // Mirar en dirección contraria al golpe
        Quaternion targetRotation = Quaternion.LookRotation(-planarDir.normalized);
        transform.rotation = targetRotation;
    }
    #endregion
}
