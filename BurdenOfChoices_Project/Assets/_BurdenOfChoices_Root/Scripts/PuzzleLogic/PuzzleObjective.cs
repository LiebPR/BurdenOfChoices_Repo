using UnityEngine;
using System;

public class PuzzleObjective : MonoBehaviour
{
    bool isCompleted;

    public event Action OnPuzzleCompleted;

    //Llamar cuando el puzzle se complete
    public void SetComplete()
    {
        if (isCompleted) return;

        isCompleted = true;
        OnPuzzleCompleted?.Invoke();
    }

    public bool IsCompleted()
    {
        return isCompleted;
    }
}
