using UnityEngine;

public class PickSystem : MonoBehaviour
{
    #region Inspector States
    [Header("Pick Config")]
    [Tooltip("Distancia a la que el jugador puede coger un objeto")]
    [SerializeField] float pickRange = 2f;

    [Header("References")]
    [SerializeField] PlayerHand playerHand; //mano del jugador
    [SerializeField] Transform pickOrigin; //punto desde donde se dispara el raycast
    #endregion

    #region Internal States
    PickableBehaviour currentPickable;
    ICatcher catcher;
    #endregion

    private void Awake()
    {
        if(playerHand == null)
            playerHand = FindAnyObjectByType<PlayerHand>();
        if (playerHand == null)
        {
            Debug.LogError("PickSystem: No se encontró el PlayerHand en la escena."); 
            return;
        }

        catcher = playerHand as ICatcher;
        if (catcher == null) Debug.LogError("PlayerHand no implementa ICatcher");
    }

    #region Unity Events
    private void OnEnable()
    {
        InputManager.OnCatch += HandlePickPressed;
        InputManager.OnCatchCanceled += HandlePickReleased;
    }

    private void OnDisable()
    {
        InputManager.OnCatch -= HandlePickPressed;
        InputManager.OnCatchCanceled -= HandlePickReleased;
    }
    #endregion

    #region PickLogic
    void HandlePickPressed()
    {
        if (currentPickable != null) return;
        if (catcher == null) return;

        if(!TryGetPickable(out PickableBehaviour pickable)) return;

        //Solo ejecutar OnEquip si realmente se ejecuta
        if(pickable != null)
        {
            currentPickable = pickable;
            currentPickable.OnEquip(catcher);
        }
    }

    void HandlePickReleased()
    {
        if(currentPickable == null) return;

        currentPickable.RequestDrop();
        currentPickable = null;
    }
    #endregion

    #region Raycast
    bool TryGetPickable(out PickableBehaviour pickable)
    {
        pickable = null;

        Ray ray = new Ray(pickOrigin.position, pickOrigin.forward);
        if(Physics.Raycast(ray, out RaycastHit hit, pickRange))
        {
            pickable = hit.collider.GetComponent<PickableBehaviour>();
            return pickable != null;
        }

        return false;
    }
    #endregion

    #region Public API
    public PickableBehaviour GetCurrentPickable() => currentPickable;
    #endregion
}
