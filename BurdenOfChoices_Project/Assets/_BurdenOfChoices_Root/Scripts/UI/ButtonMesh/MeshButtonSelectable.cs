using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeshButtonSelectable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] MeshRenderer targetMesh;
    [SerializeField] Material highlightMaterial;
    [SerializeField] Material selectedMaterial;
    [SerializeField] string sceneToLoad;

    [Header("Flow Settings")]
    [SerializeField] bool canInteractBeforeStart = false; // si false, bloquea hasta Start UI

    Camera mainCamera;
    CameraPriorityButton cameraPriorityButton;

    bool iSelected = false;
    bool isSelectable = false;

    private Material originalMaterial;

    private void Awake()
    {
        mainCamera = Camera.main;

        cameraPriorityButton = GetComponent<CameraPriorityButton>();
        if (cameraPriorityButton == null)
            Debug.LogWarning("CameraPriorityButton no asignado en " + name);

        if (targetMesh == null)
        {
            Debug.LogError("Asignar targetMesh en " + name);
            return;
        }

        originalMaterial = targetMesh.material;

        // Bloqueamos interacción hasta Start
        if (!canInteractBeforeStart)
            isSelectable = false;
    }

    public void SetSelectable(bool value)
    {
        isSelectable = value;
        if (isSelectable && !iSelected)
            targetMesh.material = originalMaterial;
    }

    public void OnClick()
    {
        if (!isSelectable) return;

        if (iSelected) return;

        iSelected = true;
        targetMesh.material = selectedMaterial != null ? selectedMaterial : originalMaterial;

        cameraPriorityButton?.OnButtonCameraPressed();

        FlowManager.Instance?.OnPlantSelected(this);
    }

    public void Deselect()
    {
        iSelected = false;
        targetMesh.material = originalMaterial;
    }

    // Métodos públicos para el Highlight
    public bool IsSelected() => iSelected;
    public Material GetSelectedMaterial() => selectedMaterial;
}