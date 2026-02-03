using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    public event Action OnSceneChangeStart;
    public event Action OnSceneChangeComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// Carga escena con fade y delay opcional antes de empezar.
    /// </summary>
    public void LoadScene(string sceneName, float delaySeconds = 0f)
    {
        StartCoroutine(SceneTransition(sceneName, delaySeconds));
    }

    IEnumerator SceneTransition(string sceneName, float delaySeconds)
    {
        if (delaySeconds > 0f)
            yield return new WaitForSeconds(delaySeconds);

        // Notificar inicio
        OnSceneChangeStart?.Invoke();

        // Hacer fade out y esperar a que termine
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut();

        // Cargar la escena
        yield return SceneManager.LoadSceneAsync(sceneName);

        // Notificar final
        OnSceneChangeComplete?.Invoke();
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());
    }

    IEnumerator QuitGameRoutine()
    {
        if (FadeController.Instance != null)
            yield return FadeController.Instance.FadeOut();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
