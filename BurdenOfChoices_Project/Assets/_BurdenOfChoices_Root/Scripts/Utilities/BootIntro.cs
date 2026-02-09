using UnityEngine;
using UnityEngine.Video;

public class BootIntro : MonoBehaviour
{
    [Header("Intro Video")]
    [SerializeField] VideoController videoController;
    [SerializeField] VideoClip introClip;

    [Header("Next Scene")]
    [SerializeField] string nextScene = "SCN_AlejandroMainMenu";

    void OnEnable()
    {
        MenuInputHandler.OnSkipConfirmed += SkipIntro;
    }

    void OnDisable()
    {
        MenuInputHandler.OnSkipConfirmed -= SkipIntro;
    }

    void Start()
    {
        PlayIntro();
    }

    void PlayIntro()
    {
        // Lanza el video con fade controlado dentro del VideoController
        videoController.PlayVideoWithFade(introClip, () =>
        {
            // Cambiar escena al terminar el video
            SceneController.Instance.LoadScene(nextScene);
        });
    }

    void SkipIntro()
    {
        videoController.SkipVideo(() =>
        {
            SceneController.Instance.LoadScene(nextScene);
        });
    }
}
