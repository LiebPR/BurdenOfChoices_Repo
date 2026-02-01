using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField] EnemyAttack attack;
    [SerializeField] Transform enemy;

    #region Attack Events
    public void OnAttackHit()
    {
        if (attack == null) return;

        attack.ResolveAttackHit();
    }

    public void OnPlaySoundAttack()
    {
        if (attack == null) return;
        AudioManager.Instance.PlayAnimationSFX("SFX_Sectario_Attack", enemy, 5f);
    }
    #endregion

    #region Death Events
    public void OnPlaySoundDeath()
    {
        AudioManager.Instance.PlayAnimationSFX("SFX_Sectario_Death", enemy, 5f);
    }
    #endregion

    #region Stun Events
    public void OnPlaySoundDeathStun()
    {
        AudioManager.Instance.PlayAnimationSFX("SFX_Sectario_DeathStun", enemy, 5f);
    }

    public void OnPlaySoundStun()
    {
        AudioManager.Instance.PlayAnimationSFX("SFX_Sectario_Stun", enemy, 5f);
    }
    #endregion

    #region Hear Events
    public void OnPlaySoundHear()
    {
        Debug.Log("Te he escuchado Bobo");
        AudioManager.Instance.PlayAnimationSFX("SFX_Sectario_Hear", enemy, 5f);
    }
    #endregion

    #region Idle Events
    public void OnPlaySoundIdle()
    {
        AudioManager.Instance.PlayAnimationSFX("SFX_Sectario_Idle", enemy, 5f);
    }
    #endregion 

}
