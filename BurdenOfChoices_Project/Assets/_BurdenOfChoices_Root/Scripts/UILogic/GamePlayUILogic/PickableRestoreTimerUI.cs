using TMPro;
using UnityEngine;

public class PickableRestoreTimerUI : MonoBehaviour
{
    #region Inspector States
    [Header("References")]
    [SerializeField] PickableBehaviour pickable;
    [SerializeField] TMP_Text timerText;

    [Header("Critical Feedback")]
    [SerializeField] Color criticalColor = Color.red;
    [SerializeField] float blinkSpeed = 6f;
    #endregion

    Color defaultColor;
    int lastDisplayedSecond = -1;

    void Awake()
    {
        if (timerText == null) return;

        defaultColor = timerText.color;
        timerText.enabled = false;
    }

    void Update()
    {
        if (pickable == null || timerText == null) return;

        float remainingTime = pickable.GetRestoreRemainingTime();
        bool isRestoring = pickable.IsRestoreRunning;

        // Apagar el contador cuando ya no se esté restaurando
        if (!isRestoring || remainingTime <= 0f)
        {
            timerText.enabled = false;
            timerText.color = defaultColor;
            lastDisplayedSecond = -1;
            return;
        }

        timerText.enabled = true;

        // Mostrar segundos exactos hasta 0
        int seconds = Mathf.CeilToInt(remainingTime);
        if (seconds != lastDisplayedSecond)
        {
            timerText.text = seconds.ToString();
            lastDisplayedSecond = seconds;
        }

        // Parpadeo crítico
        if (seconds <= 3)
        {
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color c = criticalColor;
            c.a = alpha;
            timerText.color = c;
        }
        else
        {
            timerText.color = defaultColor;
        }
    }
}
