using UnityEngine;

public class Key : MonoBehaviour, IInteractable
{
    #region Inspector States
    [Header("Consume Behaviour")]
    [Tooltip("Punto de consumo para la llave")]
    [SerializeField] float destroyDelay = 1f; //Delay antes de desaparecer
    #endregion

    //Internal State
    Lock targetLock;

    #region References
    PickableBehaviour pickable;
    PlayerHand playerHand;
    Renderer meshRenderer;
    DataProvider dataProvider;
    Rigidbody rb;
    Collider col;
    #endregion

    #region Getters
    EquipableData Data => dataProvider != null ? dataProvider.GetData<EquipableData>() : null;
    #endregion

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        dataProvider = GetComponent<DataProvider>();
        meshRenderer = GetComponentInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();
        col = GetComponentInChildren<Collider>();

        playerHand = FindAnyObjectByType<PlayerHand>();
        if(playerHand == null)
        {
            Debug.LogWarning("No se encontró PlayerHand en la escena.");
        }
    }

    #region IInteractable
    public void OnPress()
    {
    }

    public void OnRelease()
    {
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

        ConsumeKey(lockComponent);
    }
    #endregion

    #region Private API
    void ConsumeKey(Lock lockComponent)
    {
        targetLock = lockComponent;
        // Soltar forzado
        pickable.OnDrop(true);

        // Apagar físicas
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        if (col != null)
            col.isTrigger = true;

        // Mover al punto del candado
        Transform consumePoint = lockComponent.KeyConsumePoint;
        if (consumePoint != null)
        {
            transform.SetParent(null);
            transform.position = consumePoint.position;
            transform.rotation = consumePoint.rotation;
        }

        Invoke(nameof(FinishConsume), destroyDelay);
    }

    void FinishConsume()
    {
        if(targetLock != null && targetLock.IsLocked)
        {
            targetLock.UnLock();
        }

        Destroy(gameObject);
    }
    #endregion
}
