using UnityEngine;

/// <summary>
/// CursorConfinementManager:
/// Singleton que mantiene el cursor siempre confinado dentro de la ventana de juego.
/// No se destruye entre escenas y garantiza que el cursor no salga de la pantalla.
/// </summary>
public class CursorConfinementManager : MonoBehaviour
{
    private static CursorConfinementManager _instance;
    public static CursorConfinementManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyConfinement();
    }

    private void Update()
    {
        // Mantener siempre el cursor confinado
        ApplyConfinement();
    }

    private void ApplyConfinement()
    {
        Cursor.lockState = CursorLockMode.Confined;
        // Opcional: si quieres que el cursor siempre sea visible en todas las escenas
        // Cursor.visible = true;
    }
}
