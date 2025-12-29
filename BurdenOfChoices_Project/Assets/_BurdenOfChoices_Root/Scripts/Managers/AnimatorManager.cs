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

    #region Internal States
    bool isRelaxed;
    #endregion

    #region Public API
    public void SetRelaxed(bool value)
    {
        isRelaxed = value;
        ApplyState();
    }
    #endregion

    #region Core
    void ApplyState()
    {
        legsAnimator.SetBool("IsRelaxed", isRelaxed);

        torsoAnimator.SetBool("IsRelaxed", isRelaxed);
    }
    #endregion
}
