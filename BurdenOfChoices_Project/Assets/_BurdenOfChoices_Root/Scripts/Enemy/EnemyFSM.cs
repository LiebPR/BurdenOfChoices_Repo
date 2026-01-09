using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum EnemyState
{
    Patrol,
    Idle,
    Alert,
    Chase,
    Stun,
    Death,
    TurnToTarget
}

public class EnemyFSM : MonoBehaviour
{
    [SerializeField] bool debugLog;

    IEnemyState currentStateInstance;

    // Inicializamos directamente para evitar null
    Dictionary<EnemyState, IEnemyState> stateInstances = new Dictionary<EnemyState, IEnemyState>();

    bool isChangingState;
    bool hasPendingStateRequest;
    EnemyState pendingStateRequest;

    // Nombres cacheados de estados para evitar allocations al loggear
    string[] stateNames;

    #region Getter
    public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;
    public IEnemyState CurrentStateInstance => currentStateInstance;
    #endregion

    #region Events
    public event System.Action<EnemyState> OnStateChanged; // Se dispara cuando cambia de estado
    #endregion

    private void Awake()
    {
        // Cacheamos los nombres de los estados (una sola allocation al inicio)
        stateNames = System.Enum.GetNames(typeof(EnemyState));
    }

    private void Update()
    {
        if (GameStopManager.Instance != null && GameStopManager.Instance.isGamePaused)
            return; // El enemigo no ejecuta lógica mientras el juego está pausado

        // Ejecutar la lógica del estado actual
        currentStateInstance?.Handle();

        // Procesar cualquier cambio de estado pendiente fuera del stack de ChangeState
        if (!isChangingState && hasPendingStateRequest)
        {
            var pending = pendingStateRequest;
            hasPendingStateRequest = false;
            ChangeState(pending);
        }
    }

    //Registra un estado en el FSM
    public void RegisterState(EnemyState state, IEnemyState instance)
    {
        if (instance == null)
        {
            return;
        }

        stateInstances[state] = instance;
    }

    //Cambia de estado si es diferente al actual
    public void ChangeState(EnemyState newState)
    {
        // Log temprano para depuración
        if (debugLog)
            Debug.Log($"[FSM] Solicitud de cambio: {CurrentState} → {newState}");

        // Evitar cambios redundantes
        if (newState == CurrentState) return;

        // Si ya estamos cambiando de estado, encolamos la petición.
        if (isChangingState)
        {
            hasPendingStateRequest = true;
            pendingStateRequest = newState;

            if (debugLog)
                Debug.Log($"[FSM] Cambio encolado: {newState}");

            return;
        }

        isChangingState = true;

        // EXIT
        currentStateInstance?.Exit();

        // Actualizamos estado
        CurrentState = newState;

        if (debugLog)
            Debug.Log($"[FSM] Cambio ejecutado: nuevo estado = {CurrentState}");

        try
        {
            OnStateChanged?.Invoke(CurrentState);
        }
        catch { }

        // Instancia del nuevo estado
        if (!stateInstances.TryGetValue(newState, out currentStateInstance) || currentStateInstance == null)
        {
            isChangingState = false;
            return;
        }

        // ENTER
        currentStateInstance.Enter();

        isChangingState = false;
    }


    // Resetea el FSM al estado inicial
    public void ResetState()
    {
        CurrentState = EnemyState.Patrol; // Patrol como inicial
        try
        {
            OnStateChanged?.Invoke(CurrentState);
        }
        catch
        {
            // silencioso
        }

        // Instanciamos e iniciamos el estado inicial
        if (stateInstances.ContainsKey(CurrentState))
        {
            currentStateInstance = stateInstances[CurrentState];
            currentStateInstance.Enter();
        }
    }

    #region Public Handlers
    public void OnPatrol() => ChangeState(EnemyState.Patrol);
    public void OnIdle() => ChangeState(EnemyState.Idle);
    public void OnChase() => ChangeState(EnemyState.Chase);
    public void OnStun() => ChangeState(EnemyState.Stun);
    public void OnDeath() => ChangeState(EnemyState.Death);
    public void OnTurnTuTarget(Transform target)
    {
        // Obtenemos la instancia del estado TurnToTarget
        if (stateInstances.TryGetValue(EnemyState.TurnToTarget, out IEnemyState state))
        {
            TurnToTargetState turnState = state as TurnToTargetState;
            if (turnState != null)
            {
                turnState.SetTarget(target);
            }
        }

        // Cambiamos al estado TurnToTarget
        ChangeState(EnemyState.TurnToTarget);
    }
    #endregion

    #region Public API
    public void ForceResetToPatrol()
    {
        // Limpiar estado interno
        isChangingState = false;
        hasPendingStateRequest = false;

        // Forzar estado PATROL
        CurrentState = EnemyState.Patrol;

        // Obtener la instancia del estado
        if (stateInstances.TryGetValue(EnemyState.Patrol, out IEnemyState patrolState))
        {
            currentStateInstance = patrolState;
            currentStateInstance.Enter(); // Reiniciamos su lógica
        }

        // Avisamos a quien esté escuchando
        OnStateChanged?.Invoke(CurrentState);
    }
    #endregion
}