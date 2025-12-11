using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeController : MonoBehaviour
{
    [Header("UI References")]
    public Image fadeImage;

    [Header("Fade Settings")]
    public float defaultFadeTime = 0.5f;

    private Canvas fadeCanvas;
    private GraphicRaycaster raycaster;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        fadeCanvas = GetComponent<Canvas>();
        if (fadeCanvas != null)
        {
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 1000;
        }

        raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            raycaster = gameObject.AddComponent<GraphicRaycaster>();

        TryFindFadeImage();
        if (fadeImage != null) SetAlpha(0f);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryFindFadeImage();
        if (fadeImage != null) SetAlpha(0f);
    }

    private void TryFindFadeImage()
    {
        if (fadeImage != null) return;

        // Buscar dentro del Canvas de la escena
        GameObject canvasObj = GameObject.Find("C_Fade");
        if (canvasObj != null)
        {
            fadeImage = canvasObj.transform.Find("P_Fade")?.GetComponent<Image>();
            if (fadeImage == null)
                Debug.LogWarning("FadeController: No se encontró P_Fade dentro de C_Fade.");
        }
        else
        {
            Debug.LogWarning("FadeController: No se encontró C_Fade en la escena.");
        }
    }

    #region Public API
    public IEnumerator FadeOut(float duration = -1f)
    {
        float t = duration <= 0 ? defaultFadeTime : duration;
        if (raycaster != null) raycaster.enabled = true;
        yield return CrossFade(0f, 1f, t);
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        float t = duration <= 0 ? defaultFadeTime : duration;
        yield return CrossFade(1f, 0f, t);
        if (raycaster != null) raycaster.enabled = false;
    }
    #endregion

    #region Internal
    private IEnumerator CrossFade(float from, float to, float duration)
    {
        if (fadeImage == null) yield break;

        float elapsed = 0f;
        Color c = fadeImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }

        c.a = to;
        fadeImage.color = c;
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        Color c = fadeImage.color;
        c.a = a;
        fadeImage.color = c;
        if (raycaster != null) raycaster.enabled = a > 0f;
    }
    #endregion
}
