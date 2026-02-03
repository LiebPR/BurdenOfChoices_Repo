using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [SerializeField] CinemachineCamera[] cameras;
    [SerializeField] int defaultPriority = 10;
    [SerializeField] int inactivePriority = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    // ESTE MÉTODO DEBE ESTAR EXACTO
    public void ActivateCamera(CinemachineCamera cam, int priority = -1)
    {
        if (priority < 0) priority = defaultPriority;

        foreach (var c in cameras)
            c.Priority = inactivePriority;

        if (cam != null)
            cam.Priority = priority;
    }
}
