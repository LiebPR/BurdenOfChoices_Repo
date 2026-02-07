using UnityEngine;
using System;

public class TriggerNotifier : MonoBehaviour
{
    public event Action OnTriggerEntered;

    private void OnTriggerEnter(Collider other)
    {
        // Solo reaccionamos al jugador
        if (other.CompareTag("Player"))
        {
            OnTriggerEntered?.Invoke();
        }
    }
}
