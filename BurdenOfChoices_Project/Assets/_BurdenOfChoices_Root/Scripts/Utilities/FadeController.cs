using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    #region Inspector
    [Header("UI References")]
    [SerializeField] Image fadeImage;

    [Header("Fade Settings")]
    [SerializeField] float defaultFadeTime = 0.5f;
    #endregion

    #region Internal
    Canvas fadeCanvas;
    GraphicRaycaster raycaster;
    Coroutine fadeRoutine;
    #endregion

    #region Getters
    public bool IsFading => fadeRoutine != null;
    #endregion

    #region Unity Lifecycle
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupCanvas();
        TryFindFadeImage();
        SetAlpha(0f);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    #endregion

    #region Scene Handling
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindFadeImage();
        SetAlpha(0f);
    }
    #endregion

    #region Public API — Coroutine (compatibilidad)
    public IEnumerator FadeOut(float duration = -1f)
    {
        yield return StartFade(0f, 1f, duration);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        yield return StartFade(1f, 0f, duration);
    }
    #endregion

    #region Public API — Callback (SECUENCIAS)
    public void FadeOut(Action onFinished, float duration = -1f)
    {
        StartCoroutine(FadeWithCallback(0f, 1f, duration, onFinished));
    }

    public void FadeIn(Action onFinished, float duration = -1f)
    {
        StartCoroutine(FadeWithCallback(1f, 0f, duration, onFinished));
    }
    #endregion

    #region Core Fade Logic
    IEnumerator FadeWithCallback(float from, float to, float duration, Action onFinished)
    {
        yield return StartFade(from, to, duration);
        onFinished?.Invoke();
    }

    IEnumerator StartFade(float from, float to, float duration)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(CrossFade(from, to, duration));
        yield return fadeRoutine;
        fadeRoutine = null;
    }

    IEnumerator CrossFade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        float time = duration <= 0 ? defaultFadeTime : duration;
        float elapsed = 0f;

        if (raycaster != null)
            raycaster.enabled = to > from;

        Color c = fadeImage.color;

        while (elapsed < time)
        {
            elapsed += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(from, to, elapsed / time);
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;

        if (raycaster != null)
            raycaster.enabled = to > 0f;
    }
    #endregion

    #region Setup
    void SetupCanvas()
    {
        fadeCanvas = GetComponent<Canvas>();
        if (fadeCanvas == null) return;

        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 1000;

        raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            raycaster = gameObject.AddComponent<GraphicRaycaster>();
    }

    void TryFindFadeImage()
    {
        if (fadeImage != null) return;

        GameObject canvasObj = GameObject.Find("C_Fade");
        if (canvasObj == null) return;

        fadeImage = canvasObj.transform.Find("P_Fade")?.GetComponent<Image>();
    }

    void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color c = fadeImage.color;
        c.a = alpha;
        fadeImage.color = c;

        if (raycaster != null)
            raycaster.enabled = alpha > 0f;
    }
    #endregion
}
