using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroSkipUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasGroup group;
    [SerializeField] Image underlineFill;
    [SerializeField] TMP_Text skipText;

    void Awake()
    {
        HideImmediate();
    }

    void OnEnable()
    {
        MenuInputHandler.OnSkipHoldStarted += Show;
        MenuInputHandler.OnSkipHoldCanceled += Hide;
        MenuInputHandler.OnSkipHoldUpdate += SetProgress;
        MenuInputHandler.OnSkipConfirmed += Hide; // opcional
    }

    void OnDisable()
    {
        MenuInputHandler.OnSkipHoldStarted -= Show;
        MenuInputHandler.OnSkipHoldCanceled -= Hide;
        MenuInputHandler.OnSkipHoldUpdate -= SetProgress;
        MenuInputHandler.OnSkipConfirmed -= Hide;
    }

    public void Show()
    {
        group.alpha = 1f;
    }

    public void Hide()
    {
        group.alpha = 0f;
        SetProgress(0f);
    }

    void HideImmediate()
    {
        group.alpha = 0f;
        SetProgress(0f);
    }

    public void SetProgress(float value)
    {
        underlineFill.fillAmount = Mathf.Clamp01(value);
    }
}