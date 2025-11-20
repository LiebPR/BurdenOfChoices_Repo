using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// InputManager: Maneja todos los inputs del jugador usando eventos estáticos.
/// Garantiza liberar memoria correctamente y no generar warnings al cerrar la aplicación.
/// </summary>
public class InputManager : MonoBehaviour
{
    #region References
    static PlayerInputActions inputA;
    #endregion

    #region Eventos
    // MOVIMIENTO
    public static event Action<Vector2> OnMoveChanged;
    public static event Action<bool> OnRunChanged;
    public static event Action<bool> OnCrouchChanged;

    // ACCIONES
    public static event Action OnAttack;
    public static event Action OnThrow;
    public static event Action OnGather;
    public static event Action OnGatherCanceled;
    #endregion

    private void Awake()
    {
        if (inputA == null)
        {
            inputA = new PlayerInputActions();
        }
    }

    private void OnEnable()
    {
        inputA.Enable();

        // MOVIMIENTO
        inputA.GamePlay.Movement.performed += OnMovementPerformed;
        inputA.GamePlay.Movement.canceled += OnMovementCanceled;

        // CORRER
        inputA.GamePlay.Run.started += OnRunStarted;
        inputA.GamePlay.Run.canceled += OnRunCanceled;

        // AGACHARSE
        inputA.GamePlay.Crouch.started += OnCrouchStarted;
        inputA.GamePlay.Crouch.canceled += OnCrouchCanceled;

        // ACCIONES
        inputA.GamePlay.Atacar.performed += OnAttackPerformed;
        inputA.GamePlay.Throw.performed += OnThrowPerformed;
        inputA.GamePlay.Gather.performed += OnGatherPerformed;
        inputA.GamePlay.Gather.canceled += OnGatherCanceledPerformed;
    }

    private void OnDisable()
    {
        UnsubscribeAll();
        inputA.Disable();
    }

    private void OnApplicationQuit()
    {
        // Asegura liberar memoria y evitar warnings
        UnsubscribeAll();
        inputA.Disable();
        inputA.Dispose();
    }

    #region Callbacks Privados
    static void OnMovementPerformed(InputAction.CallbackContext ctx) => OnMoveChanged?.Invoke(ctx.ReadValue<Vector2>());
    static void OnMovementCanceled(InputAction.CallbackContext ctx) => OnMoveChanged?.Invoke(Vector2.zero);

    static void OnRunStarted(InputAction.CallbackContext ctx) => OnRunChanged?.Invoke(true);
    static void OnRunCanceled(InputAction.CallbackContext ctx) => OnRunChanged?.Invoke(false);

    static void OnCrouchStarted(InputAction.CallbackContext ctx) => OnCrouchChanged?.Invoke(true);
    static void OnCrouchCanceled(InputAction.CallbackContext ctx) => OnCrouchChanged?.Invoke(false);

    static void OnAttackPerformed(InputAction.CallbackContext ctx) => OnAttack?.Invoke();
    static void OnThrowPerformed(InputAction.CallbackContext ctx) => OnThrow?.Invoke();
    static void OnGatherPerformed(InputAction.CallbackContext ctx) => OnGather?.Invoke();
    static void OnGatherCanceledPerformed(InputAction.CallbackContext ctx) => OnGatherCanceled?.Invoke();
    #endregion

    #region Helper Methods
    static void UnsubscribeAll()
    {
        if (inputA == null) return;

        // MOVIMIENTO
        inputA.GamePlay.Movement.performed -= OnMovementPerformed;
        inputA.GamePlay.Movement.canceled -= OnMovementCanceled;

        // CORRER
        inputA.GamePlay.Run.started -= OnRunStarted;
        inputA.GamePlay.Run.canceled -= OnRunCanceled;

        // AGACHARSE
        inputA.GamePlay.Crouch.started -= OnCrouchStarted;
        inputA.GamePlay.Crouch.canceled -= OnCrouchCanceled;

        // ACCIONES
        inputA.GamePlay.Atacar.performed -= OnAttackPerformed;
        inputA.GamePlay.Throw.performed -= OnThrowPerformed;
        inputA.GamePlay.Gather.performed -= OnGatherPerformed;
    }
    #endregion
}
