using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    Canvas videoCanvas;
    RawImage videoImage;
    VideoPlayer videoPlayer;
    GraphicRaycaster raycaster;
    Coroutine playRoutine;

    void Awake()
    {
        CreateVideoUI();
    }

    /// <summary>
    /// Reproduce un video haciendo fade a transparente al empezar y fade a negro al terminar
    /// </summary>
    public void PlayVideoWithFade(VideoClip clip, Action onFinished = null)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayVideoRoutine(clip, onFinished));
    }

    /// <summary>
    /// Skip suave: hace fadeOut antes de llamar al callback
    /// </summary>
    public void SkipVideo(Action onFinished)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(SkipVideoRoutine(onFinished));
    }

    private IEnumerator PlayVideoRoutine(VideoClip clip, Action onFinished)
    {
        // 1. FadeIn: pantalla negra → transparente
        yield return FadeController.Instance.FadeIn();

        // 2. Activar UI y reproducir video
        videoCanvas.gameObject.SetActive(true);
        raycaster.enabled = true;
        videoImage.color = Color.white;

        videoPlayer.clip = clip;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        videoPlayer.Play();

        // 3. Espera a que termine el video
        while (videoPlayer.isPlaying)
            yield return null;

        // 4. FadeOut: transparente → negro
        yield return FadeController.Instance.FadeOut();

        // 5. Callback
        onFinished?.Invoke();
        playRoutine = null;
    }

    private IEnumerator SkipVideoRoutine(Action onFinished)
    {
        // 1. FadeOut inmediato
        yield return FadeController.Instance.FadeOut();

        // 2. Callback
        onFinished?.Invoke();
        playRoutine = null;
    }

    private void CreateVideoUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("VideoCanvas");
        canvasGO.transform.SetParent(transform, false);
        videoCanvas = canvasGO.AddComponent<Canvas>();
        videoCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        videoCanvas.sortingOrder = 99;

        raycaster = canvasGO.AddComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        // RawImage
        GameObject rawObj = new GameObject("VideoImage");
        rawObj.transform.SetParent(videoCanvas.transform, false);
        videoImage = rawObj.AddComponent<RawImage>();
        videoImage.color = Color.black;
        RectTransform rt = videoImage.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // VideoPlayer
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        RenderTexture rtTex = new RenderTexture(1920, 1080, 16);
        rtTex.Create();
        videoPlayer.targetTexture = rtTex;
        videoImage.texture = rtTex;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        var audioSource = gameObject.AddComponent<AudioSource>();
        videoPlayer.SetTargetAudioSource(0, audioSource);

        videoCanvas.gameObject.SetActive(false);
    }
}
