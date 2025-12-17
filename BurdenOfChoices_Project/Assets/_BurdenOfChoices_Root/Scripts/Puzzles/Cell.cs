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

    private void Awake()
    {
        lockedCount = locks.Count;
    }

    #region Public API
    public void NotifyLockOpened(Lock onpenLock)
    {
        lockedCount--;
        if(lockedCount <= 0)
        {
            Win();
        }
    }
    #endregion

    void Win()
    {
        SceneManager.LoadScene(winScene);
    }
}
