using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Cell : MonoBehaviour
{
    #region Inspector States
    [SerializeField] List<Lock> locks = new List<Lock>();
    [SerializeField] string winScene = "SCN:WinMenu";
    #endregion

    #region Internal States
    int lockedCount;
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
    }
    #endregion
}
