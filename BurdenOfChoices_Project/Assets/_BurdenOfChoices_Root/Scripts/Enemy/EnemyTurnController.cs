using UnityEngine;
using UnityEngine.Events;

public class EnemyTurnController : MonoBehaviour
{
    #region Events
    public UnityEvent OnTurnToTargetEnter;
    public UnityEvent OnTurnToTargetExit;
    #endregion

    #region State
    public bool Turning { get; private set; }
    Transform target;
    #endregion

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public Transform GetTarget()
    {
        return target;
    }

    public void StartTurning()
    {
        if (Turning) return;
        Turning = true;
        OnTurnToTargetEnter?.Invoke();
    }

    public void StopTurning()
    {
        if (!Turning) return;
        Turning = false;
        OnTurnToTargetExit?.Invoke();
    }
}
