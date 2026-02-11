using UnityEngine;
using UnityEngine.EventSystems;

public class MenuNavigationController : MonoBehaviour
{
    #region References
    [SerializeField] GameObject firstSelectedButton;
    #endregion

    void OnEnable()
    {
        SetInitialSelection();
    }

    #region Internal Logic
    void SetInitialSelection()
    {
        if (firstSelectedButton == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
    #endregion
}
