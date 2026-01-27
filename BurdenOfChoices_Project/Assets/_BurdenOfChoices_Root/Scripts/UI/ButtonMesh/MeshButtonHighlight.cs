using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshButtonSelectable))]
public class MeshButtonHighlight : MonoBehaviour
{
    MeshButtonSelectable button;
    IButtonVisual visual;
    Camera mainCamera;

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
            return;
        }

        if (flow == FlowManager.FlowState.PlantSelectedLocked)
        {
            if (button.IsSelected())
                visual.SetSelected();
            else
                visual.SetDisabled();

            return;
        }

        // Hover normal
        if (!button.IsSelectable())
        {
            visual.SetDisabled();
            return;
        }

        visual.SetNormal();

        if (IsHovering())
            visual.SetHover();

        if (button.IsSelected())
            visual.SetSelected();
    }

    bool IsHovering()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        return Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform;
    }
}
