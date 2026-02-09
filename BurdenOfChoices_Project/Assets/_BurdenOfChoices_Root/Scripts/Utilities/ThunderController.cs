using System.Collections;
using UnityEngine;

public class ThunderController : MonoBehaviour
{
    #region Inspector
    [Header("Cooldown")]
    [SerializeField] float cooldownTime = 5f;

    [Header("Audio")]
    [SerializeField] string lightningSFXID = "Thunder";
    [SerializeField] float thunderDelay = 0.3f;

    [Header("Lights")]
    [SerializeField] Light[] thunderLights;
    [SerializeField] float maxIntensity = 5f;
    [SerializeField] int flashCount = 3;
    [SerializeField] float flashDuration = 0.1f;
    [SerializeField] float fadeOutTime = 1f;
    #endregion

    #region Internal State
    bool isActive;
    bool inCooldown;
    #endregion

    #region Public API
    /// <summary>
    /// Llamado por cualquier TriggerNotifier.
    /// </summary>
    public void TriggerThunder()
    {
        if (isActive || inCooldown) return;

        StartCoroutine(ThunderRoutine());
    }
    #endregion

    #region Thunder Logic
    IEnumerator ThunderRoutine()
    {
        isActive = true;

        // Espera antes de reproducir el sonido
        yield return new WaitForSeconds(thunderDelay);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX2D(lightningSFXID);

        // Parpadeo de las luces
        yield return StartCoroutine(LightningFlashRoutine());

        isActive = false;
        StartCoroutine(CooldownRoutine());
    }

    IEnumerator LightningFlashRoutine()
    {
        // Asegurarse de que todas las luces empiezan apagadas
        foreach (var light in thunderLights)
            light.intensity = 0f;

        // Parpadeo rápido
        for (int i = 0; i < flashCount; i++)
        {
            foreach (var light in thunderLights)
                light.intensity = Random.Range(maxIntensity * 0.7f, maxIntensity);

            yield return new WaitForSeconds(flashDuration);

            foreach (var light in thunderLights)
                light.intensity = 0f;

            yield return new WaitForSeconds(flashDuration * 0.5f);
        }

        // Fade out final (suaviza si alguna luz quedó encendida)
        float timer = 0f;
        while (timer < fadeOutTime)
        {
            float t = 1 - (timer / fadeOutTime);
            foreach (var light in thunderLights)
                light.intensity = Mathf.Lerp(0f, maxIntensity, t);

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var light in thunderLights)
            light.intensity = 0f;
    }

    IEnumerator CooldownRoutine()
    {
        inCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        inCooldown = false;
    }
    #endregion
}
