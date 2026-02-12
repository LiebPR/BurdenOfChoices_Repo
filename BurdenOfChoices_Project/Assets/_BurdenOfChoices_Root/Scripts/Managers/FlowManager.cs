using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlowManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject mainCanvas; // Canvas que contiene Start Button
    [SerializeField] private Button backButton;      // Botón Back que aparece tras Start

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
            restoringFromLevel = true; // Bloqueamos inputs mientras restauramos
            CurrentState = FlowState.PlantSelectedLocked;
        }
        else
        {
            CurrentState = FlowState.WaitingForStartButton;
        }
    }
    #endregion
    bool restoringFromLevel;
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
        if (backButton != null)
            backButton.gameObject.SetActive(false);
        // Asumiendo que backButton está asignado en el inspector
        backButton.onClick.AddListener(OnBackPressed);

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
            StartCoroutine(EndRestoreLock());
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnBackPressed();
        }
    }

    // Llamar desde botón Start UI
    public void OnStartButtonPressed()
    {
        if (restoringFromLevel) return; // Protección
        CurrentState = FlowState.WaitingForPlantSelection;

        // Desactiva canvas principal
        if (mainCanvas != null)
            mainCanvas.SetActive(false);

        // Activa botón Back
        if (backButton != null)
            backButton.gameObject.SetActive(true);

        // Activa selección de plantas
        foreach (var plant in Object.FindObjectsByType<MeshButtonSelectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            plant.SetSelectable(true);
            plant.Deselect();
        }
    }


    // Llamar desde MeshButtonSelectable cuando se pulsa
    public void OnPlantSelected(MeshButtonSelectable plant)
    {
        if (restoringFromLevel) return;
        if (CurrentState != FlowState.WaitingForPlantSelection) return;

        CurrentState = FlowState.PlantSelectedLocked;
        lockedPlant = plant;

        foreach (var p in Object.FindObjectsByType<MeshButtonSelectable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (p != plant)
                p.SetSelectable(false);
        }

        // Ocultar botón Back mientras estás en el panel de info/tutorial
        if (backButton != null)
            backButton.gameObject.SetActive(false);

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
        if (restoringFromLevel) return;

        switch (CurrentState)
        {
            case FlowState.PlantSelectedLocked:
                // Volver a selección de plantas
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

                // Mostrar botón Back nuevamente
                if (backButton != null)
                    backButton.gameObject.SetActive(true);
                break;

            case FlowState.WaitingForPlantSelection:
                // Volver al canvas principal
                CurrentState = FlowState.WaitingForStartButton;

                if (mainCanvas != null)
                    mainCanvas.SetActive(true);

                if (backButton != null)
                    backButton.gameObject.SetActive(false);

                foreach (var plant in Object.FindObjectsByType<MeshButtonSelectable>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    plant.SetSelectable(false);
                    plant.Deselect();
                }

                lockedPlant = null;
                levelInfoPanel.gameObject.SetActive(false);
                tutorialPanel.HidePanel();
                break;

            default:
                // No hacer nada en WaitingForStartButton
                break;
        }
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

    IEnumerator EndRestoreLock()
    {
        yield return null; // Espera un frame
        restoringFromLevel = false; // Se permite input otra vez
    }
}
