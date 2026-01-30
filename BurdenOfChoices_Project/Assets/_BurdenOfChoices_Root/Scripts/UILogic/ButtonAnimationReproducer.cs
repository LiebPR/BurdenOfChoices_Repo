using UnityEngine;

public class ButtonAnimationReproducer : MonoBehaviour
{
    [SerializeField] Animator animator1;
    [SerializeField] Animator animator2;

    static readonly int ButtonPressHash = Animator.StringToHash("OpenDoor");

    public void OnButtonPressAnimStart()
    {
        animator1.SetTrigger(ButtonPressHash);
        animator2.SetTrigger(ButtonPressHash);
    }
}
