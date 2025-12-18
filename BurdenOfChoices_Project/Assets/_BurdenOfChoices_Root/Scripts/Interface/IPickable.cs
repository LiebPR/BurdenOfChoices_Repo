using UnityEngine;

public interface IPickListener
{
    void OnPick(ICatcher catcher);
    void OnDrop();
}
