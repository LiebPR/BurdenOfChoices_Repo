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
            Debug.Log($"[TriggerNotifier] Trigger activado en {gameObject.name} por {other.name}");
            OnTriggerEntered?.Invoke();
        }
    }
}
