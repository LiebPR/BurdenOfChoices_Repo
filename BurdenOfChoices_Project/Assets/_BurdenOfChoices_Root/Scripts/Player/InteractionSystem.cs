using UnityEngine;

/// <summary>
/// InteractionSystem: Gestiona el raycast y la interacción del jugador.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    #region General Variables
    [Header("Ray Config")]
    [SerializeField] float interactRange = 2f; //alcance del rayo
    [SerializeField] LayerMask interactMask = ~0; //capas válidas
    [SerializeField] bool debugRay = true; //mostrar rayo para depuración

    [Header("Points")]
    [SerializeField] Transform interactionPoints; //empty desde donde se dispara
    #endregion

    #region Internal States
    IInteractable currentTarget; //objeto que se está presionando
    IInteractable highlightedTarget; //objeto al que se apunta con el raycast
    #endregion 

    #region Unity Events
    private void OnEnable()
    {
        InputManager.OnGather += HandleInteractHoldStart;
        InputManager.OnGatherCanceled += HandleInteractHoldEnd;
    }

    private void OnDisable()
    {
        InputManager.OnGather -= HandleInteractHoldStart;
        InputManager.OnGatherCanceled -= HandleInteractHoldEnd;
    }

    private void Update()
    {
        HandleHighlight();
    }
    #endregion

    #region Interact Logic
    private void HandleInteractHoldStart()
    {
        if (highlightedTarget == null) return;

        currentTarget = highlightedTarget;
        currentTarget.OnPress();
    }

    private void HandleInteractHoldEnd()
    {
        if(currentTarget == null) return;

        currentTarget.OnRelease();
        currentTarget = null;
    }
    #endregion

    #region Highlight Logic
    private void HandleHighlight()
    {
        if(interactionPoints == null) return;

        //Rayo hacia adelante desde el interactionPoint
        Ray ray = new Ray(interactionPoints.position, interactionPoints.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask);

        if (debugRay)
        {
            Debug.DrawRay(interactionPoints.position, interactionPoints.forward * interactRange, hitSomething ? Color.green : Color.red);
        }

        if (hitSomething)
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if(interactable != null)
            {
                //Cambio de target, aplicar highlight
                if(highlightedTarget != interactable)
                {
                    RemoveHighlight();
                    highlightedTarget = interactable;
                    highlightedTarget.OnHighlight();
                }
                return;
            }
        }
        RemoveHighlight();
    }

    void RemoveHighlight()
    {
        if(highlightedTarget == null) return;

        highlightedTarget.OnRemoveHighlight();
        highlightedTarget = null;
    }
    #endregion
}
