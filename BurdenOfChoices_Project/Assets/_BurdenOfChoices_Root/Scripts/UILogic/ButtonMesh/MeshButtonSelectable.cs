using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeshButtonSelectable : MonoBehaviour
{
    [Header("Flow Settings")]
    [SerializeField] bool canInteractBeforeStart = false; // si false, bloquea hasta Start UI

    CameraPriorityButton cameraPriorityButton;

    bool isSelected = false;
    bool isSelectable = false;

    private void Awake()
    {
        cameraPriorityButton = GetComponent<CameraPriorityButton>();

    }

    private void Start()
    {
        if (!canInteractBeforeStart)
            isSelectable = false;
    }

    public void SetSelectable(bool value)
    {
        isSelectable = value;
    }

    public void OnClick()
    {
        if (!isSelectable) return;

        isSelected = true;
        cameraPriorityButton?.OnButtonCameraPressed();
        FlowManager.Instance?.OnPlantSelected(this);
    }

    public void Deselect()
    {
        isSelected = false;
    }

    // API para otros sistemas
    public bool IsSelected() => isSelected;
    public bool IsSelectable() => isSelectable;
}