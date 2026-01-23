using UnityEngine;

/// <summary>
/// Sistema de arrastre lineal entre dos puntos con triggers de agarre.
/// Bloquea movimiento lateral y rotación del jugador mientras arrastra.
/// Peso del objeto afecta velocidad y rotación.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class LinearDraggableObject : MonoBehaviour, IPickListener, IInteractable
{
    #region Inspector
    [Header("Drag Points")]
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;

    [Header("Grab Zones")]
    [SerializeField] Collider grabZoneA;
    [SerializeField] Collider grabZoneB;

    [Header("Drag Settings")]
    [SerializeField] float baseDragForce = 5f;
    [SerializeField] float feedbackForce = 0.1f;
    #endregion

    #region Internal
    Rigidbody rb;
    PlayerController playerController;
    Transform player;

    Vector3 dragAxis;
    float objectWeight = 1f;
    bool isDragging = false;
    bool canGrab = false;
    #endregion

    #region Unity
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        dragAxis = (pointB.position - pointA.position).normalized;
    }

    private void FixedUpdate()
    {
        if (!isDragging || playerController == null) return;

        // Proyección de velocidad del jugador sobre el eje de arrastre
        Vector3 playerPlanarVel = new Vector3(playerController.PlanarVelocity.x, 0f, playerController.PlanarVelocity.z);
        float projectedVel = Vector3.Dot(playerPlanarVel, dragAxis);
        if (Mathf.Abs(projectedVel) < 0.01f) return;

        // Movimiento lineal condicionado por peso
        Vector3 moveDelta = dragAxis * (projectedVel / objectWeight) * Time.fixedDeltaTime;
        Vector3 targetPos = rb.position + moveDelta;

        // Limitar a A-B
        float t = Mathf.Clamp01(ProjectTOnLine(targetPos));
        Vector3 limitedPos = Vector3.Lerp(pointA.position, pointB.position, t);

        // Feedback al límite
        if (t <= 0f || t >= 1f)
            limitedPos -= dragAxis * Mathf.Sign(projectedVel) * feedbackForce * Time.fixedDeltaTime;

        rb.MovePosition(limitedPos);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == grabZoneA || other == grabZoneB)
            canGrab = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == grabZoneA || other == grabZoneB)
            canGrab = false;
    }
    #endregion

    #region Drag Control
    public void TryStartDrag(Transform playerTransform, PlayerController controller, float weight)
    {
        if (!canGrab || controller == null) return;

        player = playerTransform;
        playerController = controller;
        objectWeight = Mathf.Max(0.1f, weight);

        isDragging = true;

        // Bloquear rotación y eje perpendicular
        playerController.LockRotation();
        Vector3 lockAxis = Vector3.Cross(dragAxis, Vector3.up);
        playerController.LockMovementAxis(lockAxis);

        // Aplicar peso al jugador
        playerController.SetDraggedWeight(objectWeight);

        // Snap de rotación al lado contrario de la zona de agarre
        Vector3 snapDir = (player.position - transform.position).normalized;
        snapDir.y = 0f;
        player.rotation = Quaternion.LookRotation(-snapDir); // mira hacia el objeto
    }

    public void StopDrag()
    {
        if (!isDragging) return;

        isDragging = false;

        if (playerController != null)
        {
            playerController.SetDraggedWeight(1f);
            playerController.UnlockMovementAxis();
            playerController.UnlockRotation();
        }

        player = null;
        playerController = null;
    }
    #endregion

    #region IPickListener
    public void OnPick(ICatcher catcher)
    {
        if (catcher == null) return;

        Transform playerRoot = catcher.GetCatchPoint().root;
        PlayerController controller = playerRoot.GetComponent<PlayerController>();
        AnimatorManager animator = playerRoot.GetComponent<AnimatorManager>();

        // Animación de coger
        if (animator != null) animator.SetGrabbing(true);

        // Inicia el drag
        TryStartDrag(playerRoot, controller, Weight);
    }

    public void OnDrop()
    {
        StopDrag();

        if (playerController != null)
        {
            AnimatorManager animator = playerController.GetComponent<AnimatorManager>();
            if (animator != null) animator.SetGrabbing(false);
        }
    }
    #endregion

    #region IInteractable
    public void OnPress() { }
    public void OnRelease() { }
    public void OnHighlight() { }
    public void OnRemoveHighlight() { }
    #endregion

    #region Utils
    public float Weight
    {
        get
        {
            // Suponiendo que se obtiene de un DataProvider o valor por defecto
            DataProvider provider = GetComponent<DataProvider>();
            if (provider != null)
            {
                var data = provider.GetData<EquipableData>();
                if (data != null) return Mathf.Max(1f, data.weight);
            }
            return 1f;
        }
    }

    float ProjectTOnLine(Vector3 targetPos)
    {
        Vector3 lineDir = pointB.position - pointA.position;
        float lineLength = lineDir.magnitude;
        lineDir.Normalize();

        float proj = Vector3.Dot(targetPos - pointA.position, lineDir);
        return proj / lineLength;
    }
    #endregion
}
