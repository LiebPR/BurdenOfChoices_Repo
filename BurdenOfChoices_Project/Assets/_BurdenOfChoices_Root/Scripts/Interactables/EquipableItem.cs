using UnityEngine;

public class EquipableItem : MonoBehaviour, IInteractable
{
    #region References
    PickableBehaviour pickable; //componente que gestiona el estado físico y equipar
    ThrowableBehaviour throwable; //componente que gestiona el lanzamiento
    PlayerHand playerHand;
    [SerializeField] EquipableData data;
    #endregion

    void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        throwable = GetComponent<ThrowableBehaviour>();

        if(playerHand == null)
        {
            playerHand = FindAnyObjectByType<PlayerHand>();
            if(playerHand == null)
            {
                Debug.LogWarning("No se encontro ningún PlayerHand en la escena.");
            }
        }
    }

    #region Interaction
    public void OnPress()
    {
        if(playerHand == null) return;

        ICatcher catcher = playerHand.GetComponent<ICatcher>();
        pickable.OnEquip(catcher);
    }

    public void OnRelease() 
    {
        pickable.OnDrop();
    }

    public void OnHighlight()
    {

    }

    public void OnRemoveHighlight()
    {

    }
    #endregion
}