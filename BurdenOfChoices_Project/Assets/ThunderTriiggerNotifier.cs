using UnityEngine;


public class ThunderTriggerNotifier : MonoBehaviour
{
    [SerializeField] ThunderController thunderController;

    void OnTriggerEnter(Collider other)
    {
        // Solo activar si el objeto que entra tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            thunderController.TriggerThunder();
        }
    }
}
