using UnityEngine;

public class Billboard : MonoBehaviour
{
    Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // Hace que el sprite siempre mire a la cámara
        transform.LookAt(mainCamera.transform.position);
    }
}
