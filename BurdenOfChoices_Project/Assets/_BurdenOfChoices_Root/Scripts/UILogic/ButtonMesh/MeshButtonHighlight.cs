using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshButtonSelectable))]
public class MeshButtonHighlight : MonoBehaviour
{
    MeshButtonSelectable button;
    IButtonVisual visual;
    Camera mainCamera;

    bool wasHovering = false;

    void Awake()
    {
        button = GetComponent<MeshButtonSelectable>();
        visual = GetComponent<IButtonVisual>();
        mainCamera = Camera.main;

        if (visual == null)
        {
            Debug.LogError("No hay IButtonVisual en " + name);
            enabled = false;
        }
    }

    void Update()
    {
        if (mainCamera == null || Mouse.current == null)
            return;

        var flow = FlowManager.Instance?.CurrentState;

        if (flow == FlowManager.FlowState.WaitingForStartButton)
        {
            visual.SetDisabled();
            wasHovering = false;
            return;
        }

        if (flow == FlowManager.FlowState.PlantSelectedLocked)
        {
            if (button.IsSelected())
                visual.SetSelected();
            else
                visual.SetDisabled();

            wasHovering = false;
            return;
        }

        if (!button.IsSelectable())
        {
            visual.SetDisabled();
            wasHovering = false;
            return;
        }

        bool isHovering = IsHovering();

        //Efecto puntual
        if (isHovering && !wasHovering)
        {
            visual.OnHoverEnter();
        }

        //Estado base
        if (button.IsSelected())
        {
            visual.SetSelected();
        }
        else if (isHovering)
        {
            visual.SetHover();
        }
        else
        {
            visual.SetNormal();
        }

        wasHovering = isHovering;
    }

    public void ForceSelected()
    {
        if (visual != null)
        {
            visual.SetSelected();
        }

        // Asegura que Update() no sobreescriba visual
        wasHovering = true;
    }

    public void ApplyHighlightImmediately()
    {
        // Asegurarse de que visual existe
        if (visual != null)
        {
            // Forzar material de highlight aunque no haya hover
            visual.SetSelected();
        }
    }

    bool IsHovering()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform;
    }
}
