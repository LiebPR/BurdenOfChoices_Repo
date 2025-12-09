using System;
using UnityEngine;

public class HearingSystem : MonoBehaviour
{
    [SerializeField] EnemyData enemyData;

    Transform player;
    PlayerNoiseEmitter noiseEmitter;

    float lastNoiseDistance = Mathf.Infinity;  // Para rastrear la última distancia del ruido

    #region Events
    public event Action<Transform> OnHearTarget;
    public event Action<Vector3> OnHearNoisePoint; // Para objetos o ruido genérico
    public event Action<Transform> OnLoseNoise; // Evento cuando el enemigo pierde el sonido
    #endregion

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        noiseEmitter = player.GetComponent<PlayerNoiseEmitter>();
    }

    private void Update()
    {
        // Comprobación de ruido del jugador
        float noise = noiseEmitter.CurrentNoise();
        if (noise > 0f)
        {
            // Calculamos el radio de percepción del sonido
            float radius = enemyData.maxHearingRadius * noise;
            float distance = Vector3.Distance(transform.position, player.position);

            // Si el enemigo puede escuchar al jugador
            if (distance <= radius)
            {
                if (distance != lastNoiseDistance)  // Si la distancia ha cambiado, indicamos que ha comenzado a oír
                {
                    OnHearTarget?.Invoke(player);
                    lastNoiseDistance = distance;
                }
            }
        }
        else
        {
            // Si no hay ruido (el jugador no se mueve o está en silencio), verificamos si el enemigo debería perder la percepción
            if (lastNoiseDistance < Mathf.Infinity)
            {
                OnLoseNoise?.Invoke(player); // Disparamos el evento de pérdida
                lastNoiseDistance = Mathf.Infinity; // Resetamos la distancia
            }
        }
    }

    // Permite que cualquier objeto genere un ruido detectable por el enemigo.
    public void ReportNoise(Vector3 position, float intensity = 1f)
    {
        float radius = enemyData.maxHearingRadius * intensity;
        float distance = Vector3.Distance(transform.position, position);

        if (distance <= radius)
        {
            OnHearNoisePoint?.Invoke(position);
        }
    }
}
