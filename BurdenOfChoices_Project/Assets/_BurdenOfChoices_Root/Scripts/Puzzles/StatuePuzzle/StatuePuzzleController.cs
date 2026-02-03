using UnityEngine;

public class StatuePuzzleController : MonoBehaviour
{
    [Header("Estatua del Puzzle")]
    [SerializeField] Statue[] orderedStatues;

    [Header("Pilares del puzzle")]
    [SerializeField] Pillar[] pillars;

    [Header("Sala puzzle")]
    [SerializeField] PuzzleRoomTrigger puzzleRoom;

    [Header("Sistema de Vidas")]
    [SerializeField] LifeSystemPuzzle lifeSystemPuzzle;

    int currentIndex;
    bool isComplete = false;

    private void OnEnable()
    {
        foreach(var statue in orderedStatues)
        {
            statue.OnFallen += HandleStatueFallen;
        }
    }

    private void OnDisable()
    {
        foreach (var statue in orderedStatues)
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

        //Estatua incorrecta -> reset completo
        if (orderedStatues[currentIndex] != fallenStatue)
        {
            ResetPuzzle();
            return;
        }

        currentIndex++;

        if(currentIndex >= orderedStatues.Length)
            CompletePuzzle();
    }

    void ResetPuzzle()
    {
        currentIndex = 0;

        foreach (var statue in orderedStatues)
            statue.ResetStatue();

        foreach (var pillar in pillars)
            pillar.ResetPillar();

        OnWrong();
    }

    public void OnWrong()
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
