using UnityEngine;

public class PeackUp : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject peackUpVFX;
    [SerializeField] Animator peackUpAnim;
    [SerializeField] PeackUpHandler peackUpHandler;

    AudioEmitter idleEmitter;
    #region Animator Parameters
    static readonly int OnCollectHash = Animator.StringToHash("Collect");
    #endregion

    #region Interaction Contract
    public void OnPress()
    {
        peackUpAnim.SetBool(OnCollectHash, true);
        AudioManager.Instance.PlaySFX2D("SFX_PickUp_Pick");
    }

    public void OnRelease()
    {
        peackUpHandler.RegisterPeackUp();
        Destroy(peackUpVFX);
        Destroy(gameObject);
        if (idleEmitter != null)
            idleEmitter.Stop(0.1f);
    }

    public void OnHighlight(){}

    public void OnRemoveHighlight(){}
    #endregion
}
