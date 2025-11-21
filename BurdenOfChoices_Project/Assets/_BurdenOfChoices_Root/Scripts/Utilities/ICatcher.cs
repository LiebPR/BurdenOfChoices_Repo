using UnityEngine;

/// <summary>
/// ICatcher: Interfaz que define un objeto capaz de atrapar Pickables.
/// </summary>
public interface ICatcher
{
    Transform GetCatchPoint();
}
