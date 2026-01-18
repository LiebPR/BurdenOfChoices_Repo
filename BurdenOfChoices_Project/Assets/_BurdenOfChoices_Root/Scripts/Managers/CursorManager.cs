using UnityEngine;

/// <summary>
/// CursorManager:
/// Controla la visibilidad del cursor únicamente mientras esta escena esté activa.
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("Gameplay Settings")]
    [SerializeField] bool hideCursorDuringGameplay = true;

    bool isCursorForcedVisible = false;

    void OnEnable()
    {
        ApplyVisibility();
    }

    void OnDisable()
    {
        RestoreCursor();
    }

    void ApplyVisibility()
    {
        if (isCursorForcedVisible)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = !hideCursorDuringGameplay;
        }
    }

    void RestoreCursor()
    {
        Cursor.visible = true;
    }

    /// <summary>
    /// Fuerza la visibilidad del cursor (ej: menús).
    /// </summary>
    public void ForceCursorVisible(bool value)
    {
        isCursorForcedVisible = value;
        ApplyVisibility();
    }
}
