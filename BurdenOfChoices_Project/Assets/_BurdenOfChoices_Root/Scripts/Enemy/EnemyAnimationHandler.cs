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
    static readonly int IsStunnedHash = Animator.StringToHash("IsStunned");
    static readonly int IsDeathHash = Animator.StringToHash("IsDeath");
    static readonly int IsTurnningHash = Animator.StringToHash("IsTurnning");
    static readonly int TurnDirectionHash = Animator.StringToHash("Angular");
    static readonly int IHearHash = Animator.StringToHash("IHear");
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

    #region Turnning
    public void SetTurnningBody(bool isTurnning)
    {
        body.SetBool(IsTurnningHash, isTurnning);
        farol.SetBool(IsTurnningHash, isTurnning);
    }

    public void SetTurnDirection(float dir)
    {
        body.SetFloat(TurnDirectionHash, dir);
        farol.SetFloat(TurnDirectionHash, dir);
    }
    #endregion

    #region Hear
    public void SetHearBody()
    {
        body.SetTrigger(IHearHash);
        farol.SetTrigger(IHearHash);
    }
    #endregion

    #region Death
    public void SetDeathBody(bool isDeath)
    {
        body.SetBool(IsDeathHash, isDeath);
        farol.SetBool(IsDeathHash, isDeath);
    }

    public void SetDeathLegs(bool isDeath)
    {
        legs.SetBool (IsDeathHash, isDeath);
    }
    #endregion

    #region Stunned
    public void SetStunnedBody(bool isStunned)
    {
        body.SetBool(IsStunnedHash, isStunned);
        farol.SetBool(IsStunnedHash, isStunned);
    }

    public void SetStunnedLegs(bool isStunned)
    {
        legs.SetBool(IsStunnedHash, isStunned);
    }
    #endregion
}
