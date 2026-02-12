using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class UIButtonFocusFeedback : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Arrow References")]
    [SerializeField] Image arrow;
    [SerializeField] Vector2 arrowOffset = new Vector2(-40f, 0f);

    [Header("Arrow Idle Animation")]
    [SerializeField] float animationDelay = 0.8f;
    [SerializeField] float animationDistance = 10f;
    [SerializeField] float animationSpeed = 2f;

    [Header("Press Animation")]
    [SerializeField] float pressDuration = 0.2f;
    [SerializeField] float arrowPressDistance = 15f;
    [SerializeField] float textPressScale = 0.8f;

    [Header("Text Reference")]
    [SerializeField] RectTransform buttonText;

    [Header("Button Reference")]
    [SerializeField] Button button;

    [Header("Audio")]
    [SerializeField] string highlightSFXID; //ID del sonido en AudioManager
    [SerializeField] float volumen = 0.8f;

    [Header("Mode")]
    [Tooltip("Si está activo, solo hace la animación de presionar, no invoca Button.onClick")]
    [SerializeField] bool disableClick = false;

    RectTransform arrowRect;
    bool isHovered;
    Coroutine arrowCoroutine;
    bool isPressed;
    bool hasPlayedHighlight = false;

    void Awake()
    {
        if (arrow != null) arrowRect = arrow.rectTransform;
        if (button == null) button = GetComponent<Button>();
    }

    void OnDisable() => StopArrowAnimation();

    public void OnSelect(BaseEventData eventData) => TryShowArrow();

    public void OnDeselect(BaseEventData eventData)
    {
        StopArrowAnimation();
        hasPlayedHighlight = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        TryShowArrow();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        hasPlayedHighlight = false;

        if (UIInputModeManager.Instance.CurrentMode == UIInputModeManager.InputMode.Mouse)
            StopArrowAnimation();
    }

    void TryShowArrow()
    {
        if (arrowRect == null) return;

        var mode = UIInputModeManager.Instance.CurrentMode;

        if (mode == UIInputModeManager.InputMode.Navigation || isHovered)
        {
            arrow.gameObject.SetActive(true);
            arrowRect.anchoredPosition = arrowOffset;

            if (!hasPlayedHighlight && !string.IsNullOrEmpty(highlightSFXID))
            {
                AudioManager.Instance.PlaySFX2D(highlightSFXID, volumen);
                hasPlayedHighlight = true;
            }

            if (arrowCoroutine != null)
                StopCoroutine(arrowCoroutine);
            arrowCoroutine = StartCoroutine(ArrowIdleAnimation());
        }
        else
        {
            hasPlayedHighlight = false; // Resetear cuando se pierde foco
        }
    }

    void StopArrowAnimation()
    {
        if (arrowRect == null) return;

        if (arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
            arrowCoroutine = null;
        }

        arrowRect.anchoredPosition = arrowOffset;
        arrow.gameObject.SetActive(false);
    }

    IEnumerator ArrowIdleAnimation()
    {
        yield return new WaitForSeconds(animationDelay);
        Vector2 startPos = arrowOffset;
        Vector2 endPos = startPos + new Vector2(animationDistance, 0f);

        while (true)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * animationSpeed;
                arrowRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
                yield return null;
            }

            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * animationSpeed;
                arrowRect.anchoredPosition = Vector2.Lerp(endPos, startPos, t);
                yield return null;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) => PressStart();
    public void OnPointerUp(PointerEventData eventData) => PressEnd();

    public void PressStart()
    {
        if (isPressed) return;
        isPressed = true;

        if (arrowCoroutine != null)
        {
            StopCoroutine(arrowCoroutine);
            arrowCoroutine = null;
        }

        UIInputModeManager.Instance.ForceNavigationMode();
        StartCoroutine(PressAnimationWithDelay());
    }

    IEnumerator PressAnimationWithDelay()
    {
        Vector2 arrowStart = arrowRect.anchoredPosition;
        Vector2 arrowEnd = arrowOffset + new Vector2(arrowPressDistance, 0f);

        Vector3 textStart = buttonText.localScale;
        Vector3 textEnd = Vector3.one * textPressScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / pressDuration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            arrowRect.anchoredPosition = Vector2.Lerp(arrowStart, arrowEnd, smoothT);
            buttonText.localScale = Vector3.Lerp(textStart, textEnd, smoothT);

            yield return null;
        }

        // Pequeño retraso para que se aprecie la animación
        yield return new WaitForSeconds(0.05f);

        // Invocar Button.onClick solo si disableClick es false
        if (!disableClick && button != null)
            button.onClick.Invoke();

        // Volver flecha y texto a estado original
        arrowRect.anchoredPosition = arrowOffset;
        buttonText.localScale = Vector3.one;

        if (UIInputModeManager.Instance.CurrentMode == UIInputModeManager.InputMode.Navigation)
            arrowCoroutine = StartCoroutine(ArrowIdleAnimation());

        isPressed = false;
    }

    public void PressEnd()
    {
        if (!isPressed) return;
        isPressed = false;

        // NO invocamos Button.onClick aquí
        if (UIInputModeManager.Instance.CurrentMode == UIInputModeManager.InputMode.Navigation)
            arrowCoroutine = StartCoroutine(ArrowIdleAnimation());
    }
}
