using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("References")]
    [SerializeField] FadeController fadeController;

    bool isTransitioning;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        var es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (es != null)
            DontDestroyOnLoad(es.gameObject);

        if (fadeController == null)
            fadeController = FindAnyObjectByType<FadeController>();
    }

    #region Public API
    /// <summary>
    /// Carga una escena controlando:
    /// - Delay de salida de escena
    /// - Delay previo al fade
    /// - Duración del FadeOut
    /// - Duración del FadeIn
    /// </summary>
    public void LoadScene(string sceneName, float exitDelay = 0f, float fadeDelay = 0f, float fadeOutDuration = -1f, float fadeInDuration = -1f)
    {
        if (isTransitioning) return;
        StartCoroutine(LoadSceneRoutine(
            sceneName,
            exitDelay,
            fadeDelay,
            fadeOutDuration,
            fadeInDuration
        ));
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Internal Logic

    IEnumerator LoadSceneRoutine(string sceneName, float exitDelay, float fadeDelay, float fadeOutDuration, float fadeInDuration)
    {
        isTransitioning = true;

        // 1. Delay de salida de escena
        if (exitDelay > 0f)
            yield return new WaitForSeconds(exitDelay);

        // 2. Delay previo al fade
        if (fadeDelay > 0f)
            yield return new WaitForSeconds(fadeDelay);

        // 3. Fade Out
        if (fadeController != null)
            yield return fadeController.FadeOut(fadeOutDuration);

        // 4. Carga de escena
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // Estado del juego
        if (GameDirector.Instance != null)
        {
            if (sceneName.ToLower().Contains("menu"))
            {
                GameDirector.Instance.SetPhase(GamePhase.Menu);
            }
            else
            {
                GameDirector.Instance.SetPhase(GamePhase.Playing);
                GameDirector.Instance.ResetOutcome();
            }
        }

        // 5. Fade In
        if (fadeController != null)
            yield return fadeController.FadeIn(fadeInDuration);

        isTransitioning = false;
    }

    #endregion
}
