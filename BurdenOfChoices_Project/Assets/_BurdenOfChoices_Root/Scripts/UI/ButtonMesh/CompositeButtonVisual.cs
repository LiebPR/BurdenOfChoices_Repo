using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour, IButtonVisual  
{
    IButtonVisual[] visuals;

    private void Awake()
    {
        visuals = GetComponentsInChildren<IButtonVisual>();
    }

    public void SetNormal() => ForEach(v => v.SetNormal());
    public void SetSelected() => ForEach(v => v.SetSelected());
    public void SetDisabled() => ForEach(v => v.SetDisabled());
    public void SetHover() => ForEach(v => v.SetHover());
    public void OnHoverEnter() => ForEach(v => v.SetHover());

    void ForEach(System.Action<IButtonVisual> action)
    {
        foreach (var v in visuals)
        {
            if (!ReferenceEquals(v, this))
                action(v);
        }
    }
}
