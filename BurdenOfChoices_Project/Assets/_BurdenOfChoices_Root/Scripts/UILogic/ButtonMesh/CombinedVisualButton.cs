using UnityEngine;

public class CombinedVisualButton : MonoBehaviour, IButtonVisual
{
    [Header("Visuals")]
    [SerializeField] MaterialMesh materialVisual;
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
        //Hover persistente por material
        materialVisual?.SetHover();
    }

    public void OnHoverEnter()
    {
        //Flash animado
        animationVisual?.OnHoverEnter();

        //Hover normal
        materialVisual?.SetHover();
    }
}
