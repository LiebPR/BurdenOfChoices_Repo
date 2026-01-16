using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    [SerializeField] private CinemachineCamera entryCamera;      // cámara de la zona
    [SerializeField] private CinemachineCamera outSideCamera;  // cámara base de la sala

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (entryCamera != null)
            CameraManager.Instance.ActivateCamera(entryCamera);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Volver a la cámara base de la sala
        if (outSideCamera != null)
            CameraManager.Instance.ActivateCamera(outSideCamera);
    }
}
