using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [SerializeField] string sceneMusicID = "MenuTrack";
    [SerializeField] float fadeTime = 1f;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(sceneMusicID, fadeTime);
    }
}
