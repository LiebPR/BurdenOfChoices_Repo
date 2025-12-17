using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    #region References
    PickableBehaviour pickable;
    PlayerHand playerHand;
    Renderer meshRenderer;
    DataProvider dataProvider;
    #endregion

    #region Getters
    EquipableData Data => dataProvider != null ? dataProvider.GetData<EquipableData>() : null;
    #endregion

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        dataProvider = GetComponent<DataProvider>();
        meshRenderer = GetComponentInChildren<Renderer>();

        playerHand = FindAnyObjectByType<PlayerHand>();
        if(playerHand == null)
        {
            Debug.LogWarning("No se encontró PlayerHand en la escena.");
        }
    }

    #region IInteractable
    public void OnPress()
    {
        if (playerHand == null || pickable == null) return;

        ICatcher carcher = playerHand.GetComponent<ICatcher>();
        pickable.OnEquip(carcher);
    }

    public void OnRelease()
    {
        if(pickable == null) return;

        pickable.RequestDrop();
    }

    public void OnHighlight()
    {
        if (meshRenderer == null || Data == null) return;

        meshRenderer.material = Data.highlightMaterial;
    }

    public void OnRemoveHighlight()
    {
        if (meshRenderer == null || Data == null) return;

        meshRenderer.material = Data.originalMaterial;
    }
    #endregion

    #region Collision Logic
    private void OnTriggerEnter(Collider other)
    {
        if (!pickable.IsCatched) return; // Solo funciona si la llave está cogida

        Lock lockComponent = other.GetComponent<Lock>();
        if (lockComponent == null) return;

        if (!lockComponent.IsLocked) return;

        lockComponent.UnLock(); // Desbloquea y apaga el candado
        Destroy(gameObject);    // Consume la llave
    }
    #endregion
}
