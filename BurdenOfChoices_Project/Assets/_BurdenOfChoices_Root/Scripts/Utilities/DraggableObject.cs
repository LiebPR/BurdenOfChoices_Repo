using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DraggableObject : MonoBehaviour
{
    [Header("Grab Points")]
    public Collider[] grabPoints;

    [Header("Drag Settings")]
    public Vector3 dragAxis = Vector3.right; // X o Z
    public Transform pointA;
    public Transform pointB;
    public float maxDragSpeed = 2.5f;
    public float dragDamping = 8f;

    [Header("Weight")]
    [SerializeField] private float weight = 1f;
    public float Weight => weight;

    [HideInInspector] public bool isBeingDragged;
    [HideInInspector] public Transform currentPlayer;

    // 👉 NUEVO: cara del objeto desde donde se agarra (en espacio LOCAL)
    [HideInInspector] public Vector3 grabFaceLocal;

    private Rigidbody rb;

    [Header("Physics Settings")]
    public float dragForceMultiplier = 50f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.linearDamping = 5f;
    }

    public bool CanBeGrabbedBy(Transform player)
    {
        foreach (var col in grabPoints)
            if (col.bounds.Contains(player.position))
                return true;
        return false;
    }

    public void StartDragging(Transform player)
    {
        isBeingDragged = true;
        currentPlayer = player;

        // 🔹 Posición del jugador en espacio LOCAL del objeto
        Vector3 localPlayerPos = transform.InverseTransformPoint(player.position);

        // 🔹 Determinar cara exacta según eje de drag
        if (Mathf.Abs(dragAxis.x) > Mathf.Abs(dragAxis.z))
        {
            grabFaceLocal = localPlayerPos.x >= 0f ? Vector3.right : Vector3.left;
        }
        else
        {
            grabFaceLocal = localPlayerPos.z >= 0f ? Vector3.forward : Vector3.back;
        }
    }

    public void StopDragging()
    {
        isBeingDragged = false;
        currentPlayer = null;
        grabFaceLocal = Vector3.zero;
    }

    public void ApplyForce(Vector3 playerInput, float playerWeightFactor)
    {
        if (!isBeingDragged) return;

        Vector3 axis = dragAxis.normalized;

        // 🔹 Input proyectado SOLO en el eje permitido
        Vector3 inputProjected = Vector3.Project(playerInput, axis);

        // 🔹 Aceleración controlada (no acumulativa salvaje)
        float accel = dragForceMultiplier * playerWeightFactor / Mathf.Max(Weight, 0.1f);
        rb.AddForce(inputProjected * accel, ForceMode.Acceleration);

        // 🔹 Limitar velocidad SOLO en eje de drag
        Vector3 velocity = rb.linearVelocity;
        float axisSpeed = Vector3.Dot(velocity, axis);
        axisSpeed = Mathf.Clamp(axisSpeed, -maxDragSpeed, maxDragSpeed);

        Vector3 clampedVelocity = axis * axisSpeed;
        rb.linearVelocity = new Vector3(
            clampedVelocity.x,
            rb.linearVelocity.y,
            clampedVelocity.z
        );

        // 🔹 Frenado cuando no hay input
        if (inputProjected.sqrMagnitude < 0.001f)
        {
            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                new Vector3(0f, rb.linearVelocity.y, 0f),
                dragDamping * Time.fixedDeltaTime
            );
        }

        // 🔹 Clamp SUAVE por límites (sin teletransporte)
        if (pointA != null && pointB != null)
        {
            float min, max, pos;

            if (Mathf.Abs(axis.x) > 0.1f)
            {
                min = Mathf.Min(pointA.position.x, pointB.position.x);
                max = Mathf.Max(pointA.position.x, pointB.position.x);
                pos = Mathf.Clamp(rb.position.x, min, max);
                rb.MovePosition(new Vector3(pos, rb.position.y, rb.position.z));
            }
            else
            {
                min = Mathf.Min(pointA.position.z, pointB.position.z);
                max = Mathf.Max(pointA.position.z, pointB.position.z);
                pos = Mathf.Clamp(rb.position.z, min, max);
                rb.MovePosition(new Vector3(rb.position.x, rb.position.y, pos));
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!isBeingDragged) return;

        Gizmos.color = Color.green;
        Vector3 worldDir = transform.TransformDirection(grabFaceLocal);
        Gizmos.DrawLine(transform.position, transform.position + worldDir);
    }
#endif
}
