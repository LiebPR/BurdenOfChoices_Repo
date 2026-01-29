using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager centralizado. Maneja música, ambiente y SFX (2D y 3D).
/// Singleton persistente entre escenas.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Singleton
    public static AudioManager Instance;
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Cargar todos los clips asignados en el Inspector
        LoadClips(musicClips, ambientClips, sfxClips);
    }
    #endregion

    #region Channels
    // AudioSources fijos por canal
    public AudioSource musicSource;
    public AudioSource ambientSource;

    // Canal de SFX temporal
    public Transform sfxContainer;

    // Volumen global
    [Range(0, 1)] public float masterVolume = 1f;
    [Range(0, 1)] public float musicVolume = 1f;
    [Range(0, 1)] public float ambientVolume = 1f;
    [Range(0, 1)] public float sfxVolume = 1f;
    #endregion
    
    #region Inspector Arrays
    [Header("Music Clips")]
    public AudioClip[] musicClips;

    [Header("Ambient Clips")]
    public AudioClip[] ambientClips;

    [Header("SFX Clips")]
    public AudioClip[] sfxClips;
    #endregion
    
    #region AudioLibrary
    // Diccionario de AudioClips
    public Dictionary<string, AudioClip> musicLibrary = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioClip> ambientLibrary = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();

    /// <summary>
    /// Se pueden cargar todos los clips desde Resources o Addressables
    /// </summary>
    public void LoadClips(AudioClip[] musics, AudioClip[] ambients, AudioClip[] sfx)
    {
        foreach (var c in musics) musicLibrary[c.name] = c;
        foreach (var c in ambients) ambientLibrary[c.name] = c;
        foreach (var c in sfx) sfxLibrary[c.name] = c;
    }
    #endregion

    #region Music API
    public void PlayMusic(string trackID, float fadeInTime = 1f)
    {
        if (!musicLibrary.ContainsKey(trackID))
        {
            Debug.LogWarning("Clip no encontrado: " + trackID);
            return;
        }

        if (!musicLibrary.ContainsKey(trackID)) return;
        StopMusic();
        musicSource.clip = musicLibrary[trackID];
        musicSource.volume = 0;
        musicSource.loop = true;
        musicSource.Play();
        StartCoroutine(FadeVolume(musicSource, musicVolume, fadeInTime));
    }

    public void StopMusic(float fadeOutTime = 1f)
    {
        if (musicSource.isPlaying)
            StartCoroutine(FadeVolume(musicSource, 0, fadeOutTime, stopAfterFade: true));
    }

    public void ChangeMusic(string trackID, float fadeTime = 1f)
    {
        if (!musicLibrary.ContainsKey(trackID)) return;
        StartCoroutine(ChangeMusicRoutine(trackID, fadeTime));
    }

    System.Collections.IEnumerator ChangeMusicRoutine(string newTrack, float fadeTime)
    {
        yield return FadeVolume(musicSource, 0, fadeTime / 2f, stopAfterFade: true);
        musicSource.clip = musicLibrary[newTrack];
        musicSource.Play();
        yield return FadeVolume(musicSource, musicVolume, fadeTime / 2f);
    }
    #endregion

    #region Ambient API
    public void PlayAmbient(string ambientID, float fadeInTime = 1f)
    {
        if (!ambientLibrary.ContainsKey(ambientID)) return;
        ambientSource.clip = ambientLibrary[ambientID];
        ambientSource.loop = true;
        ambientSource.volume = 0;
        ambientSource.Play();
        StartCoroutine(FadeVolume(ambientSource, ambientVolume, fadeInTime));
    }

    public void StopAmbient(float fadeOutTime = 1f)
    {
        if (ambientSource.isPlaying)
            StartCoroutine(FadeVolume(ambientSource, 0, fadeOutTime, stopAfterFade: true));
    }

    public AudioEmitter PlayAmbientAt(string ambientID, Vector3 position, float fadeInTime = 0.5f)
    {
        if (!ambientLibrary.ContainsKey(ambientID)) return null;
        GameObject go = new GameObject("AmbientEmitter_" + ambientID);
        go.transform.position = position;
        go.transform.parent = sfxContainer;
        AudioEmitter emitter = go.AddComponent<AudioEmitter>();
        emitter.Play3D(ambientLibrary[ambientID], true, ambientVolume, fadeInTime);
        return emitter;
    }

    public void StopAmbientAt(AudioEmitter emitter, float fadeOutTime = 0.5f)
    {
        if (emitter != null)
            emitter.Stop(fadeOutTime);
    }
    #endregion

    #region SFX API
    public void PlaySFX2D(string sfxID, float volume = 1f)
    {
        if (!sfxLibrary.ContainsKey(sfxID)) return;
        AudioSource.PlayClipAtPoint(sfxLibrary[sfxID], Camera.main.transform.position, volume * sfxVolume * masterVolume);
    }

    public AudioEmitter PlaySFXAtPosition(string sfxID, Vector3 position, float volume = 1f)
    {
        if (!sfxLibrary.ContainsKey(sfxID)) return null;
        GameObject go = new GameObject("SFX_" + sfxID);
        go.transform.position = position;
        go.transform.parent = sfxContainer;
        AudioEmitter emitter = go.AddComponent<AudioEmitter>();
        emitter.Play3D(sfxLibrary[sfxID], false, volume * sfxVolume, 0);
        return emitter;
    }

    public AudioEmitter PlaySFXAttached(string sfxID, Transform parent, float volume = 1f)
    {
        if (!sfxLibrary.ContainsKey(sfxID)) return null;
        GameObject go = new GameObject("SFX_" + sfxID);
        go.transform.parent = parent;
        go.transform.localPosition = Vector3.zero;
        AudioEmitter emitter = go.AddComponent<AudioEmitter>();
        emitter.Play3D(sfxLibrary[sfxID], false, volume * sfxVolume, 0);
        return emitter;
    }

    public void StopSFX(AudioEmitter emitter)
    {
        if (emitter != null) emitter.Stop(0);
    }
    #endregion

    #region Volume & Fade Helpers
    System.Collections.IEnumerator FadeVolume(AudioSource source, float targetVolume, float duration, bool stopAfterFade = false)
    {
        float start = source.volume;
        float time = 0;
        while (time < duration)
        {
            source.volume = Mathf.Lerp(start, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = targetVolume;
        if (stopAfterFade) source.Stop();
    }
    #endregion
}
