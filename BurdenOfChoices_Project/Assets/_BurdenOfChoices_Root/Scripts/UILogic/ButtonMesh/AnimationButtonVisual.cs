using UnityEngine;

public class AnimationButtonVisual : MonoBehaviour
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
        //Hover persistente (en caso de que se necesite)
    }

    public void OnHoverEnter()
    {
        animator1.SetTrigger(Hover);
        animator2.SetTrigger(Hover);
    }
}
