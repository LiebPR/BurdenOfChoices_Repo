public interface IButtonVisual
{
    void SetNormal();
    void SetHover(); //estado de Hover persistente
    void OnHoverEnter(); //efecto puntual (flash / trigger)
    void SetSelected();
    void SetDisabled();
}