using UnityEngine;
using System.Collections;

/// <summary>
/// AudioEmitter: Representa un AudioSource 3D temporal o ligado.
/// Controla reproducción, fade, loop y destrucción.
/// </summary>
public class AudioEmitter : MonoBehaviour
{
    AudioSource source;

    public void Play3D(AudioClip clip, bool loop, float volume = 1f, float fadeIn = 0f)
    {
        source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = 0f;
        source.loop = loop;
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 50f;
        source.Play();
        StartCoroutine(FadeVolume(volume, fadeIn));
        if (!loop) Destroy(gameObject, clip.length + 0.1f);
    }

    public void Stop(float fadeOut = 0f)
    {
        if (fadeOut > 0)
            StartCoroutine(FadeOutAndDestroy(fadeOut));
        else
            Destroy(gameObject);
    }

    IEnumerator FadeVolume(float target, float duration)
    {
        float start = source.volume;
        float time = 0;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(start, target, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = target;
    }

    IEnumerator FadeOutAndDestroy(float duration)
    {
        float start = source.volume;
        float time = 0;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(start, 0, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
