using UnityEngine;

public class EquipableItem : MonoBehaviour, IInteractable
{
    [SerializeField] EquipableData data;

    #region References
    Transform catcherPoint;
    PickableBehaviour pickable;
    ThrowableBehaviour throwable;
    #endregion

    void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        throwable = GetComponent<ThrowableBehaviour>();

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
            catcherPoint = player.transform.Find("CathcherPoint");
    }

    public void OnPress()
    {
        if (!data.isEquipped)
            Equip();
        else
            Throw();
    }

    public void OnRelease() 
    {
        if (data.isEquipped && pickable != null)
        {
            data.isEquipped = false;
            pickable.Drop();
        }
    }
    public void OnHighlight() { }
    public void OnRemoveHighlight() { }

    #region Equip & Throw Logic
    void Equip()
    {
        if (catcherPoint == null) return;
        if (pickable == null) return;

        data.isEquipped = true;
        pickable.Pick(catcherPoint);
    }

    void Throw()
    {
        if (catcherPoint == null) return;
        if (throwable == null) return;

        data.isEquipped = false;
        throwable.OnThrow(catcherPoint);
    }
    #endregion
}