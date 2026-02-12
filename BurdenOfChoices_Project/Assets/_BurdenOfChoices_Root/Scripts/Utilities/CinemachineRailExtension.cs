using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class CinemachineRailExtension : CinemachineExtension
{
    #region Rail

    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] Transform player;

    [Header("Rail Settings")]
    [SerializeField] RailAxis followAxis = RailAxis.Z;

    #endregion

    #region Follow

    [SerializeField] float targetOffsetDistance = 2f;
    [SerializeField] float followDamping = 6f;

    #endregion

    #region Rotation

    [SerializeField] bool enableRotation = true;

    [SerializeField] float minPitch = 64f;
    [SerializeField] float maxPitch = 74f;

    [Tooltip("Porcentaje del carril sin rotación")]
    [SerializeField] float rotationStartT = 0.15f;

    #endregion

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (!Application.isPlaying) return;
        if (stage != CinemachineCore.Stage.Body) return;
        if (!player || !pointA || !pointB) return;

        Vector3 railVector = pointB.position - pointA.position;
        Vector3 railDir = railVector.normalized;
        float railLength = railVector.magnitude;

        Vector3 playerVector = player.position - pointA.position;

        float projectedDistance = followAxis == RailAxis.Z
            ? Vector3.Dot(playerVector, railDir)
            : Vector3.Dot(playerVector, railDir);

        float t = Mathf.Clamp01(projectedDistance / railLength);

        Vector3 camPos = Vector3.Lerp(pointA.position, pointB.position, t);

        float camOffset = targetOffsetDistance;

        float minDistance = Vector3.Distance(pointA.position, camPos);
        float maxDistance = railLength - Vector3.Distance(camPos, pointB.position);
        camOffset = Mathf.Clamp(camOffset, -minDistance, maxDistance);

        camPos += railDir * camOffset;

        float dt = deltaTime > 0f ? deltaTime : Time.deltaTime;
        state.RawPosition = Vector3.Lerp(
            state.RawPosition,
            camPos,
            1f - Mathf.Exp(-followDamping * dt)
        );

        if (!enableRotation) return;

        float pitchT = Mathf.InverseLerp(rotationStartT, 1f, t);
        pitchT = Mathf.Clamp01(pitchT);
        pitchT = pitchT * pitchT * (3f - 2f * pitchT);

        float pitch = Mathf.Lerp(minPitch, maxPitch, pitchT);

        state.RawOrientation = Quaternion.Euler(
            pitch,
            state.RawOrientation.eulerAngles.y,
            state.RawOrientation.eulerAngles.z
        );
    }
}

public enum RailAxis
{
    X,
    Z
}