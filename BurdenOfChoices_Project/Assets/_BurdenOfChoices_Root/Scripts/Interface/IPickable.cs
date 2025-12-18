using UnityEngine;

public class IPickable
{
    void Pick(ICatcher catcher) { }
    void Drop() { }
    bool IsPicked { get; }
}
