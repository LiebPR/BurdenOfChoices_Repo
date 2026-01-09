using UnityEngine;

/// <summary>
/// GameStopManager: Maneja la pausa global del juego sin afectar UI ni objetos inspeccionables.
/// </summary>
public class GameStopManager : MonoBehaviour
{
    public static GameStopManager Instance { get; private set; }

    [HideInInspector] public bool isGamePaused = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Congela el juego (sin usar Time.timeScale)
    /// </summary>
    public void PauseGame()
    {
        isGamePaused = true;
    }

    /// <summary>
    /// Reanuda el juego
    /// </summary>
    public void ResumeGame()
    {
        isGamePaused = false;
    }
}
