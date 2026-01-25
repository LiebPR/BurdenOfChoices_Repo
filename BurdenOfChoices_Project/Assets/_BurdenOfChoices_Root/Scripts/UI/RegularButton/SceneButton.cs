using UnityEngine;

public class SceneButton : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Nombre de la escena a cargar")]
    [SerializeField] string sceneName;
    [SerializeField] float delay = 0.1f;

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
            SceneController.Instance.LoadScene(sceneName, delay);
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
