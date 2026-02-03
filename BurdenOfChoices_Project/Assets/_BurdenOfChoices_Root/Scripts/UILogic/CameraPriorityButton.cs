using Unity.Cinemachine;
using UnityEngine;

public class CameraPriorityButton : MonoBehaviour
{
    #region References
    [SerializeField] CinemachineCamera targetCamera;
    [SerializeField] int priorityOverride = -1; 
    #endregion 

    //UI CALLBACK
    public void OnButtonCameraPressed()
    {
        if(CameraManager.Instance == null)
            return;
        CameraManager.Instance.ActivateCamera(targetCamera, priorityOverride);
    }
}
