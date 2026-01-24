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
    [SerializeField] float initialResistance = 0.3f; // resistencia inicial
    [SerializeField] float resistanceDuration = 0.2f;
    #endregion

    #region Internal
    Vector3 dragVelocitySmooth;
    Vector3 initialLocalOffset;
    #endregion

    #region Getters
    public bool IsDragging => currentDrag != null;
    public DraggableObject CurrentDrag => currentDrag;
    public Transform CarrilA => currentDrag != null ? currentDrag.carrilA : null;
    public Transform CarrilB => currentDrag != null ? currentDrag.carrilB : null;
    #endregion

    private void Awake()
    {
        player = GetComponent<PlayerController>();
        animator = GetComponent<AnimatorManager>();

        if (dragAnchor == null)
        {
            Debug.LogWarning("DragAnchor no asignado. Usando transform del jugador como fallback.");
            dragAnchor = transform;
        }
    }

    private void FixedUpdate()
    {
        if (IsDragging)
            UpdateDrag();
    }

    #region Public API
    public void StartDrag(DraggableObject draggable)
    {
        if (draggable == null || draggable.activeGrabPoint == null) return;

        currentDrag = draggable;
        currentDrag.StartDragging();

        animator?.SetGrabbing(true);
        player.LockRotation();
        player.LockCrouch();

        currentDrag.transform.SetParent(dragAnchor);
        initialLocalOffset = dragAnchor.InverseTransformPoint(currentDrag.transform.position);

        ApplyInputClamp();

        // --- Aplicamos peso del objeto arrastrable ---
        float dragWeightFactor = Mathf.Clamp01(1f - currentDrag.Weight * 0.15f); // 0 = pesado, 1 = sin peso
        player.SetMovementModifier(dragWeightFactor, 1f);

        // Resistencia inicial temporal
        StartCoroutine(TemporaryResistance(player, dragWeightFactor * initialResistance, resistanceDuration));

        if (debug)
            Debug.Log("[Drag] Started dragging object: " + draggable.name + " Weight: " + draggable.Weight);
    }

    public void StopDrag()
    {
        if (currentDrag == null) return;

        currentDrag.StopDragging();

        animator?.SetGrabbing(false);
        player.UnlockRotation();
        player.UnlockCrouch();
        player.ResetMovementModifier();

        currentDrag.transform.SetParent(null);
        RemoveInputClamp();

        if (debug)
            Debug.Log("[Drag] Stopped dragging object: " + currentDrag.name);

        currentDrag = null;
        dragVelocitySmooth = Vector3.zero;
    }
    #endregion

    #region Drag Logic
    void UpdateDrag()
    {
        if (currentDrag == null || dragAnchor == null) return;

        Vector3 targetLocalPos = initialLocalOffset;
        currentDrag.transform.localPosition = Vector3.SmoothDamp(
            currentDrag.transform.localPosition,
            targetLocalPos,
            ref dragVelocitySmooth,
            followDamp
        );

        if (currentDrag.carrilA != null && currentDrag.carrilB != null)
        {
            Vector3 projectedPos = currentDrag.ProjectedPosition(player.transform.position);
            player.rb.position = new Vector3(projectedPos.x, player.rb.position.y, projectedPos.z);
        }
    }
    #endregion

    #region Input Restrictions
    void ApplyInputClamp()
    {
        if (currentDrag == null || player == null || CarrilA == null || CarrilB == null) return;

        Vector3 dir = (CarrilB.position - CarrilA.position).normalized;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.z))
            player.LockMovementAxis(Vector3.forward);
        else
            player.LockMovementAxis(Vector3.right);
    }

    void RemoveInputClamp()
    {
        if (player == null) return;
        player.UnlockMovementAxis();
    }
    #endregion

    #region Resistance Coroutine
    IEnumerator TemporaryResistance(PlayerController player, float extraPenalty, float duration)
    {
        float timer = duration;
        while (timer > 0f)
        {
            float factor = 1f - extraPenalty * (timer / duration);
            player.SetMovementModifier(factor, 1f);
            timer -= Time.deltaTime;
            yield return null;
        }
        player.ResetMovementModifier();
    }
    #endregion
}
