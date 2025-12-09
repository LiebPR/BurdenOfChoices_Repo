using UnityEngine;

/// <summary>
/// Gestiona colisiones de objetos que generan ruido detectable por los enemigos.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class ItemCollisionHandler : MonoBehaviour
{
    [SerializeField] float noiseIntensity = 1f; // Intensidad base del ruido generado al chocar
    [SerializeField] EnemyData enemyData;
    [SerializeField] LayerMask noiseLayerMask;

    private void OnCollisionEnter(Collision collision)
    {
        // Comprobamos si la capa del objeto colisionado está incluida en el LayerMask
        if ((noiseLayerMask.value & (1 << collision.gameObject.layer)) == 0)
            return; // No está en las capas que generan ruido

        // Calculamos el punto del impacto
        Vector3 impactPoint = collision.contacts[0].point;

        // Detectamos todos los enemigos en escena
        HearingSystem[] enemies = FindObjectsByType<HearingSystem>(FindObjectsSortMode.None);

        foreach (var enemy in enemies)
        {
            // Calculamos la distancia al enemigo
            float distance = Vector3.Distance(enemy.transform.position, impactPoint);
            float maxRadius = enemyData.maxHearingRadius;

            if (distance <= maxRadius)
            {
                // Informamos al HearingSystem del ruido
                enemy.ReportNoise(impactPoint, noiseIntensity);
            }
        }
    }
}