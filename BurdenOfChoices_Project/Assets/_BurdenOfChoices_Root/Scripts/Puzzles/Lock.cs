using System.Threading;
using UnityEngine;

public class Lock : MonoBehaviour
{
    #region Inspector States
    [SerializeField] Cell ownerCell;

    [Header("Consume Point")]
    [Tooltip("Punto al que acudiara la llave cuando impacte con el candado")]
    [SerializeField] Transform keyConsumePoint; 
    #endregion

    #region Internal States
    bool isLocked = true;
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

        //Desaparación física y visual
        gameObject.SetActive(false);
    }
}
