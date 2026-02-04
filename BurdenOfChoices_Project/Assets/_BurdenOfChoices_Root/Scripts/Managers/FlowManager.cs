using UnityEngine;

public class FlowManager : MonoBehaviour
{
    #region Instance
    public static FlowManager Instance;

    public enum FlowState
    {
        WaitingForStartButton,
        WaitingForPlantSelection,
        PlantSelectedLocked
    }

    public FlowState CurrentState { get; private set; }
    #endregion

    [Header("UI")]
    [SerializeField] LevelInfoPanel levelInfoPanel;

    private MeshButtonSelectable lockedPlant;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
            Instance = this;

        CurrentState = FlowState.WaitingForStartButton;
    }

    // Llamar desde botón Start UI
    public void OnStartButtonPressed()
    {
        CurrentState = FlowState.WaitingForPlantSelection;

        foreach (var plant in Object.FindObjectsByType<MeshButtonSelectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            plant.SetSelectable(true);
            plant.Deselect();
        }
    }

    // Llamar desde MeshButtonSelectable cuando se pulsa
    public void OnPlantSelected(MeshButtonSelectable plant)
    {
        if (CurrentState != FlowState.WaitingForPlantSelection) return;

        CurrentState = FlowState.PlantSelectedLocked;
        lockedPlant = plant;

        foreach (var p in Object.FindObjectsByType<MeshButtonSelectable>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (p != plant)
                p.SetSelectable(false);
        }

        levelInfoPanel.SetLevel(plant);
        levelInfoPanel.gameObject.SetActive(true);
    }

    // Llamar desde Escape o botón UI de volver atrás
    public void OnBackPressed()
    {
        if (CurrentState != FlowState.PlantSelectedLocked) return;

        CurrentState = FlowState.WaitingForPlantSelection;

        foreach (var plant in Object.FindObjectsByType<MeshButtonSelectable>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            plant.SetSelectable(true);
            plant.Deselect();
        }

        lockedPlant = null;
        levelInfoPanel.gameObject.SetActive(false);
    }

    public MeshButtonSelectable GetLockedPlant() => lockedPlant;
}
