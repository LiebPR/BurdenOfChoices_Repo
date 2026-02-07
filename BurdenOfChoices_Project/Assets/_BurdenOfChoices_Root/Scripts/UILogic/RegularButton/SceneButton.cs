using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena a cargar")]
    [SerializeField] string sceneName;

    [Tooltip("Delay antes de iniciar la carga de escena")]
    [SerializeField] float delaySeconds = 0f;

    public void SetScene(string newScene)
    {
        sceneName = newScene;
    }

    public void LoadSceneDelay()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("SceneButton: Scene no asignada.");
            return;
        }

        SceneController.Instance?.LoadScene(sceneName, delaySeconds);
    }

    public void QuitGame()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.QuitGame();
    }
}
