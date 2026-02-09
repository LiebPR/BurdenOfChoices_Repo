using UnityEngine;

public class AnimationButtonVisual : MonoBehaviour
{
    [SerializeField] Animator animator1;
    [SerializeField] Animator animator2;
    [SerializeField] string highlightAnimSFX = "SFX_Door_Locked";
    [SerializeField] float volumen = 0.5f;

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

        AudioManager.Instance.PlaySFX2D(highlightAnimSFX, volumen);
    }
}
