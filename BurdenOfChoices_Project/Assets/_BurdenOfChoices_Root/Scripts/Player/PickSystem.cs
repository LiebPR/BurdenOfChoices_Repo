using System;
using UnityEngine;

public class PickSystem : MonoBehaviour
{
    #region Inspector States
    [Header("Pick Config")]
    [Tooltip("Distancia a la que el jugador puede coger un objeto")]
    [SerializeField] float pickRange = 1f; //radio de la esfera de recogida

    [Header("References")]
    [SerializeField] PlayerHand playerHand; //mano del jugador
    [SerializeField] Transform pickOrigin; //punto desde donde se dispara el raycast
    #endregion

    #region Internal States
    PickableBehaviour currentPickable;
    ICatcher catcher;
    #endregion

    #region Eventos
    public static event Action<PickableBehaviour> OnPickStarted;
    public static event Action<PickableBehaviour> OnPickEnded;
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
        if (catcher == null) return;
        if (currentPickable != null) return;
        if (!TryGetPickable(out PickableBehaviour pickable)) return;

        // DRAGABLE
        var draggable = pickable.GetComponent<DraggableObject>();
        if (draggable != null)
        {
            if (!draggable.ResolveGrabPoint(playerHand.transform.root))
                return;

            var dragController = playerHand.GetComponentInParent<DraggController>();
            if (dragController != null)
                dragController.StartDrag(draggable);

            currentPickable = pickable;

            // Evento centralizado
            OnPickStarted?.Invoke(pickable);
            return;
        }

        // EQUIP NORMAL
        currentPickable = pickable;
        currentPickable.OnEquip(catcher);

        // Evento centralizado
        OnPickStarted?.Invoke(pickable);
    }

    void HandlePickReleased()
    {
        if (currentPickable == null) return;

        var draggable = currentPickable.GetComponent<DraggableObject>();
        if (draggable != null && draggable.isBeingDragged)
        {
            var dragController = playerHand.GetComponentInParent<DraggController>();
            if (dragController != null)
                dragController.StopDrag();
        }

        currentPickable.RequestDrop();

        // Evento centralizado
        OnPickEnded?.Invoke(currentPickable);

        currentPickable = null;
    }
    #endregion

    #region OverlapShere Detection
    bool TryGetPickable(out PickableBehaviour pickable)
    {
        pickable = null;

        //Detecta todos los colliders dentro del radio
        Collider[] hits = Physics.OverlapSphere(pickOrigin.position, pickRange);

        for(int i = 0; i < hits.Length; i++)
        {
            PickableBehaviour p = hits[i].GetComponentInParent<PickableBehaviour>();
            if (p != null)
            {
                pickable = p;
                return true;
            }
        }

        return false;
    }
    #endregion

    #region Public API
    public PickableBehaviour GetCurrentPickable() => currentPickable;
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pickOrigin == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pickOrigin.position, pickRange);
    }
#endif
}