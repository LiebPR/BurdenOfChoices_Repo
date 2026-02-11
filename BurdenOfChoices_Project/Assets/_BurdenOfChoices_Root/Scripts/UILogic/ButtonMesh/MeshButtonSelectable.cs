using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MeshButtonSelectable : MonoBehaviour
{
    #region Inspector
    [Header("Flow Settings")]
    [SerializeField] bool canInteractBeforeStart = false; // si false, bloquea hasta Start UI

    [Header("Level Info")]
    [SerializeField] LevelData levelData;
    [SerializeField] CinemachineCamera previewCamera;  //Menu / Preview
    [SerializeField] CinemachineCamera playCamera; //Nivel / Play

    [Header("Tutorial")]
    [SerializeField] bool iTutorial; //define si el botón es de tutorial o no. 

    [SerializeField] string pressButtonSFX = "SFX_UI_MeshButtonPress";
    #endregion

    #region Internal
    CameraPriorityButton cameraPriorityButton;

    bool isSelected = false;
    bool isSelectable = false;
    #endregion

    #region Getters
    public LevelData LevelData => levelData;
    public CinemachineCamera PreviewCamera => previewCamera;
    public CinemachineCamera PlayCamera => playCamera; 
    public bool IsTutorial => iTutorial;   //propiedad para acceder al estado del tutorial
    #endregion

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
        AudioManager.Instance.PlaySFX2D(pressButtonSFX);
    }

    public void Deselect()
    {
        isSelected = false;
    }

    // API para otros sistemas
    public bool IsSelected() => isSelected;
    public bool IsSelectable() => isSelectable;
    public void ForceHighlight()
    {
        isSelected = true; // primero interno
        var highlight = GetComponent<MeshButtonHighlight>();
        if (highlight != null)
        {
            highlight.ForceSelected(); // luego forzar visual
        }
    }
}