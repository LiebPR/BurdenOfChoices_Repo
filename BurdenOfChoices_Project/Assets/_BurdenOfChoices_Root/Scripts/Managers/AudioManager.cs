using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[System.Serializable]
public class SFXData
{
    public string id; //Nombre del SFX principal
    public AudioClip mainClip;
    public bool isRandom = false;
    public AudioClip[] randomClips; //clips aleatorios
}

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
        // Cargar SFXData en diccionario
        sfxDataLibrary.Clear();
        foreach (var sfx in sfxDatabase)
        {
            if (sfx != null && !string.IsNullOrEmpty(sfx.id))
                sfxDataLibrary[sfx.id] = sfx;
        }
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
    public SFXData[] sfxDatabase;
    #endregion

    #region AudioLibrary
    // Diccionario de AudioClips
    public Dictionary<string, AudioClip> musicLibrary = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioClip> ambientLibrary = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioClip> sfxLibrary = new Dictionary<string, AudioClip>();
    public Dictionary<string, SFXData> sfxDataLibrary = new Dictionary<string, SFXData>();

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
    public void PlayMusic(string trackID, float fadeTime = 1f)
    {
        if (!musicLibrary.ContainsKey(trackID))
        {
            Debug.LogWarning("Clip no encontrado: " + trackID);
            return;
        }

        // Si no hay música sonando, entra directo
        if (!musicSource.isPlaying)
        {
            musicSource.clip = musicLibrary[trackID];
            musicSource.volume = 0f;
            musicSource.loop = true;
            musicSource.Play();
            StartCoroutine(FadeVolume(musicSource, musicVolume, fadeTime));
            return;
        }

        // Si hay música, transición con fade
        StartCoroutine(ChangeMusicRoutine(trackID, fadeTime));
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
        // Esperar un frame para dejar que la escena termine de cargar
        yield return null;

        if (musicSource.isPlaying)
            yield return FadeVolume(musicSource, 0, fadeTime * 0.5f, stopAfterFade: true);

        musicSource.clip = musicLibrary[newTrack];
        musicSource.volume = 0f;
        musicSource.loop = true;
        musicSource.Play();

        yield return FadeVolume(musicSource, musicVolume, fadeTime * 0.5f);
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
    AudioClip GetSFXClip(string sfxID)
    {
        if (sfxDataLibrary.ContainsKey(sfxID))
        {
            var sfx = sfxDataLibrary[sfxID];
            if (sfx.isRandom && sfx.randomClips.Length > 0)
            {
                int rand = Random.Range(0, sfx.randomClips.Length);
                return sfx.randomClips[rand];
            }
            return sfx.mainClip;
        }
        if (sfxLibrary.ContainsKey(sfxID)) return sfxLibrary[sfxID];
        return null;
    }

    public void PlaySFX2D(string sfxID, float volume = 1f)
    {
        AudioClip clip = GetSFXClip(sfxID); // Evaluar solo una vez
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] SFX no encontrado: " + sfxID);
            return;
        }

        GameObject go = new GameObject("SFX2D_" + sfxID);
        go.transform.parent = sfxContainer; // opcional, para mantener jerarquía limpia

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * sfxVolume * masterVolume;
        source.spatialBlend = 0f; // 2D
        source.Play();

        Destroy(go, clip.length + 0.1f); // Se destruye al terminar
    }

    public AudioSource PlaySFX2DLoop(string sfxID, bool loop, float volume = 1f, float pitch = 1f)
    {
        AudioClip clip = GetSFXClip(sfxID);
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] SFX no encontrado: " + sfxID);
            return null;
        }

        GameObject go = new GameObject("SFX2D_Loop_" + sfxID);
        go.transform.parent = sfxContainer;

        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.volume = volume * sfxVolume * masterVolume;
        source.pitch = pitch;
        source.spatialBlend = 0f;
        source.Play();

        return source;
    }

    public AudioEmitter PlaySFXAtPosition(string sfxID, Vector3 position, float volume = 1f)
    {
        AudioClip clip = GetSFXClip(sfxID);
        if (clip == null) return null;

        GameObject go = new GameObject("SFX_" + sfxID);
        go.transform.position = position;
        go.transform.parent = sfxContainer;
        AudioEmitter emitter = go.AddComponent<AudioEmitter>();
        emitter.Play3D(clip, false, volume * sfxVolume, 0);
        return emitter;
    }

    public AudioEmitter PlaySFXAttached(string sfxID, Transform parent, bool loop = false, float volume = 1f)
    {
        AudioClip clip = GetSFXClip(sfxID);
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] SFX no encontrado: " + sfxID);
            return null;
        }

        GameObject go = new GameObject("SFX_" + sfxID);
        go.transform.parent = parent;
        go.transform.localPosition = Vector3.zero;
        AudioEmitter emitter = go.AddComponent<AudioEmitter>();
        emitter.Play3D(clip, loop, volume * sfxVolume, 0);
        return emitter;
    }

    public void PlayAnimationSFX(string sfxID, Transform origin, float rangeMultiplier = 1f)
    {
        AudioClip clip = GetSFXClip(sfxID);
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] SFX no encontrado: " + sfxID);
            return;
        }

        GameObject go = new GameObject("AnimSFX_" + sfxID);
        go.transform.parent = origin;
        go.transform.localPosition = Vector3.zero;

        AudioEmitter emitter = go.AddComponent<AudioEmitter>();
        emitter.Play3D(clip, false, sfxVolume, 0);

        var src = go.GetComponent<AudioSource>();
        src.minDistance *= rangeMultiplier;
        src.maxDistance *= rangeMultiplier;
    }

    public void StopSFX(AudioEmitter emitter)
    {
        if (emitter != null) emitter.Stop(0);
    }

    public void StopSFX2D(AudioSource source)
    {
         if (source == null) return;
         Destroy(source.gameObject);
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
