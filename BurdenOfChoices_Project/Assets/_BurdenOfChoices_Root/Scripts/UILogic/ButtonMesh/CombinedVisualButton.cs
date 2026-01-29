using UnityEngine;

public class CombinedVisualButton : MonoBehaviour, IButtonVisual
{
    [Header("Visuals")]
    [SerializeField] MaterialButtonVisual materialVisual;
    [SerializeField] AnimationButtonVisual animationVisual;

    public void SetNormal()
    {
        materialVisual?.SetNormal();
    }

    public void SetSelected()
    {
        materialVisual?.SetSelected();
    }

    public void SetDisabled()
    {
        materialVisual?.SetDisabled();
    }

    public void SetHover()
    {
        materialVisual?.SetHover();
    }

    public void OnHoverEnter()
    {
        // feedback puntual
        animationVisual?.OnHoverEnter();
        materialVisual?.OnHoverEnter();
    }
}
