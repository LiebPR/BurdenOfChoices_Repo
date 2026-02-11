using UnityEngine;

public class RestoreSFXAmbient : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.Instance.StopAllSFX();
        AudioManager.Instance.StopAllAmbient();
    }
}
