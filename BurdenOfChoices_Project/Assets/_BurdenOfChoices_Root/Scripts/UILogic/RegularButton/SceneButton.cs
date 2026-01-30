using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena a cargar")]
    [SerializeField] string sceneName;

    [Tooltip("Delay antes de iniciar la carga de escena")]
    [SerializeField] float delaySeconds = 0f;

    public void LoadSceneDelay()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneButton: No se ha asignado nombre de escena.");
            return;
        }

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(sceneName, delaySeconds);
        }
        else
        {
            Debug.LogError("SceneButton: SceneController.Instance no encontrado en la escena.");
        }
    }

    public void QuitGame()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.QuitGame();
    }
}
