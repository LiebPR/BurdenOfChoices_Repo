using UnityEngine;

public class Billboard : MonoBehaviour
{
    #region Inspector States
    [Tooltip("Tamaño relativo en pantalla. Valores más altos hacen el objeto más visible.")]
    [SerializeField] float screenSize = 0.05f;

    [Tooltip("Si está activo, el billboard no rota en el eje Y (útil para textos o iconos)")]
    [SerializeField] bool lockYAxis = true;
    #endregion

    Camera mainCamera;

    private void Awake()
    {
        //Referencia a la cámara principal
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        //Orientar el objeto hacia la cámara
        Vector3 lookPos = mainCamera.transform.position;

        //BLoqueamos la rotación vertical si se desea
        if (lockYAxis)
            lookPos.y = transform.position.y;

        transform.LookAt(lookPos);

        //Corregir orientación para sprites
        transform.Rotate(0f, 180f, 0f);

        //Ajustamos la escala para mantener un tamaño constante en pantalla
        float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
        float scale = distance * screenSize;

        transform.localScale = Vector3.one * scale;
    }
}
