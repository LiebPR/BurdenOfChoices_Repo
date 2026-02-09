using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// InspectableObject:
/// Permite inspeccionar un objeto acercándolo a la cámara,
/// rotarlo manteniendo click izquierdo y salir con ESC.
/// </summary>
public class InspectableObject : MonoBehaviour, IInteractable
{
    #region Inspector
    [Header("Inspection")]
    [SerializeField] Transform seeObject;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float rotationSpeed = 150f;
    [SerializeField] float returnSpeed = 5f;
    #endregion

    #region Internal State
    bool isMovingToInspect;
    bool isInspecting;
    bool rotationReady;
    bool isReturning;
    bool isReady = true;

    Vector3 originalPosition;
    Quaternion originalRotation;

    Vector3 returnStartPosition;
    Quaternion returnStartRotation;
    #endregion

    #region References
    Collider objectCollider;
    Camera mainCamera;
    #endregion

    private void Awake()
    {
        mainCamera = Camera.main;

        originalPosition = transform.position;
        originalRotation = transform.rotation;

        objectCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (isMovingToInspect)
        {
            MoveToInspectionPoint();
        }
        else if (isInspecting)
        {
            HandleRotation();
        }
        else if (isReturning)
        {
            ReturnToOriginalTransform();
        }
    }

    #region IInteractable
    public void OnPress()
    {
        if (!isReady || isMovingToInspect || isInspecting || isReturning) return;

        isMovingToInspect = true;
        rotationReady = false;
        isReady = false; // Bloqueamos nuevas inspecciones

        if (gameObject != null)
            objectCollider.isTrigger = true;

        GameStopManager.Instance.PauseGame();
        InspectionUIManager.Instance.RegisterInspectable(this);
    }

    // No se usa
    public void OnRelease() { }

    public void OnHighlight() { }
    public void OnRemoveHighlight() { }
    #endregion

    #region Inspection Logic
    void MoveToInspectionPoint()
    {
        if (seeObject == null) return;

        InspectionUIManager.Instance.Show();

        transform.position = Vector3.Lerp(transform.position, seeObject.position, moveSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, seeObject.rotation, moveSpeed * Time.unscaledDeltaTime);

        bool posDone = Vector3.Distance(transform.position, seeObject.position) < 0.02f;
        bool rotDone = Quaternion.Angle(transform.rotation, seeObject.rotation) < 0.5f;

        if (posDone && rotDone)
        {
            isMovingToInspect = false;
            isInspecting = true;
            rotationReady = true;
        }
    }

    void HandleRotation()
    {
        if (!rotationReady) return;
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.isPressed) return;

        Vector2 delta = Mouse.current.delta.ReadValue();

        float rotX = delta.x * rotationSpeed * Time.deltaTime;
        float rotY = delta.y * rotationSpeed * Time.deltaTime;

        transform.Rotate(mainCamera.transform.up, -rotX, Space.World);
        transform.Rotate(mainCamera.transform.right, rotY, Space.World);
    }

    public void ExitInspection()
    {
        // Si todavía se está moviendo hacia la inspección, interrumpir
        if (isMovingToInspect)
        {
            isMovingToInspect = false;
        }

        // Guardar posición y rotación actual como punto de inicio del retorno
        returnStartPosition = transform.position;
        returnStartRotation = transform.rotation;

        isInspecting = false;
        rotationReady = false;
        isReturning = true;

        GameStopManager.Instance.ResumeGame();
        InspectionUIManager.Instance.Hide();
    }

    void ReturnToOriginalTransform()
    {
        // Lerp desde la posición actual hacia la original
        transform.position = Vector3.Lerp(transform.position, originalPosition, returnSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, originalRotation, returnSpeed * Time.deltaTime);

        bool posDone = Vector3.Distance(transform.position, originalPosition) < 0.02f;
        bool rotDone = Quaternion.Angle(transform.rotation, originalRotation) < 0.5f;

        if (posDone && rotDone)
        {
            if (objectCollider != null)
                objectCollider.isTrigger = false;

            isReturning = false;
            isMovingToInspect = false;
            isInspecting = false;
            rotationReady = false;
            isReady = true; // listo para inspección otra vez
        }
    }
    #endregion
}
