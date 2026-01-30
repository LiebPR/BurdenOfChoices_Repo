using UnityEngine;

public class UIButtonToggleUI : MonoBehaviour
{
    #region References
    [SerializeField] GameObject targetUI;
    #endregion 

    #region UI Callback

    public void ToggleUI()
    {
        if (targetUI == null)
            return;

        targetUI.SetActive(!targetUI.activeSelf);
    }

    public void DisableUI()
    {
        if (targetUI == null)
            return;

        targetUI.SetActive(false);
    }

    public void EnableUI()
    {
        if (targetUI == null)
            return;

        targetUI.SetActive(true);
    }

    #endregion
}
