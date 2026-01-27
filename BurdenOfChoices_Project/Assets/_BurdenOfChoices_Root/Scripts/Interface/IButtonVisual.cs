public interface IButtonVisual
{
    void SetNormal();
    void SetHover();          // hover continuo (opcional)
    void OnHoverEnter();      // ?? nuevo
    void SetSelected();
    void SetDisabled();
}