using UnityEngine;

public class PeackUp : MonoBehaviour, IInteractable
{
    [SerializeField] GameObject peackUpVFX;
    [SerializeField] Animator peackUpAnim;
    [SerializeField] PeackUpHandler peackUpHandler;

    #region Animator Parameters
    static readonly int OnCollectHash = Animator.StringToHash("Collect");
    #endregion

    #region Interaction Contract
    public void OnPress()
    {
        peackUpAnim.SetBool(OnCollectHash, true);
    }

    public void OnRelease()
    {
        peackUpHandler.RegisterPeackUp();
        Destroy(peackUpVFX);
        Destroy(gameObject);
    }

    public void OnHighlight(){}

    public void OnRemoveHighlight(){}
    #endregion
}
