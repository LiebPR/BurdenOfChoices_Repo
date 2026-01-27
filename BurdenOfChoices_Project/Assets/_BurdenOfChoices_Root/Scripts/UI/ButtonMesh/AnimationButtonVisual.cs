using UnityEngine;

public class AnimationButtonVisual : MonoBehaviour, IButtonVisual
{
    [SerializeField] Animator animator1;
    [SerializeField] Animator animator2;

    static readonly int Hover = Animator.StringToHash("Highlight");


    public void SetNormal()
    {

    }


    public void SetSelected()
    {

    }

    public void SetDisabled()
    {

    }

    public void SetHover()
    {
    }

    public void OnHoverEnter()
    {
        animator1.SetTrigger(Hover);
        animator2.SetTrigger(Hover);
    }
}
