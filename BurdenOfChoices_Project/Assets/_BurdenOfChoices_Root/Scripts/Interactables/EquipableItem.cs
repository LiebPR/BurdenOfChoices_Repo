using UnityEngine;

public class EquipableItem : MonoBehaviour, IInteractable
{
    [SerializeField] EquipableData data;

    #region References
    #endregion

    void Awake()
    {
    }

    public void OnPress()
    {
    }

    public void OnRelease() 
    {
    }
    public void OnHighlight() { }
    public void OnRemoveHighlight() { }
}