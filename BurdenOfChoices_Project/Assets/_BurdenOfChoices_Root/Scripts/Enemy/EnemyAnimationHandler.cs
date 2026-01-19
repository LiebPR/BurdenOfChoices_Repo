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
    static readonly int IStunHash = Animator.StringToHash("IStun");
    static readonly int IsDeathAfterStunHash = Animator.StringToHash("DeathAfterDelay");
    static readonly int IDeathHash = Animator.StringToHash("IDeath"); 
    static readonly int IsTurnningHash = Animator.StringToHash("IsTurnning");
    static readonly int TurnDirectionHash = Animator.StringToHash("Angular");
    static readonly int IHearHash = Animator.StringToHash("IHear");
    static readonly int IAttackingHash = Animator.StringToHash("IAttacking");
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
    public void SetDeathBody()
    {
        body.SetTrigger(IDeathHash);
        farol.SetTrigger(IDeathHash);
    }

    public void SetDeathLegs()
    {
        legs.SetTrigger(IDeathHash);
    }

    public void SetDeathAfterStunBody(bool isDeath)
    {
        body.SetBool(IsDeathAfterStunHash, isDeath);
        farol.SetBool(IsDeathAfterStunHash, isDeath);
    }

    public void SetDeathAfterStunLegs(bool isDeath)
    {
        legs.SetBool (IsDeathAfterStunHash, isDeath);
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

    public void SetStunBody()
    {
        body.SetTrigger(IStunHash);
        farol.SetTrigger(IStunHash);
    }

    public void SetStunLegs()
    {
        legs.SetTrigger(IStunHash);
    }
    #endregion

    #region Attack
    public void SetAttackBody()
    {
        body.SetTrigger(IAttackingHash);
        farol.SetTrigger(IAttackingHash);
    }
    #endregion
}
