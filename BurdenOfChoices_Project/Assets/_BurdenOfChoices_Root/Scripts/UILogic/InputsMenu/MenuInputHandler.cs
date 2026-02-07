using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MenuInputHandler : MonoBehaviour
{
    #region Inspector References
    [SerializeField] Camera mainCamera;
    [SerializeField] float raycastDistance = 100f;

    [Header("Input Skip")]
    [SerializeField] float holdTimeToSkip = 1.5f;
    #endregion

    float holdTimer;
    bool isHolding;

    MenuInputAction menuInput;
    InputAction interactionAction;
    InputAction escapeAction;
    InputAction skipAction;

    #region Input Events
    public static event Action OnSkipHoldStarted;
    public static event Action OnSkipHoldCanceled;
    public static event Action<float> OnSkipHoldUpdate;
    public static event Action OnSkipConfirmed;
    #endregion

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        menuInput = new MenuInputAction();
        interactionAction = menuInput.Menu.Interaction;
        escapeAction = menuInput.Menu.Escape;
        skipAction = menuInput.Menu.Skip;
    }

    private void Update()
    {
        if (!isHolding)
            return;

        holdTimer += Time.deltaTime;
        OnSkipHoldUpdate?.Invoke(holdTimer / holdTimeToSkip);

        if (holdTimer >= holdTimeToSkip)
        {
            isHolding = false;
            OnSkipConfirmed?.Invoke();
        }
    }

    private void OnEnable()
    {
        interactionAction.Enable();
        escapeAction.Enable();
        skipAction.Enable();

        interactionAction.performed += OnInteraction;

        escapeAction.performed += OnEscape;

        skipAction.started += OnInteractionStarted;
        skipAction.canceled += OnInteractionCanceled;

        skipAction.performed += OnSkipPressed;
    }

    private void OnDisable()
    {
        interactionAction.performed -= OnInteraction;

        escapeAction.performed -= OnEscape;

        skipAction.started -= OnInteractionStarted;
        skipAction.canceled -= OnInteractionCanceled;

        interactionAction.Disable();
        escapeAction.Disable();
        skipAction.Disable();
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

    #region Callback Input Skip
    void OnInteractionStarted(InputAction.CallbackContext ctx)
    {
        isHolding = true;
        holdTimer = 0f;
        OnSkipHoldStarted?.Invoke();
    }


    void OnInteractionCanceled(InputAction.CallbackContext ctx)
    {
        isHolding = false;
        holdTimer = 0f;
        OnSkipHoldCanceled?.Invoke();
    }

    private void OnSkipPressed(InputAction.CallbackContext ctx)
    {
        // Dispara un evento que el DialogSystem escuchará
        DialogSystem activeDialog = FindAnyObjectByType<DialogSystem>();
        if (activeDialog != null)
            activeDialog.SkipOrNext();
    }
    #endregion
}
