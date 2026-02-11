using UnityEngine;
using UnityEngine.InputSystem;

public class UIInputModeManager : MonoBehaviour
{
    public static UIInputModeManager Instance;

    public enum InputMode
    {
        Mouse,
        Navigation
    }

    [SerializeField] float switchCooldown = 0.3f; // tiempo de bloqueo tras usar navegación

    public InputMode CurrentMode { get; private set; } = InputMode.Mouse;

    float lastNavigationTime;

    void Awake() => Instance = this;

    void Update()
    {
        DetectNavigationInput();
        DetectMouseHover();
    }

    void DetectNavigationInput()
    {
        bool navInput = false;

        if (Keyboard.current != null)
        {
            navInput = Keyboard.current.wKey.wasPressedThisFrame ||
                       Keyboard.current.aKey.wasPressedThisFrame ||
                       Keyboard.current.sKey.wasPressedThisFrame ||
                       Keyboard.current.dKey.wasPressedThisFrame ||
                       Keyboard.current.upArrowKey.wasPressedThisFrame ||
                       Keyboard.current.downArrowKey.wasPressedThisFrame ||
                       Keyboard.current.spaceKey.wasPressedThisFrame || // Espacio
                       Keyboard.current.enterKey.wasPressedThisFrame; // Enter
        }

        if (!navInput && Gamepad.current != null)
        {
            navInput = Gamepad.current.leftStick.ReadValue().magnitude > 0.5f ||
                       Gamepad.current.buttonSouth.wasPressedThisFrame; // Botón A / Cross
        }

        if (navInput)
        {
            CurrentMode = InputMode.Navigation;
            lastNavigationTime = Time.time;
        }
    }

    void DetectMouseHover()
    {
        // Solo puede volver a mouse mode si pasó cooldown desde último input de navegación
        if (Time.time - lastNavigationTime < switchCooldown)
            return;

        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
        {
            // Solo cambia a mouse mode si el puntero está encima del botón
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                CurrentMode = InputMode.Mouse;
            }
        }
    }

    public void ForceNavigationMode()
    {
        CurrentMode = InputMode.Navigation;
        lastNavigationTime = Time.time; // reinicia cooldown
    }
}
