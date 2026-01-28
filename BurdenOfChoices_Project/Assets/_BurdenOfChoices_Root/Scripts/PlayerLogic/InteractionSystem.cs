using UnityEngine;

/// <summary>
/// InteractionSystem: Gestiona el raycast y la interacción del jugador.
/// </summary>
public class InteractionSystem : MonoBehaviour
{
    #region Inspector States
    [Header("References")]
    [SerializeField] PickSystem pickSystem; //para ignorar el objeto cogido

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
    #endregion

    private void Update()
    {
        HandleHighlight();
    }

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

        Ray ray = new Ray(interactionPoints.position, interactionPoints.forward);

        //Obtener todos los hits del raycast
        RaycastHit[] hits = Physics.RaycastAll(ray, interactRange, interactMask);

        //Objeto que está en la mano
        PickableBehaviour pickedObject = pickSystem != null ? pickSystem.GetCurrentPickable() : null;

        IInteractable closestInteractable = null;
        float closestDistance = Mathf.Infinity;

        foreach(var hit in hits)
        {
            //Ignorar el objeto que está cogido
            if (pickedObject != null && hit.collider.gameObject == pickedObject.gameObject) continue;

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if(interactable != null)
            {
                float distance = Vector3.Distance(interactionPoints.position, hit.point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }

        //Aplicar hightlight al interactable más cercano
        if(closestInteractable != highlightedTarget)
        {
            RemoveHighlight();
            highlightedTarget = closestInteractable;
            if(highlightedTarget != null)
            {
                highlightedTarget.OnHighlight();
            }
        }
#if UNITY_EDITOR
        if (debugRay)
        {
            Debug.DrawRay(interactionPoints.position, interactionPoints.forward * interactRange,
                closestInteractable != null ? Color.green : Color.red);
        }
#endif
    }

    void RemoveHighlight()
    {
        if(highlightedTarget == null) return;

        highlightedTarget.OnRemoveHighlight();
        highlightedTarget = null;
    }
    #endregion
}
