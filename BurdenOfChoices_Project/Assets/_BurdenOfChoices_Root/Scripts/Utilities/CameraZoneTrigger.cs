using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CameraZoneTrigger : MonoBehaviour
{
    #region Inspector
    [SerializeField] CinemachineCamera targetCamera;
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (targetCamera == null) return;

        CameraManager.Instance.ActivateCamera(targetCamera);
    }
}
