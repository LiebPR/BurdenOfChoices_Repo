using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public static FadeController Instance;

    public Image fadeImage;
    public float fadeDuration = 1f;
    public CanvasGroup fadeCanvasGroup;

    public bool IsFaded { get; private set; } = true;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(FadeIn(1f));
    }

    public IEnumerator FadeOut(float? customDuration = null)
    {
        yield return Fade(1f, customDuration ?? fadeDuration);
    }

    public IEnumerator FadeIn(float? customDuration = null)
    {
        yield return Fade(0f, customDuration ?? fadeDuration);
    }

    IEnumerator Fade(float targetAlpha, float duration)
    {
        float startAlpha = fadeImage.color.a;
        float timer = 0f;

        fadeCanvasGroup.blocksRaycasts = true;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
            fadeImage.color = new Color(
                fadeImage.color.r,
                fadeImage.color.g,
                fadeImage.color.b,
                alpha
            );

            fadeCanvasGroup.blocksRaycasts = alpha > 0.01f;
            yield return null;
        }

        fadeImage.color = new Color(
            fadeImage.color.r,
            fadeImage.color.g,
            fadeImage.color.b,
            targetAlpha
        );

        IsFaded = targetAlpha == 1f;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.01f;
    }
}
