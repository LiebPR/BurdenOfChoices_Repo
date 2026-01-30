using UnityEngine;

public class EnemyPerceptionFeedback : MonoBehaviour
{
    #region Inspector
    [Header("Vignette")]
    [SerializeField] SpriteRenderer vignetteBackground;
    [SerializeField] SpriteRenderer vignette;

    [Header("Vignette Colors")]
    [SerializeField] Color suspiciousColor = Color.yellow;
    [SerializeField] Color chaseColor = Color.red;
    [SerializeField] Color stunColor = Color.gray;

    [Header("Vignette Symbols")]
    [SerializeField] SpriteRenderer suspiciousSymbol; // ?
    [SerializeField] SpriteRenderer chaseSymbol; // !
    [SerializeField] SpriteRenderer stunSymbol; // Stun

    [Header("Enemy Data")]
    [SerializeField] EnemyData enemyData;
    #endregion

    #region Internal States
    SpriteRenderer currentSymbol;
    bool vignetteActive;

    // Transición de color
    float colorTimer = 0f;
    float colorDuration = 0.5f;
    Color startColor;
    Color targetColor;
    bool isTransitioning = false;

    // Transición de pérdida (rojo → amarillo)
    float lostColorTimer = 0f;
    float lostColorDuration = 1f;
    bool isLostTransitioning = false;
    #endregion

    #region References
    EnemyFSM fsm;
    VisionSystem vision;
    StunState stun;
    #endregion

    private void Awake()
    {
        fsm = GetComponent<EnemyFSM>();
        vision = GetComponent<VisionSystem>();
        stun = GetComponent<StunState>();
    }

    private void Update()
    {
        UpdateFeedback();
        UpdateColorTransition();
    }

    #region Feedback
    void UpdateFeedback()
    {
        // STUN - Corte total
        if (stun != null && stun.IsStunned)
        {
            ApplyFeedback(stunSymbol, stunColor, true);
            ResetTransitions();
            return;
        }

        // CHASE - visión directa
        if (vision != null && vision.CanSeeTarget())
        {
            ApplyFeedback(chaseSymbol, chaseColor);

            if (!isTransitioning)
            {
                startColor = vignetteBackground.color;
                targetColor = chaseColor;
                colorDuration = enemyData.visionDelay;
                colorTimer = 0f;
                isTransitioning = true;
            }

            isLostTransitioning = false;
            return;
        }

        // SUSPICIOUS - percepción sin visión
        if (fsm.CurrentState == EnemyState.InvestigateSound)
        {
            ApplyFeedback(suspiciousSymbol, suspiciousColor);

            if (!isLostTransitioning)
            {
                startColor = vignetteBackground.color;
                targetColor = suspiciousColor;
                lostColorDuration = enemyData.lostDelay;
                lostColorTimer = 0f;
                isLostTransitioning = true;
            }

            isTransitioning = false;
            return;
        }

        // Sin percepción → apagar todo
        ClearFeedback();
        ResetTransitions();
    }

    void ApplyFeedback(SpriteRenderer newSymbol, Color color, bool forceReset = false)
    {
        // Cambiar símbolo si es diferente
        if (currentSymbol != newSymbol || forceReset)
        {
            DisableAllSymbols();
            newSymbol.enabled = true;
            currentSymbol = newSymbol;
        }

        // Activar viñeta si estaba apagada
        if (!vignetteActive || forceReset)
        {
            vignetteBackground.enabled = true;
            vignette.enabled = true;
            vignetteActive = true;

            vignetteBackground.color = color;
            startColor = color;
            targetColor = color;
            isTransitioning = false;
            isLostTransitioning = false;
        }
    }
    #endregion

    #region Public API
    public void ClearFeedback()
    {
        if (!vignetteActive) return;

        vignetteBackground.enabled = false;
        vignette.enabled = false;
        DisableAllSymbols();

        vignetteActive = false;
        currentSymbol = null;
    }
    #endregion

    #region Color Transition
    void UpdateColorTransition()
    {
        // Amarillo → Rojo
        if (isTransitioning)
        {
            colorTimer += Time.deltaTime;
            float t = Mathf.Clamp01(colorTimer / colorDuration);
            vignetteBackground.color = Color.Lerp(startColor, targetColor, t);

            if (t >= 1f)
                isTransitioning = false;
        }

        // Rojo → Amarillo (pérdida de visión)
        if (isLostTransitioning)
        {
            lostColorTimer += Time.deltaTime;
            float t = Mathf.Clamp01(lostColorTimer / lostColorDuration);
            vignetteBackground.color = Color.Lerp(startColor, targetColor, t);

            if (t >= 1f)
                isLostTransitioning = false;
        }
    }

    void ResetTransitions()
    {
        isTransitioning = false;
        isLostTransitioning = false;
        colorTimer = 0f;
        lostColorTimer = 0f;
    }
    #endregion

    #region Helpers
    void DisableAllSymbols()
    {
        suspiciousSymbol.enabled = false;
        chaseSymbol.enabled = false;
        stunSymbol.enabled = false;
    }
    #endregion
}
