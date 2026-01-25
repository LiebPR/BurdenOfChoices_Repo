using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MeshButtonSelectable))]
public class MeshButtonHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] MeshRenderer targetMesh;
    [SerializeField] Material highlightMaterial;

    private Material originalMaterial;
    private MeshButtonSelectable meshButton;
    private Camera mainCamera;

    private void Awake()
    {
        if (targetMesh == null)
        {
            Debug.LogError("Asignar targetMesh en " + name);
            return;
        }

        meshButton = GetComponent<MeshButtonSelectable>();
        mainCamera = Camera.main;

        originalMaterial = targetMesh.material;
    }

    private void Update()
    {
        if (mainCamera == null || Mouse.current == null) return;

        // Bloqueo antes de Start
        if (FlowManager.Instance != null &&
            FlowManager.Instance.CurrentState == FlowManager.FlowState.WaitingForStartButton)
        {
            targetMesh.material = originalMaterial;
            return;
        }

        // Mantener material de selección si está seleccionado
        if (meshButton != null && meshButton.IsSelected())
        {
            targetMesh.material = meshButton.GetSelectedMaterial() ?? originalMaterial;
            return;
        }

        // Hover
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        bool isHovering = Physics.Raycast(ray, out RaycastHit hit) && hit.transform == transform;

        targetMesh.material = isHovering ? highlightMaterial : originalMaterial;
    }
}
