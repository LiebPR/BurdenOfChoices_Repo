using UnityEngine;

public class SceneMusicController : MonoBehaviour
{
    [SerializeField] string menuMusicID = "MenuTrack";
    [SerializeField] float fadeInTime = 1f;

    private void Start()
    {
        Debug.Log("PlayMusic llamado: " + menuMusicID);
        AudioManager.Instance.PlayMusic(menuMusicID, fadeInTime);
    }
}
