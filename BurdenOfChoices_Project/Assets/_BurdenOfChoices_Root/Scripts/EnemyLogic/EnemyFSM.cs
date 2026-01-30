using System.Collections.Generic;
using UnityEngine;

public enum EnemyState
{
    Patrol,
    Idle,
    Alert,
    Chase,
    Stun,
    Death,
    TurnToTarget,
    InvestigateSound
}

public class EnemyFSM : MonoBehaviour
{
    [SerializeField] bool debugLog;

    IEnemyState currentStateInstance;

    // Diccionario de instancias de estados
    Dictionary<EnemyState, IEnemyState> stateInstances = new Dictionary<EnemyState, IEnemyState>();

    bool isChangingState;
    bool hasPendingStateRequest;
    bool isDead;
    EnemyState pendingStateRequest;

    // Cache de nombres de estados para logs
    string[] stateNames;

    #region Getters
    public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;
    public IEnemyState CurrentStateInstance => currentStateInstance;
    #endregion

    #region Events
    public event System.Action<EnemyState> OnStateChanged;
    #endregion

    private void Awake()
    {
        // Cacheamos nombres de estados para logs
        stateNames = System.Enum.GetNames(typeof(EnemyState));
    }

    private void Update()
    {
        if (isDead) return;

        if (GameStopManager.Instance != null && GameStopManager.Instance.isGamePaused)
            return;

        // Lógica del estado actual
        currentStateInstance?.Handle();

        // Procesar cambios de estado pendientes
        if (!isChangingState && hasPendingStateRequest)
        {
            var pending = pendingStateRequest;
            hasPendingStateRequest = false;
            ChangeState(pending);
        }
    }

    #region Registro de Estados
    /// <summary>
    /// Registra un estado en la FSM
    /// </summary>
    public void RegisterState(EnemyState state, IEnemyState instance)
    {
        if (instance == null)
        {
            Debug.LogWarning($"[FSM] Intento de registrar estado null: {state}");
            return;
        }

        stateInstances[state] = instance;
    }
    #endregion

    #region Cambio de Estado
    /// <summary>
    /// Cambia de estado si es diferente al actual
    /// </summary>
    public void ChangeState(EnemyState newState)
    {
        if (newState == CurrentState) return;

        if (isChangingState)
        {
            // Encolamos la petición
            hasPendingStateRequest = true;
            pendingStateRequest = newState;
            return;
        }

        isChangingState = true;

        // EXIT del estado actual
        currentStateInstance?.Exit();

        // Actualizamos estado
        CurrentState = newState;

        if (newState == EnemyState.Death)
        {
            isDead = true;
            isChangingState = false;
            hasPendingStateRequest = false;
        }

        if (!isDead)
        {
            try { OnStateChanged?.Invoke(CurrentState); } catch { }
        }

        // Obtener la instancia del nuevo estado
        if (!stateInstances.TryGetValue(newState, out currentStateInstance) || currentStateInstance == null)
        {
            isChangingState = false;
            if (debugLog)
                Debug.LogWarning($"[FSM] No existe instancia del estado {newState}");
            return;
        }

        // ENTER
        currentStateInstance.Enter();

        isChangingState = false;
    }
    #endregion

    #region Inicialización Segura
    /// <summary>
    /// Inicializa la FSM y asigna el estado inicial sin llamar Enter todavía.
    /// </summary>
    public void InitializeFSM(EnemyState initialState = EnemyState.Patrol)
    {
        CurrentState = initialState;

        if (stateInstances.TryGetValue(initialState, out currentStateInstance))
        {
            // Solo asigna, no llama Enter todavía
        }
    }

    /// <summary>
    /// Inicia la FSM, llamando Enter en el estado actual.
    /// </summary>
    public void StartFSM()
    {
        if (currentStateInstance != null)
        {
            currentStateInstance.Enter();
            OnStateChanged?.Invoke(CurrentState);
        }
        else if (debugLog)
        {
            Debug.LogWarning("[FSM] StartFSM llamado pero el estado inicial es null");
        }
    }
    #endregion

    #region Handlers Públicos
    public void OnPatrol() { if (isDead) return; ChangeState(EnemyState.Patrol); }
    public void OnIdle() { if (isDead) return; ChangeState(EnemyState.Idle); }
    public void OnChase() { if (isDead) return; ChangeState(EnemyState.Chase); }
    public void OnStun() { if (isDead) return; ChangeState(EnemyState.Stun); }
    public void OnDeath() { if (isDead) return; ChangeState(EnemyState.Death); }
    public void OnTurnToTarget(Transform target)
    {
        if(isDead) return;

        if (stateInstances.TryGetValue(EnemyState.TurnToTarget, out IEnemyState state))
        {
            if (state is TurnToTargetState turnState)
            {
                turnState.SetTarget(target);
            }
        }
        ChangeState(EnemyState.TurnToTarget);
    }
    public void OnInvestigateSound() { if (isDead) return; ChangeState(EnemyState.InvestigateSound); }
    #endregion

    #region Reseteo Forzado
    public void ForceResetToPatrol()
    {
        if(isDead) return;

        isChangingState = false;
        hasPendingStateRequest = false;

        CurrentState = EnemyState.Patrol;

        if (stateInstances.TryGetValue(EnemyState.Patrol, out IEnemyState patrolState))
        {
            currentStateInstance = patrolState;
            currentStateInstance.Enter();
        }

        OnStateChanged?.Invoke(CurrentState);
    }
    #endregion
}
