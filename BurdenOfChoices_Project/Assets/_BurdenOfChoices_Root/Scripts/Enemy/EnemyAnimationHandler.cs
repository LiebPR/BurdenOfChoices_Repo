using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    #region Inpector References
    public Animator body;
    public Animator legs;
    public Animator farol;
    #endregion  

    #region Animator Parameters
    static readonly int VelocityHash = Animator.StringToHash("Velocity");
    static readonly int IsRunningHash = Animator.StringToHash("IsRunning");
    static readonly int IsStunnedhash = Animator.StringToHash("IsStunned");
    static readonly int IsDeathHash = Animator.StringToHash("IsDeath");
    #endregion

    #region Velocity
    public void SetVelocityBody(float velocity)
    {
        body.SetFloat(VelocityHash, velocity);
        farol.SetFloat(VelocityHash, velocity);
    }

    public void SetVelocityLegs(float velocity)
    {
        legs.SetFloat(VelocityHash, velocity);
    }
    #endregion

    #region Running
    public void SetIsRunningBody(bool isRunning)
    {
        body.SetBool(IsRunningHash, isRunning);
        farol.SetBool(IsRunningHash, isRunning);
    }

    public void SetIsRunningLegs(bool isRunning)
    {
        legs.SetBool(IsRunningHash, isRunning);
    }
    #endregion
}
