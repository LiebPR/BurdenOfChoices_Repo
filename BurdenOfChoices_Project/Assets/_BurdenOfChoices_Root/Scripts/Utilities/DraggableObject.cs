using UnityEngine;

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
    public Transform carrilA;
    public Transform carrilB;
    [SerializeField] float weight = 1f;
    public float Weight => weight;
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
        float t = Vector3.Dot(targetPos - carrilA.position, direction) / Vector3.Distance(carrilA.position, carrilB.position);
        t = Mathf.Clamp01(t);
        return carrilA.position + direction * Vector3.Distance(carrilA.position, carrilB.position) * t;
    }
    #endregion
}
