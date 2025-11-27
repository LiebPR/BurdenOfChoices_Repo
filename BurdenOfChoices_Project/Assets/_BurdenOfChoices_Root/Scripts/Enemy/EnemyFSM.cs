using UnityEngine;

public enum EnemyState
{
    Patrol,
    Idle,
    Alert,
    Chase,
    Stun,
    Death
}
public class EnemyFSM : MonoBehaviour
{
    [SerializeField] bool debugLog;

    #region Getter
    public EnemyState CurrentState { get; private set; } = EnemyState.Patrol;
    #endregion

    #region Events
    public event System.Action<EnemyState> OnStateChanged; //Se dispara cuando cambia de estado
    #endregion

    //Cambia de estado si es diferente al actual
    void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState) return; //no cambiar si ya está en ese estado
        if(debugLog) Debug.Log($"Enemy change from {CurrentState} to {newState}");
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState); //dispara el evento de cambio de estado
    }

    public void ResetState()
    {
        CurrentState = EnemyState.Patrol; // Patrol como inicial.
        OnStateChanged?.Invoke(CurrentState); //dispara el evento de cambio de estadp
    }

    #region Public Handlers
    public void OnPatrol() => ChangeState(EnemyState.Patrol);
    public void OnIdle() => ChangeState(EnemyState.Idle);
    public void OnAlert() => ChangeState(EnemyState.Alert);
    public void OnChase() => ChangeState(EnemyState.Chase);
    public void OnStun() => ChangeState(EnemyState.Stun);
    public void OnDeath() => ChangeState(EnemyState.Death);
    #endregion
}
