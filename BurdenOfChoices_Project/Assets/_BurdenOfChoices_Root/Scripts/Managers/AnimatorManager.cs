using UnityEngine;

/// <summary>
/// AnimatorManager
/// Gestiona los estados de animación compartidos
/// entre piernas y torso.
/// </summary>
public class AnimatorManager : MonoBehaviour
{
    #region Inspector Variables
    [Header("Animators")]
    [SerializeField] Animator legsAnimator;
    [SerializeField] Animator torsoAnimator;
    #endregion

    #region Animator Hashes
    // Solo los lee 1 vez. 
    //PLAYER CONTROLLER
    static readonly int VelocityHash = Animator.StringToHash("Velocity");
    static readonly int IsRelaxedHash = Animator.StringToHash("IsRelaxed");
    static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");

    //ATTACK SYSTEM
    static readonly int IsAttackHash = Animator.StringToHash("IsAttack");
    static readonly int IsSlashingHash = Animator.StringToHash("IsSlashing");

    //PICK / THROW
    static readonly int IsPickingHash = Animator.StringToHash("IsPicking");
    static readonly int IsThrowingHash = Animator.StringToHash("IsThrowing");
    #endregion

    #region Internal States
    float velocity;
    bool isRelaxed;
    bool isCrouching;
    #endregion

    #region Reference
    PlayerController playerController;
    #endregion

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        PickableBehaviour.OnEquipped += HandlePick;
        PickableBehaviour.OnDropped += HandleDrop;
    }

    private void OnDisable()
    {
        PickableBehaviour.OnEquipped -= HandlePick;
        PickableBehaviour.OnDropped -= HandleDrop;
    }

    #region Public API
    //PLAYER CONTROLLER
    public void SetVelocity(float value)
    {
        velocity = value;

        legsAnimator.SetFloat(VelocityHash, velocity);
        torsoAnimator.SetFloat(VelocityHash, velocity);
    }

    public void SetMovementRatio(float ratio)
    {
        UpdateLegsAnimSpeed(ratio);
    }

    public void SetCrouching(bool value)
    {
        isCrouching = value;
        legsAnimator.SetBool(IsCrouchingHash, isCrouching);
        torsoAnimator.SetBool(IsCrouchingHash, isCrouching);
    }

    //ROOM TRIGGER
    public void SetRelaxed(bool value)
    {
        isRelaxed = value;
        ApplyState();
    }

    //ATTACK SYSTEM
    public void PlayAttack(WeaponAttackType type)
    {
        bool slashing = type == WeaponAttackType.Slash;

        torsoAnimator.SetBool(IsSlashingHash, slashing);

        torsoAnimator.SetTrigger(IsAttackHash);
    }

    //PICK / THROW
    public void SetPicking(bool value)
    {
        torsoAnimator.SetBool(IsPickingHash, value);
    }

    public void SetThrowing(bool value)
    {
        torsoAnimator.SetBool(IsThrowingHash, value);
    }
    #endregion

    #region Core
    void ApplyState()
    {
        legsAnimator.SetBool(IsRelaxedHash, isRelaxed);

        torsoAnimator.SetBool(IsRelaxedHash, isRelaxed);
    }
    #endregion

    #region Upd Anims
    void UpdateLegsAnimSpeed(float ratio)
    {
        if (velocity < 0.1f)
        {
            legsAnimator.speed = 1f;
            torsoAnimator.speed = 1f;
            return;
        }

        float maxSpeed = isCrouching ? 1.2f : 1.5f;
        float animSpeed = Mathf.Clamp(ratio, 0.8f, maxSpeed);

        legsAnimator.speed = animSpeed;
        torsoAnimator.speed = animSpeed;
    }
    #endregion

    #region Animation Events
    public void OnAttackStart()
    {
        playerController.PausePlayer();
    }

    public void OnAttackEnd()
    {
        playerController.ResumePlayer();
    }
    #endregion

    #region Handles
    void HandlePick(PickableBehaviour p)
    {
        SetPicking(true);
    }

    void HandleDrop(PickableBehaviour p)
    {
        SetPicking(false);
    }
    #endregion
}
