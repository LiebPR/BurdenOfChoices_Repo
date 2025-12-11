using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public static SceneController Instance;

    [Header("References")]
    public FadeController fadeController;

    private bool isTransitioning = false;
    private float defaultDelayBetween = 0.1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Mantener EventSystem
            var es = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (es != null)
                DontDestroyOnLoad(es.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeController == null)
        {
            fadeController = FindAnyObjectByType<FadeController>();
        }
    }

    #region Public API
    public void LoadScene(string sceneName, float delay = -1f)
    {
        if (isTransitioning) return;
        float useDelay = delay < 0 ? defaultDelayBetween : delay;
        StartCoroutine(LoadSceneRoutine(sceneName, useDelay));
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

    #region Internal
    private IEnumerator LoadSceneRoutine(string sceneName, float delay)
    {
        isTransitioning = true;

        if (fadeController != null)
            yield return fadeController.FadeOut();

        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        if (GameDirector.Instance != null)
        {
            string lower = sceneName.ToLower();
            if (lower.Contains("menu"))
            {
                GameDirector.Instance.SetPhase(GamePhase.Menu);
            }
            else
            {
                GameDirector.Instance.SetPhase(GamePhase.Playing);
                GameDirector.Instance.ResetOutcome();
            }
        }

        if (fadeController != null)
            yield return fadeController.FadeIn();

        isTransitioning = false;
    }
    #endregion
}
