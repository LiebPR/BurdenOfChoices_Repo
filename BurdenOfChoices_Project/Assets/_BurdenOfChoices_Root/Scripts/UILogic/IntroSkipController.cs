using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class IntroSkipController : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] float fadeOutDuration = 1f;

    private void OnEnable()
    {
        MenuInputHandler.OnSkipConfirmed += Skip;
    }

    private void OnDisable()
    {
        MenuInputHandler.OnSkipConfirmed -= Skip;
    }

    void Skip()
    {
        // Inicia coroutine de fade out
        StartCoroutine(FadeOutVideoAudioAndLoad());
    }

    IEnumerator FadeOutVideoAudioAndLoad()
    {
        if (videoPlayer != null)
        {
            double startVolume = videoPlayer.GetDirectAudioVolume(0);
            float time = 0f;

            while (time < fadeOutDuration)
            {
                float t = time / fadeOutDuration;
                videoPlayer.SetDirectAudioVolume(0, (float)(startVolume * (1 - t)));
                time += Time.deltaTime;
                yield return null;
            }

            // Aseguramos volumen a 0
            videoPlayer.SetDirectAudioVolume(0, 0f);
        }

        // Finalmente carga la escena del menú
        SceneController.Instance.LoadScene(sceneToLoad);
    }
}
