using UnityEngine;

public class ButtonAnimationReproducer : MonoBehaviour
{
    [SerializeField] Animator animator1;
    [SerializeField] Animator animator2;
    [SerializeField] string audioAnimationSFX = "SFX_BigDoor_Open";
    [SerializeField] float volumen = 1f;

    static readonly int ButtonPressHash = Animator.StringToHash("OpenDoor");

    public void OnButtonPressAnimStart()
    {
        animator1.SetTrigger(ButtonPressHash);
        animator2.SetTrigger(ButtonPressHash);
        AudioManager.Instance.PlaySFX2D(audioAnimationSFX, volumen);
    }
}
