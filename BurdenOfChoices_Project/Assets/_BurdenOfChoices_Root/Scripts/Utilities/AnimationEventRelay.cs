using UnityEngine;


public class AnimationEventRelay : MonoBehaviour
{
    [Header("Target References")]
    [SerializeField] AnimatorManager animatorManager;
    [SerializeField] PlayerController playerController;

    #region Animation Events
    public void OnAttackStart()
    {
        if (playerController != null)
            playerController.PausePlayer();
    }
    
    /// <summary>
    /// Llamar al final de un ataque
    /// </summary>
    public void OnAttackEnd()
    {
        if (playerController != null)
            playerController.ResumePlayer(); // reanuda el player
    }

    /// <summary>
    /// Llamar al inicio de la recogida de un objeto
    /// </summary>
    public void OnPickStart()
    {
        if (animatorManager != null)
            animatorManager.SetPicking(true);
    }
    #endregion
}
