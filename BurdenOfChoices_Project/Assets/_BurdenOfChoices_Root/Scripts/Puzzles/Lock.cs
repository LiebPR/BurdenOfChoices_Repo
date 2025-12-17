using System.Threading;
using UnityEngine;

public class Lock : MonoBehaviour
{
    #region Inspector States
    [SerializeField] Cell ownerCell;
    #endregion

    #region Internal States
    bool isLocked = true;
    #endregion

    #region Getters
    public bool IsLocked => isLocked;
    #endregion

    //Public API
    public void UnLock()
    {
        if(!isLocked) return;

        isLocked = false;

        ownerCell.NotifyLockOpened(this);

        //Desaparación física y visual
        gameObject.SetActive(false);
    }
}
