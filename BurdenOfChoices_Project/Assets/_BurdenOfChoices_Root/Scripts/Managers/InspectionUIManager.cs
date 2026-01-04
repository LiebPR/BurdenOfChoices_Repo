using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InspectionUIManager : MonoBehaviour
{
    public static InspectionUIManager Instance;

    [SerializeField] GameObject inspectionUI;
    [SerializeField] Button exitButton;

    InspectableObject currentInspectable;

    private void Awake()
    {
        currentInspectable = GetComponent<InspectableObject>();

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if(inspectionUI != null)
            inspectionUI.SetActive(false);

        if (exitButton != null)
            exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void Update()
    {
        // Si hay un objeto inspeccionable activo y se presiona ESC, simula click en botón
        if (currentInspectable != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnExitButtonClicked();
        }
    }

    public void Show()
    {
        if (inspectionUI != null)
            inspectionUI.SetActive(true);
    }

    public void Hide()
    {
        if (inspectionUI != null)
            inspectionUI.SetActive(false);
    }

    //Se llama al pulsar ESC o el botón
    public void OnExitButtonClicked()
    {
        if(currentInspectable != null)
            currentInspectable.ExitInspection();
    }
}
