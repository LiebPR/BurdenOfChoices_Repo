using UnityEngine;

public class StatuePuzzleController : MonoBehaviour
{
    [Header("Estatua del Puzzle")]
    [SerializeField] Statue[] statues;

    [Header("Sala puzzle")]
    [SerializeField] PuzzleRoomTrigger puzzleRoom;

    [Header("Sistema de Vidas")]
    [SerializeField] LifeSystemPuzzle lifeSystemPuzzle;

    bool isComplete = false;

    private void OnEnable()
    {
        foreach(var statue in statues)
        {
            statue.OnFallen += HandleStatueFallen;
        }
    }

    private void OnDisable()
    {
        foreach (var statue in statues)
        {
            statue.OnFallen -= HandleStatueFallen;
        }
    }

    /// <summary>
    /// Llamado cuando una estatua del puzzle cae
    /// </summary>
    void HandleStatueFallen(Statue fallenStatue)
    {
        if(isComplete) return;

        //Verifica si todas las estatuas han caído
        foreach(var s in statues)
        {
            if(!s.IsFallen()) return;
        }

        CompletePuzzle();
    }

    public void OnWrongHit()
    {
        if(isComplete) return;

        if (lifeSystemPuzzle != null)
            lifeSystemPuzzle.LoseLife();
    }

    void CompletePuzzle()
    {
        isComplete = true;

        if (puzzleRoom != null)
            puzzleRoom.CompletePuzzle();
    }
}
