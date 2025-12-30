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
    static readonly int VelocityHash = Animator.StringToHash("Velocity");
    static readonly int IsRelaxedHash = Animator.StringToHash("IsRelaxed");
    static readonly int IsCrouchingHash = Animator.StringToHash("IsCrouching");
    #endregion

    #region Internal States
    float velocity;
    bool isRelaxed;
    bool isCrouching;
    #endregion

    #region Public API
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

    public void SetRelaxed(bool value)
    {
        isRelaxed = value;
        ApplyState();
    }

    public void SetCrouching(bool value)
    {
        isCrouching = value;
        legsAnimator.SetBool(IsCrouchingHash, isCrouching);
        torsoAnimator.SetBool(IsCrouchingHash, isCrouching);
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
}
