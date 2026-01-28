using UnityEngine;
using UnityEngine.Video;

public class IntroVideoController : MonoBehaviour
{
    #region References
    [SerializeField] VideoPlayer videoPlayer;
    #endregion

    #region Settings
    [Header("PlayBack")]
    [SerializeField] bool loopVideo;

    [Header("Scene Transition")]
    [SerializeField] bool loadSceneOnEnd;
    [SerializeField] string sceneToLoad;
    #endregion

    private void Start()
    {
        ConfigureVideo();
        PlayVideo();
    }

    #region Video Logic
    void ConfigureVideo()
    {
        videoPlayer.isLooping = loopVideo;

        if(!loopVideo && loadSceneOnEnd)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void PlayVideo()
    {
        videoPlayer.Play();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        // Load the specified scene
        SceneController.Instance.LoadScene(sceneToLoad);
    }
    #endregion 
}
