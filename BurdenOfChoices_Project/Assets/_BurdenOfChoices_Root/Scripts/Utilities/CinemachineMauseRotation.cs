using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controla la rotación de un pivot usado por una cámara Cinemachine,
/// usando el ratón con límites definidos.
/// </summary>
public class CinemachineMouseLimitedRotation : MonoBehaviour
{
    #region Settings

    [Header("Idle Motion")]
    [SerializeField] float idleAmplitudeX = 0.6f;
    [SerializeField] float idleAmplitudeY = 0.4f;
    [SerializeField] float idleSpeed = 0.2f;

    [Header("Mouse Influence")]
    [SerializeField] float mouseInfluence = 0.015f;
    [SerializeField] float mouseMaxOffset = 4f;

    [Header("Damping")]
    [SerializeField] float dampTime = 5f;

    #endregion

    #region Internal State

    Vector2 idleOffset;
    Vector2 mouseOffset;
    Vector2 finalOffset;
    Vector2 offsetVelocity;

    float idleTime;

    Quaternion originalRotation;

    #endregion

    #region Unity Callbacks

    void Awake()
    {
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        UpdateIdleMotion();
        UpdateMouseInfluence();
        ApplyFinalMotion();
    }

    #endregion

    #region Logic

    void UpdateIdleMotion()
    {
        idleTime += Time.unscaledDeltaTime * idleSpeed;

        idleOffset.x = Mathf.Sin(idleTime) * idleAmplitudeX;
        idleOffset.y = Mathf.Cos(idleTime * 0.8f) * idleAmplitudeY;
    }

    void UpdateMouseInfluence()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            mouseOffset += delta * mouseInfluence;
            mouseOffset = Vector2.ClampMagnitude(mouseOffset, mouseMaxOffset);
        }
        else
        {
            // Retorno suave al soltar click
            mouseOffset = Vector2.SmoothDamp(
                mouseOffset,
                Vector2.zero,
                ref offsetVelocity,
                dampTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );
        }
    }

    void ApplyFinalMotion()
    {
        Vector2 targetOffset = idleOffset + mouseOffset;

        finalOffset = Vector2.SmoothDamp(
            finalOffset,
            targetOffset,
            ref offsetVelocity,
            dampTime,
            Mathf.Infinity,
            Time.unscaledDeltaTime
        );

        // Aplicar offsets relativos a la rotación original
        Quaternion rotX = Quaternion.AngleAxis(finalOffset.y, Vector3.right);
        Quaternion rotY = Quaternion.AngleAxis(finalOffset.x, Vector3.up);

        transform.localRotation = originalRotation * rotY * rotX;
    }

    #endregion
}
