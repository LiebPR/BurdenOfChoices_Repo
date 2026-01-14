using UnityEngine;

/// <summary>
/// AnimatorManager
/// Gestiona todos los estados de animación
/// usando un único Animator con capas.
/// </summary>
public class AnimatorManager : MonoBehaviour
{
    #region Inspector
    [SerializeField] Animator smAnimator;
    [SerializeField] Animator catchPointAnimator;
    [SerializeField] Animator colliderAnimator;

    [Header("Animations Speed")]
    [SerializeField] float walkBaseFrameRate = 35f;
    #endregion

    #region Animator Hashes
    static readonly int VelocityHash = Animator.StringToHash("Velocity");
    static readonly int IsRelaxedHash = Animator.StringToHash("IsRelaxed");
    static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    static readonly int IsAttackHash = Animator.StringToHash("IsAttack");
    static readonly int IsSlashingHash = Animator.StringToHash("IsSlashing");
    static readonly int IsPickingHash = Animator.StringToHash("IsPicking");
    static readonly int IsThrowingHash = Animator.StringToHash("IsThrowing");
    static readonly int ThrowHash = Animator.StringToHash("Throw");
    static readonly int IsDeathHash = Animator.StringToHash("IsDeath");
    #endregion

    #region State
    float velocity;
    bool isCrouching;
    float isRelaxed;
    float currentWalkFrameRate; // frame rate actual suavizado
    float walkFrameRateVelocity; // helper para smoothDamp
    #endregion

    #region Reference
    PlayerController playerController;
    PlayerHealth health;
    #endregion

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        health = GetComponent<PlayerHealth>();
    }

    void OnEnable()
    {
        PickableBehaviour.OnEquipped += HandlePick;
        PickableBehaviour.OnDropped += HandleDrop;
    }

    void OnDisable()
    {
        PickableBehaviour.OnEquipped -= HandlePick;
        PickableBehaviour.OnDropped -= HandleDrop;
    }

    #region Public API
    public void SetVelocity(float value)
    {
        velocity = value;
        smAnimator.SetFloat(VelocityHash, velocity, 0.05f, Time.deltaTime);
    }

    public void SetMovementRatio(float ratio)
    {
        UpdateAnimSpeed(ratio);
    }

    public void SetCrouching(bool value)
    {
        isCrouching = value;
        smAnimator.SetBool(IsCrouchingHash, value);
        catchPointAnimator.SetBool(IsCrouchingHash, value);
        colliderAnimator.SetBool(IsCrouchingHash, value);
    }

    public void SetRelaxed(float value)
    {
        isRelaxed = Mathf.Clamp01(value);
        smAnimator.SetFloat(IsRelaxedHash, isRelaxed);
    }

    public void PlayAttack(float slashingValue)
    {
        smAnimator.SetFloat(IsSlashingHash, Mathf.Clamp01(slashingValue));
        smAnimator.SetTrigger(IsAttackHash);
    }

    public void SetGrabbing(bool value)
    {
        smAnimator.SetBool(IsPickingHash, value);
    }

    public void StartHold()
    {
        smAnimator.SetBool(IsThrowingHash, true);
    }

    public void EndHold()
    {
        smAnimator.SetBool(IsThrowingHash, false);
    }

    public void TriggerThrow()
    {
        smAnimator.SetTrigger(ThrowHash);
    }

    public void DeathAnim()
    {
        smAnimator.SetTrigger(IsDeathHash);
    }
    #endregion

    #region Core
    void UpdateAnimSpeed(float ratio)
    {
        // Solo aplicamos frame rate variable si estamos caminando
        if (velocity < 0.1f)
        {
            // Idle o detenido ? animación normal
            smAnimator.speed = 1f;
            currentWalkFrameRate = walkBaseFrameRate;
            return;
        }

        // Determinar frame rate objetivo según velocidad y si corre o está agachado
        float maxMultiplier = isCrouching ? 1.2f : 1.5f;
        float targetFrameRate = Mathf.Clamp(ratio * walkBaseFrameRate, walkBaseFrameRate * 0.8f, walkBaseFrameRate * maxMultiplier);

        // Suavizado para transición fluida
        currentWalkFrameRate = Mathf.SmoothDamp(currentWalkFrameRate, targetFrameRate, ref walkFrameRateVelocity, 0.1f);

        // Aplicamos solo a Walk
        smAnimator.speed = currentWalkFrameRate / walkBaseFrameRate;
    }
    #endregion

    #region Handles
    void HandlePick(PickableBehaviour p)
    {
        SetGrabbing(true);
    }

    void HandleDrop(PickableBehaviour p)
    {
        SetGrabbing(false);
    }
    #endregion
}
