using UnityEngine;
using System.Collections;

public class FlowManager : MonoBehaviour
{
    #region Instance
    public static FlowManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (GameFlowContext.ReturnFromLevel)
        {
            CurrentState = FlowState.PlantSelectedLocked;
        }
        else
        {
            CurrentState = FlowState.WaitingForStartButton;
        }
    }
    #endregion

    public enum FlowState
    {
        WaitingForStartButton,
        WaitingForPlantSelection,
        PlantSelectedLocked
    }

    public FlowState CurrentState { get; private set; }

    [Header("UI")]
    [SerializeField] LevelInfoPanel levelInfoPanel;
    [SerializeField] TutorialInfoPanel tutorialPanel;

    private MeshButtonSelectable lockedPlant;

    private void Start()
    {
        if (GameFlowContext.ReturnFromLevel)
        {
            var targetPlant = FindPlantByLevelData(GameFlowContext.LastPlayedLevel);
            if (targetPlant != null)
            {
                lockedPlant = targetPlant;
                lockedPlant.SetSelectable(true);

                // Forzar highlight con retraso de un frame
                StartCoroutine(HighlightAfterInitialization(targetPlant));

                CurrentState = FlowState.PlantSelectedLocked;

                foreach (var plant in Object.FindObjectsByType<MeshButtonSelectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    plant.SetSelectable(plant == targetPlant);
                    plant.Deselect();
                }

                // Cámara de preview
                if (CameraManager.Instance != null && targetPlant.PreviewCamera != null)
                    CameraManager.Instance.ActivateCamera(targetPlant.PreviewCamera);

                levelInfoPanel.SetLevel(targetPlant);
                levelInfoPanel.gameObject.SetActive(true);

                GameFlowContext.Clear();
            }
        }
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

        foreach (var p in Object.FindObjectsByType<MeshButtonSelectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (p != plant)
                p.SetSelectable(false);
        }

        //Aquí cambiamos el comportamiento según si es un tutorial
        if (plant.IsTutorial)
        {
            tutorialPanel.SetTutorial(plant.LevelData.levelName);
            tutorialPanel.gameObject.SetActive(true);
        }
        else
        {
            levelInfoPanel.SetLevel(plant);
            levelInfoPanel.gameObject.SetActive(true);
        }
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
        tutorialPanel.HidePanel();
    }

    MeshButtonSelectable FindPlantByLevelData(LevelData data)
    {
        foreach (var plant in Object.FindObjectsByType<MeshButtonSelectable>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (plant.LevelData == data)
                return plant;
        }

        return null;
    }

    IEnumerator HighlightAfterInitialization(MeshButtonSelectable targetPlant)
    {
        // Espera un frame para que Awake/Start de todos los highlights se ejecute
        yield return null;

        // Asegura que el botón está seleccionado internamente
        targetPlant.ForceHighlight();

        // Aplica el material de highlight inmediatamente
        var highlight = targetPlant.GetComponent<MeshButtonHighlight>();
        if (highlight != null)
            highlight.ApplyHighlightImmediately();
    }

    public MeshButtonSelectable GetLockedPlant() => lockedPlant;
}
