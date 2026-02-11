using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InspectionUIManager : MonoBehaviour
{
    public static InspectionUIManager Instance;

    [SerializeField] GameObject inspectionUI;
    [SerializeField] Button exitButton;
    [SerializeField] GameObject inspectionPanel;

    [SerializeField] List<InspectableObject> currentInspectables = new List<InspectableObject>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inspectionUI != null)
            inspectionUI.SetActive(false);
        if (inspectionPanel != null)
            inspectionPanel.SetActive(false);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void Update()
    {
        // Salir de inspección con ESC
        if (currentInspectables.Count > 0 && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnExitButtonClicked();
        }
    }

    /// <summary>
    /// Registra un objeto inspeccionable que entra a inspección
    /// </summary>
    public void RegisterInspectable(InspectableObject obj)
    {
        if (!currentInspectables.Contains(obj))
        {
            currentInspectables.Add(obj);
            Show();
        }
    }

    /// <summary>
    /// Muestra la UI
    /// </summary>
    public void Show()
    {
        if (inspectionUI != null)
            inspectionUI.SetActive(true);
        if (inspectionPanel != null)
            inspectionPanel.SetActive(true);
    }

    /// <summary>
    /// Oculta la UI
    /// </summary>
    public void Hide()
    {
        if (inspectionUI != null)
            inspectionUI.SetActive(false);
        if (inspectionPanel != null)
            inspectionPanel.SetActive(false);
    }

    /// <summary>
    /// Sale de inspección de todos los objetos registrados
    /// </summary>
    public void OnExitButtonClicked()
    {
        foreach (var inspectable in currentInspectables)
        {
            if (inspectable != null)
                inspectable.ExitInspection();
        }
        currentInspectables.Clear();
        Hide();
    }
}
