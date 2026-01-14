using UnityEngine;

public class DraggableBehaviour : MonoBehaviour
{
    #region Inspector
    [Header("Push Points")]
    [SerializeField] Transform pushPosX;
    [SerializeField] Transform pushNegX;
    [SerializeField] Transform pushPosZ;
    [SerializeField] Transform pushNegZ;

    [Header("Drag Config")]
    [SerializeField] float blockCheckDistance = 0.15f;
    [SerializeField] LayerMask obstacleMask;
    #endregion

    #region Internal
    PickableBehaviour pickable;
    Transform player;
    PlayerController playerController;
    Rigidbody rb;
    DraggableObject draggableObject;

    DragFace activeFace = DragFace.None;

    Vector3 dragAxis;         // Eje en el que el objeto puede moverse
    Vector3 allowedDirection; // Dirección que determina si se aleja o acerca
    float weightMultiplier = 1f;
    #endregion

    #region Unity
    void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        if (pickable != null)
            rb = pickable.rb;
        draggableObject = GetComponent<DraggableObject>();
    }

    void FixedUpdate()
    {
        if (playerController == null || activeFace == DragFace.None) return;

        Vector3 playerPlanarVel = playerController.PlanarVelocity;
        float projectedVel = Vector3.Dot(playerPlanarVel, dragAxis);

        if (Mathf.Abs(projectedVel) < 0.01f) return;
        if (IsBlocked(dragAxis * Mathf.Sign(projectedVel))) return;

        // Movimiento usando MovePosition
        Vector3 targetPos = rb.position + dragAxis * projectedVel * weightMultiplier * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }
    #endregion

    #region Drag Control
    public void StartDrag(Transform playerTransform)
    {
        player = playerTransform;
        playerController = player.GetComponent<PlayerController>();

        if (playerController != null)
            playerController.LockRotation();

        ResolveActiveFace(player);
    }

    public void StopDrag()
    {
        if (playerController != null)
        {
            playerController.UnlockMovementAxis();
            playerController.UnlockRotation();
        }

        player = null;
        playerController = null;
        activeFace = DragFace.None;
    }
    #endregion

    #region Face Resolution
    public void ResolveActiveFace(Transform playerTransform)
    {
        activeFace = DragFace.None;

        float dPosX = Vector3.Distance(player.position, pushPosX.position);
        float dNegX = Vector3.Distance(player.position, pushNegX.position);
        float dPosZ = Vector3.Distance(player.position, pushPosZ.position);
        float dNegZ = Vector3.Distance(player.position, pushNegZ.position);

        float min = Mathf.Min(dPosX, dNegX, dPosZ, dNegZ);
        if (min > 0.6f) return;

        Vector3 lookDir = Vector3.zero;

        if (min == dPosX) { activeFace = DragFace.PosX; dragAxis = Vector3.right; lookDir = Vector3.left; }
        else if (min == dNegX) { activeFace = DragFace.NegX; dragAxis = Vector3.left; lookDir = Vector3.right; }
        else if (min == dPosZ) { activeFace = DragFace.PosZ; dragAxis = Vector3.forward; lookDir = Vector3.back; }
        else if (min == dNegZ) { activeFace = DragFace.NegZ; dragAxis = Vector3.back; lookDir = Vector3.forward; }

        // Aplicar bloqueo de movimiento en PlayerController
        if (playerController != null)
        {
            // Bloquear eje perpendicular al drag
            Vector3 lockAxis = (dragAxis.x != 0f) ? Vector3.forward : Vector3.right;
            playerController.LockMovementAxis(lockAxis);

            // Forzar mirada del jugador
            if (lookDir != Vector3.zero)
                player.rotation = Quaternion.LookRotation(lookDir);

            // Resetear velocidad planar para evitar impulso inicial
            playerController.rb.linearVelocity = new Vector3(0f, playerController.rb.linearVelocity.y, 0f);

            // Aplicar peso del objeto arrastrable
            float objectWeight = draggableObject != null ? draggableObject.Weight : 1f;
            playerController.SetDraggedWeight(objectWeight);
        }
    }
    #endregion

    #region Block Check
    bool IsBlocked(Vector3 dir)
    {
        Collider c = pickable.GetComponent<Collider>();
        Bounds b = c.bounds;

        return Physics.BoxCast(
            b.center,
            b.extents * 0.95f,
            dir.normalized,
            Quaternion.identity,
            blockCheckDistance,
            obstacleMask
        );
    }
    #endregion

    enum DragFace { None, PosX, NegX, PosZ, NegZ }
}
