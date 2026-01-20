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
        dialogText.text = "";
        foreach(char c in text)
        {
            dialogText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        typingCoroutine = null; 
    }
    #endregion
}
