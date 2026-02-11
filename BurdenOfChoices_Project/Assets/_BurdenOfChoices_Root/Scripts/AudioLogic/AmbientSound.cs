using UnityEngine;

public class AmbientSound : MonoBehaviour
{
    [SerializeField] string ambientSound = "AM_Sound";
    [SerializeField] float volumen = 1f; 

    void Start()
    {
        AudioManager.Instance.PlayAmbient(ambientSound, volumen);
    }
}
