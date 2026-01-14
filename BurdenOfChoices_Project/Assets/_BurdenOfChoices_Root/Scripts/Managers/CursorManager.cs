using UnityEngine;

/// <summary>
/// CursorManager: controla solo la visibilidad del cursor mientras el jugador exista.
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    [SerializeField] bool hideCursorDuringGameplay = true; // por defecto, cursor invisible

    // Control de visibilidad forzada
    bool isCursorForcedVisible = false;

    private void Awake()
    {
        UpdateCursorVisibility();
    }

    private void Update()
    {
        // Opcional: asegurar que el cursor se mantiene invisible en cada frame
        UpdateCursorVisibility();
    }

    void UpdateCursorVisibility()
    {
        if (isCursorForcedVisible)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = !hideCursorDuringGameplay ? true : false;
        }
    }

    /// <summary>
    /// Fuerza que el cursor se vea temporalmente (ejemplo: menú).
    /// </summary>
    public void ForceCursorVisible(bool value)
    {
        isCursorForcedVisible = value;
        UpdateCursorVisibility();
    }
}
