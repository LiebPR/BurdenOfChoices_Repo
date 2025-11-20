using UnityEngine;

/// <summary>
/// AnimatorManager: Se encaarga de gestionar todas la animaciones del jagaador.
/// Escucha los eventos del InputManager y actualiza el Animator según ele stado.
/// </summary>
public class AnimatorManager : MonoBehaviour
{
    #region References
    Animator animator;
    PlayerController playerController;
    #endregion

    #region Internal States
    bool lastCrouchState;
    int hashIsCrouching;
    #endregion

    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        hashIsCrouching = Animator.StringToHash("isCrouching");
        lastCrouchState = false;
    }

    private void OnEnable()
    {
        animator.SetBool(hashIsCrouching, false);
    }
    private void OnDisable()
    {
    }

    private void Update()
    {
        UpdateCrouchAnimation();
    }

    #region Crouch Animation Logic
    void UpdateCrouchAnimation()
    {
        if (animator == null) return;
        if (lastCrouchState == playerController.IsCrouching) return;

        animator.SetBool(hashIsCrouching, playerController.IsCrouching);
        lastCrouchState = playerController.IsCrouching;
    }
    #endregion
}
