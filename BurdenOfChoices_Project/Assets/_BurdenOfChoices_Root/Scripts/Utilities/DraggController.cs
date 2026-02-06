using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class DraggController : MonoBehaviour
{
    #region References
    PlayerController player;
    AnimatorManager animator;
    DraggableObject currentDrag;
    #endregion

    #region Inspector
    [Header("Drag Settings")]
    [SerializeField] bool debug = false;
    public Transform dragAnchor;
    [SerializeField] float followDamp = 0.05f;
    #endregion

    #region Internal
    Vector3 dragVelocitySmooth;
    Vector3 initialLocalOffset;
    Coroutine resistanceCoroutine;
    #endregion

    #region Getters
    public bool IsDragging => currentDrag != null;
    public DraggableObject CurrentDrag => currentDrag;
    #endregion

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        animator = GetComponent<AnimatorManager>();

        if (dragAnchor == null)
            dragAnchor = transform;
    }

    private void FixedUpdate()
    {
        if (IsDragging)
            UpdateDrag();
    }

    #region Public API
    public void StartDrag(DraggableObject draggable)
    {
        if (draggable == null || !draggable.ResolveGrabPoint(player.transform)) return;

        currentDrag = draggable;
        currentDrag.StartDragging();

        animator?.SetGrabbing(true);
        player.LockRotation();
        player.LockCrouch();

        currentDrag.transform.SetParent(dragAnchor);
        initialLocalOffset = dragAnchor.InverseTransformPoint(currentDrag.transform.position);

        // Detener al jugador al instante
        player.rb.linearVelocity = Vector3.zero;
        player.ResetMovementModifier();

        ApplyInputClamp();

        // Aplicamos peso y resistencia del objeto
        player.SetWeight(currentDrag.Weight);

        if (resistanceCoroutine != null) StopCoroutine(resistanceCoroutine);
        resistanceCoroutine = StartCoroutine(TemporaryResistance(player, currentDrag.InitialResistance, currentDrag.TimeInitialResistance));
    }

    public void StopDrag()
    {
        if (currentDrag == null) return;

        currentDrag.StopDragging();

        animator?.SetGrabbing(false);
        player.UnlockRotation();
        player.UnlockCrouch();

        // Resetear peso y resistencia
        player.SetWeight(1f);
        player.ApplyDragResistance(0f);

        currentDrag.transform.SetParent(null);
        RemoveInputClamp();

        // Limpiar bloqueos de movimiento al soltar
        player.ClearBlockedDirections();
        player.rb.linearVelocity = Vector3.zero; // detener cualquier velocidad residual

        if (resistanceCoroutine != null)
        {
            StopCoroutine(resistanceCoroutine);
            resistanceCoroutine = null;
        }

        if (debug)
            Debug.Log("[Drag] Stopped dragging object: " + currentDrag.name);

        currentDrag = null;
        dragVelocitySmooth = Vector3.zero;
    }
    #endregion

    #region Drag Logic
    void UpdateDrag()
    {
        if (currentDrag == null) return;

        if (currentDrag.CarrilA != null && currentDrag.CarrilB != null)
        {
            Vector3 dir = (currentDrag.CarrilB.position - currentDrag.CarrilA.position).normalized;
            float totalLength = Vector3.Distance(currentDrag.CarrilA.position, currentDrag.CarrilB.position);
            float distanceAlong = Vector3.Dot(currentDrag.transform.position - currentDrag.CarrilA.position, dir);

            // Clamp distance dentro del carril
            distanceAlong = Mathf.Clamp(distanceAlong, 0f, totalLength);
            Vector3 clampedPos = currentDrag.CarrilA.position + dir * distanceAlong;
            clampedPos.y = currentDrag.transform.position.y;

            // SmoothDamp hacia la posición clamped
            currentDrag.transform.position = Vector3.SmoothDamp(
                currentDrag.transform.position,
                clampedPos,
                ref dragVelocitySmooth,
                followDamp
            );

            // --- Bloqueos dinámicos y detención del jugador ---
            if (distanceAlong <= 0f)
            {
                // Objeto en A
                player.BlockMovementInDirection(-dir);  // bloquear hacia A
                player.ClearMovementBlock(dir);         // permitir hacia B
                player.rb.linearVelocity = Vector3.ProjectOnPlane(player.rb.linearVelocity, -dir); // detener movimiento hacia A
            }
            else if (distanceAlong >= totalLength)
            {
                // Objeto en B
                player.BlockMovementInDirection(dir);   // bloquear hacia B
                player.ClearMovementBlock(-dir);        // permitir hacia A
                player.rb.linearVelocity = Vector3.ProjectOnPlane(player.rb.linearVelocity, dir);  // detener movimiento hacia B
            }
            else
            {
                // Objeto en medio
                player.ClearBlockedDirections();
            }
        }
    }
    #endregion

    #region Input Restrictions
    void ApplyInputClamp()
    {
        if (currentDrag == null || player == null || currentDrag.CarrilA == null || currentDrag.CarrilB == null) return;

        Vector3 dir = (currentDrag.CarrilB.position - currentDrag.CarrilA.position).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            player.LockMovementAxis(Vector3.forward);
        else
            player.LockMovementAxis(Vector3.right);
    }

    void RemoveInputClamp()
    {
        player?.UnlockMovementAxis();
    }
    #endregion

    #region Resistance Coroutine
    IEnumerator TemporaryResistance(PlayerController player, float extraResistance, float duration)
    {
        float timer = duration;
        while (timer > 0f)
        {
            float factor = 1f - extraResistance * (timer / duration);
            player.ApplyDragResistance(factor);
            timer -= Time.deltaTime;
            yield return null;
        }
        player.ApplyDragResistance(0f);
    }
    #endregion
}
