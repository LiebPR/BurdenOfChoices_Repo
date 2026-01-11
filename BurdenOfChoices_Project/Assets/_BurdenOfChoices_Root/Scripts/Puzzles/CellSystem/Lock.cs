using System.Threading;
using UnityEngine;

public class Lock : MonoBehaviour
{
    #region Inspector States
    [SerializeField] Cell ownerCell;
    [SerializeField] Animator anim;
    [SerializeField] Transform keyConsumePoint;

    [Header("Disable Timings")]
    [SerializeField] float lockDisableDelay = 1f;
    [SerializeField] float chainFadeDelay = 0.3f;

    [SerializeField] GameObject chain;
    #endregion

    #region Internal States
    bool isLocked = true;
    #endregion

    #region Aniamtions Parameters
    static readonly int IsLockhash = Animator.StringToHash("IsLock");
    #endregion

    #region Getters
    public bool IsLocked => isLocked;
    public Transform KeyConsumePoint => keyConsumePoint; 
    #endregion

    //Public API
    public void UnLock()
    {
        if(!isLocked) return;

        isLocked = false;

        ownerCell.NotifyLockOpened(this);

        if(anim != null)
        {
            anim.SetBool(IsLockhash, true); //dispara aniamción UnLock
        }

        //Desaparación física y visual
        Invoke(nameof(DisableLock), lockDisableDelay);
    }

    void DisableLock()
    {
        gameObject.SetActive(false);

        chain.SetActive(false);
    }
}
