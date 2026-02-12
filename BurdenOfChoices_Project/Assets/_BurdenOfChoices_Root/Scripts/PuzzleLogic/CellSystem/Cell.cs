using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cell : MonoBehaviour
{
    #region Inspector States
    [SerializeField] List<Lock> locks = new List<Lock>();
    [SerializeField] Animator doorAnimator;
    [SerializeField] float openDelay = 0.5f;
    #endregion

    #region Internal States
    int lockedCount;
    #endregion

    #region Event
    public event Action OnCellUnlocked;
    #endregion

    #region Animator
    static readonly int IsCellOpenHash = Animator.StringToHash("IsCellOpen");
    #endregion

    #region Getters
    public bool AreAllLocksUnlocked => lockedCount <= 0;
    #endregion

    private void Awake()
    {
        lockedCount = locks.Count;
    }

    #region Public API
    public void NotifyLockOpened(Lock onpenLock)
    {
        lockedCount = Mathf.Max(lockedCount - 1, 0);
        if (AreAllLocksUnlocked)
        {
            OnCellUnlocked?.Invoke();
            StartCoroutine(OpenDoorWithDelay());
        }
    }
    #endregion

    private IEnumerator OpenDoorWithDelay()
    {
        if (openDelay > 0f)
            yield return new WaitForSeconds(openDelay);

        if (doorAnimator != null)
        {
            doorAnimator.SetBool(IsCellOpenHash, true);
            AudioManager.Instance.PlaySFX2D("SFX_CellDoor");
        }
            
    }
}
