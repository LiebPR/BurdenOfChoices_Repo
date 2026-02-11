using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogUI : MonoBehaviour
{
    #region References
    [SerializeField] GameObject dialogPanel;
    [SerializeField] Image portraitImage; //Imagen que se actualizará según emoción
    [SerializeField] TMP_Text dialogText;
    [SerializeField] TMP_Text speakerNameText;
    #endregion

    #region Internal
    Coroutine typingCoroutine;
    Coroutine portraitCoroutine;
    CanvasGroup portraitCanvasGroup;
    #endregion

    #region Getters
    public bool IsTyping => typingCoroutine != null;
    #endregion

    #region Audio
    string typingSFXID;
    float typingSFXInterval;
    float typingVolume;
    float lastTypingSFXTime;
    #endregion

    private void Awake()
    {
        if (portraitImage != null)
        {
            portraitCanvasGroup = portraitImage.GetComponent<CanvasGroup>();
            if (portraitCanvasGroup == null)
                portraitCanvasGroup = portraitImage.gameObject.AddComponent<CanvasGroup>();
        }
    }

    #region Public API
    public void Show(){ dialogPanel.SetActive(true);}

    public void Hide()
    {
        dialogPanel.SetActive(false);
        dialogText.text = "";
        speakerNameText.text = "";

        // Detener fade si está activo
        if (portraitCoroutine != null)
            StopCoroutine(portraitCoroutine);
        portraitCoroutine = null;

        if (portraitImage != null)
            portraitImage.gameObject.SetActive(false);
    }

    public void SetText(string text, float typeSpeed = 0.03f)
    {
        //Si ya está escribiendo, detener
        if(typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeTextCoroutine(text, typeSpeed));
    }

    public void SkipTyping()
    {
        //muestra el texto completo de golpe
        if(typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }
    }

    public void SetTypingAudio(string sfxID, float interval, float volume)
    {
        typingSFXID = sfxID;
        typingSFXInterval = interval;
        typingVolume = volume;
    }

    public void SetSpeakerName(string speakerName)
    {
        speakerNameText.text = speakerName;
        speakerNameText.gameObject.SetActive(!string.IsNullOrEmpty(speakerName));
    }

    public void SetPortrait(Sprite portrait)
    {
        if (portraitImage == null) return;

        portraitImage.sprite = portrait;
        portraitImage.gameObject.SetActive(portrait != null);
    }
    #endregion

    #region Typewriter Coroutine
    IEnumerator TypeTextCoroutine(string text, float typeSpeed)
    {
        dialogText.text = text;
        dialogText.maxVisibleCharacters = 0;
        lastTypingSFXTime = 0f;

        for (int i = 0; i <= text.Length; i++)
        {
            dialogText.maxVisibleCharacters = i;

            if (i > 0)
            {
                char c = text[i - 1];

                // Ignorar espacios y signos
                if (!char.IsWhiteSpace(c) && char.IsLetter(c))
                {
                    if (Time.time - lastTypingSFXTime >= typingSFXInterval)
                    {
                        AudioManager.Instance.PlaySFX2D(typingSFXID, typingVolume);
                        lastTypingSFXTime = Time.time;
                    }
                }
            }

            yield return new WaitForSeconds(typeSpeed);
        }

        typingCoroutine = null;
    }
    #endregion
}
