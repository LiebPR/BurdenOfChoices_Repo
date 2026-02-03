using UnityEngine;


public class AnimationEventRelay : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] AnimatorManager animatorManager;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerHealth health;
    [SerializeField] PlayerThrowController throwController;

    #region Animation Events
    public void OnAttackStart()
    {
        if (playerController != null)
            playerController.PausePlayer();
        if(playerController != null)
            playerController.LockRotation();
    }
    
    /// <summary>
    /// Llamar al final de un ataque
    /// </summary>
    public void OnAttackEnd()
    {
        if (playerController != null)
            playerController.ResumePlayer(); // reanuda el player
        if (playerController != null)
            playerController.UnlockRotation();
    }

    /// <summary>
    /// Llamar al inicio de la recogida de un objeto
    /// </summary>
    public void OnPickStart()
    {
        if (animatorManager != null)
            animatorManager.SetGrabbing(true);
    }
    #endregion

    #region Death
    public void OnDeathAnimationEnd()
    {
        if (health != null)
            health.OnDeathAnimationFinished();
    }
    #endregion

}
