using UnityEngine;

public class Key : MonoBehaviour, IInteractable, IPickListener
{
    #region Inspector States
    [Header("References")]
    [SerializeField] Animator anim;

    [Header("Consume Behaviour")]
    [Tooltip("Punto de consumo para la llave")]
    [SerializeField] float destroyDelay = 1f; //Delay antes de desaparecer
    #endregion

    //Internal State
    Lock targetLock;
    bool isConsuming;

    #region References
    PickableBehaviour pickable;
    PlayerHand playerHand;
    DataProvider dataProvider;
    #endregion

    #region Animator Params
    static readonly int IsCatchHash = Animator.StringToHash("IsCatch");
    static readonly int IsConsumeHash = Animator.StringToHash("IsConsume");
    #endregion

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        dataProvider = GetComponent<DataProvider>();

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
        
    }

    public void OnRemoveHighlight()
    {
        
    }
    #endregion

    #region IPickableListener
    public void OnPick(ICatcher catcher)
    {
        if(anim != null)
        {
            anim.SetBool(IsCatchHash, true);
        }
    }

    public void OnDrop()
    {
        if (anim != null && !isConsuming)
            anim.SetBool(IsCatchHash, false);
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
        isConsuming = true;
        targetLock = lockComponent;

        // Soltar forzado
        pickable.BlockDrop();
        pickable.OnDropWithoutPhysics();

        // Mover al punto del candado
        Transform consumePoint = lockComponent.KeyConsumePoint;
        if (consumePoint != null)
        {
            //Animator 
            if (anim != null)
                anim.SetBool(IsConsumeHash, true);

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
