using UnityEngine;

/// <summary>
/// Controla un efecto de temblor de cámara con fade in y fade out
/// </summary>
public class CameraShackePuzzle : MonoBehaviour
{
    [SerializeField] float shakeMagnitude = 0.5f;
    [SerializeField] float shakeDuration = 1f;

    #region Internal 
    Vector3 initialPosition;
    float currentShakeDuration = 0f;
    bool isShaking = false;
    #endregion

    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isShaking)
        {
            if (currentShakeDuration > 0)
            {
                // Calculamos un valor entre 0 y 1 según el tiempo restante
                float normalizedTime = 1 - (currentShakeDuration / shakeDuration);

                // Fade in: primeros 20% de la duración, fade out: últimos 20%
                float intensityMultiplier = Mathf.Sin(normalizedTime * Mathf.PI);
                // Esto hace que comience y termine en 0, con máximo en medio

                transform.localPosition = initialPosition + Random.insideUnitSphere * shakeMagnitude * intensityMultiplier;

                currentShakeDuration -= Time.deltaTime;
            }
            else
            {
                isShaking = false;
                transform.localPosition = initialPosition;
            }
        }
    }

    /// <summary>
    /// Activa el temblor de cámara por una duración específica y con magnitud definida
    /// </summary>
    public void TriggerShake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
        currentShakeDuration = duration;
        isShaking = true;
    }
}
