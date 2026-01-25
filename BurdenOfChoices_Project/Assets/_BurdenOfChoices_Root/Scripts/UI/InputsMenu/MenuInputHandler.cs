using UnityEngine;
using UnityEngine.InputSystem;

public class MenuInputHandler : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] float raycastDistance = 100f;

    MenuInputAction menuInput;
    InputAction interactionAction;
    InputAction escapeAction;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        menuInput = new MenuInputAction();
        interactionAction = menuInput.Menu.Interaction;
        escapeAction = menuInput.Menu.Escape;
    }

    private void OnEnable()
    {
        interactionAction.Enable();
        escapeAction.Enable();

        interactionAction.performed += OnInteraction;
        escapeAction.performed += OnEscape;
    }

    private void OnDisable()
    {
        interactionAction.performed -= OnInteraction;
        escapeAction.performed -= OnEscape;

        interactionAction.Disable();
        escapeAction.Disable();
    }

    private void OnInteraction(InputAction.CallbackContext context)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance))
        {
            var button = hit.transform.GetComponent<MeshButtonSelectable>();
            if (button != null)
                button.OnClick();
        }
    }

    private void OnEscape(InputAction.CallbackContext context)
    {
        // Aquí puedes cerrar paneles o volver al menú anterior
    }
}
