using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
public class CinemachineRailExtension : CinemachineExtension
{
    [Header("Rail")]
    [SerializeField] Transform pointA;
    [SerializeField] Transform pointB;
    [SerializeField] Transform player;

    [Header("Target Centering")]
    [Tooltip("Distancia sobre el carril para mantener al jugador centrado")]
    [SerializeField] float targetOffsetDistance = 2f;

    [Header("Follow Damping")]
    [Tooltip("Cuanto mayor, más rápida responde la cámara")]
    [SerializeField] float followDamping = 6f;

    [Header("Pitch")]
    [SerializeField] float minPitch = 64f;
    [SerializeField] float maxPitch = 74f;

    [Header("Pitch Control")]
    [Tooltip("Porcentaje del carril sin rotación")]
    [SerializeField] float rotationStartT = 0.15f;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body || !player || !pointA || !pointB) return;

        //CALCULAR EL RAIL
        Vector3 railVector = pointB.position - pointA.position;
        Vector3 railDir = railVector.normalized;
        float railLength = railVector.magnitude;

        Vector3 playerVector = player.position - pointA.position;

        //Progreso del jugador sobre el carril
        float t = Mathf.Clamp01(Vector3.Dot(playerVector, railDir) / railLength);

        //POSICIÓN DE LA CAMRA SOBRE EL RAIL
        Vector3 camPos = Vector3.Lerp(pointA.position, pointB.position, t);

        //CENTRAR AL JUGADOR EN LA CAMARA
        float camOffset = targetOffsetDistance;

        //Evitar que la cámara salga del carril
        float minDistance = Vector3.Distance(pointA.position, camPos);
        float maxDistance = railLength - Vector3.Distance(camPos, pointB.position);
        camOffset = Mathf.Clamp(camOffset, -minDistance, maxDistance);

        camPos += railDir * camOffset;

        float dt = deltaTime > 0f ? deltaTime : Time.deltaTime;

        Vector3 dampedPos = Vector3.Lerp(state.RawPosition, camPos, 1f - Mathf.Exp(-followDamping * dt));

        state.RawPosition = dampedPos;

        //CONTROL DE LA DEATH ZONE DE ROTACIÓN
        float pitchT = Mathf.InverseLerp(rotationStartT, 1f, t);
        pitchT = Mathf.Clamp01(pitchT);

        //SmothStep easing
        pitchT = pitchT * pitchT * (3f - 2f * pitchT);

        float pitch = Mathf.Lerp(minPitch, maxPitch, pitchT);

        state.RawOrientation = Quaternion.Euler(pitch, state.RawOrientation.eulerAngles.y, state.RawOrientation.eulerAngles.z);
    }
}
