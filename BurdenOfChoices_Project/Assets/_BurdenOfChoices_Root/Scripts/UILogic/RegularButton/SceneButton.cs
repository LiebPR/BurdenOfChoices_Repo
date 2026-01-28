using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena a cargar")]
    [SerializeField] string sceneName;
    [SerializeField] float delay = 0.1f;
    [SerializeField] float fadeDelay = 0.5f;
    [SerializeField] float fadeOutDuration = 0.5f;
    [SerializeField] float fadeInDuration = 0.5f;

    public void LoadScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneButton: No se ha asignado nombre de escena.");
            return;
        }

        // Llama al SceneController existente, usando su delay por defecto
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(sceneName, delay, fadeDelay, fadeOutDuration, fadeInDuration);
        }
        else
        {
            Debug.LogError("SceneButton: SceneController.Instance no encontrado en la escena.");
        }
    }

    public void QuitGame()
    {
        SceneController.Instance.QuitGame();
    }
}
