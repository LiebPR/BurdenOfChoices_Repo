using UnityEngine;

/// <summary>
/// Objeto arrastrable: define resistencia, puntos de agarre y carril de arrastre.
/// </summary>
public class DraggableObject : MonoBehaviour
{
    #region Grab Definition
    public enum GrabSide { PosX, NegX, PosZ, NegZ }

    [System.Serializable]
    public struct GrabDefinition
    {
        public GrabSide side;
        public Transform grabPoint;
        public Vector3 lookDirection;
    }

    [Header("Grab Points")]
    [SerializeField] GrabDefinition[] grabPoints;
    #endregion

    #region Drag State
    [HideInInspector] public Transform activeGrabPoint;
    [HideInInspector] public Vector3 grabFaceLocal;
    [HideInInspector] public bool isBeingDragged;
    [HideInInspector] public Transform currentPlayer;
    #endregion

    #region Drag Config
    [Header("Carril & Peso")]
    [SerializeField] Transform carrilA;
    [SerializeField] Transform carrilB;

    [Header("Resistencias")]
    [SerializeField] float weight = 1f;
    [SerializeField] float initialResistance = 0.3f;
    [SerializeField] float constantResistance = 0.1f;
    [SerializeField] float timeInitialResistance = 0.2f;

    public float Weight => weight;
    public float InitialResistance => initialResistance;
    public float ConstantResistance => constantResistance;
    public float TimeInitialResistance => timeInitialResistance;

    public Transform CarrilA => carrilA;
    public Transform CarrilB => carrilB;
    #endregion

    #region Grab Resolution
    public bool ResolveGrabPoint(Transform player)
    {
        if (player == null) return false;

        currentPlayer = player;
        Vector3 localPlayerPos = transform.InverseTransformPoint(player.position);

        float absX = Mathf.Abs(localPlayerPos.x);
        float absZ = Mathf.Abs(localPlayerPos.z);

        GrabSide side = absX > absZ
            ? (localPlayerPos.x > 0f ? GrabSide.PosX : GrabSide.NegX)
            : (localPlayerPos.z > 0f ? GrabSide.PosZ : GrabSide.NegZ);

        foreach (var gp in grabPoints)
        {
            if (gp.side == side)
            {
                activeGrabPoint = gp.grabPoint;
                grabFaceLocal = gp.lookDirection;
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Drag Control
    public void StartDragging()
    {
        if (activeGrabPoint == null) return;
        isBeingDragged = true;
    }

    public void StopDragging()
    {
        isBeingDragged = false;
        activeGrabPoint = null;
        grabFaceLocal = Vector3.zero;
        currentPlayer = null;
    }
    #endregion

    #region Carril Projection
    public Vector3 ProjectedPosition(Vector3 targetPos)
    {
        if (carrilA == null || carrilB == null) return targetPos;

        Vector3 direction = (carrilB.position - carrilA.position).normalized;
        Vector3 projected = Vector3.Project(targetPos - carrilA.position, direction);
        float t = Mathf.Clamp01(projected.magnitude / Vector3.Distance(carrilA.position, carrilB.position));
        return carrilA.position + direction * Vector3.Distance(carrilA.position, carrilB.position) * t;
    }
    #endregion
}
