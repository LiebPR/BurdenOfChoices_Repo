using UnityEngine;

public class PuzzleRoomTrigger : MonoBehaviour
{
    #region Inspector Variables
    [Header("Doors Control")]
    [SerializeField] Door[] doorsToLock;
    [SerializeField] Door[] secondaryDoors; //puertas secundarias, se desbloquean al completar puzzle.

    [Header("Puzzle Reward")]
    [SerializeField] GameObject keyPrefab;
    [SerializeField] Transform keyDropPoint;

    [Header("Puzzle de la sala")]
    [SerializeField] PuzzleObjective puzzle; //solo un puzzle por sala
    #endregion

    #region Internal States
    bool puzzleActive;
    bool puzzleCompleted;
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (puzzleActive || puzzleCompleted) return;

        ActivatePuzzleRoom();
    }

    #region Puzzle Flow
    void ActivatePuzzleRoom()
    {
        puzzleActive = true;

        for (int i = 0; i < doorsToLock.Length; i++)
        {
            doorsToLock[i].Lock();
        }

        // Suscribirse al puzzle
        if (!puzzle.IsCompleted())
            puzzle.OnPuzzleCompleted += HandlePuzzleCompleted;
    }

    void HandlePuzzleCompleted()
    {
        CompletePuzzle();
    }

    public void CompletePuzzle()
    {
        if (puzzleCompleted) return;

        puzzleCompleted = true;
        puzzleActive = false;

        //Desbloquea las puertas secundarias
        for (int i = 0; i < secondaryDoors.Length; i++)
        {
            secondaryDoors[i].Unlock();
        }

        DropKey();
    }

    void DropKey()
    {
        if (keyPrefab == null || keyDropPoint == null) return;

        Instantiate(keyPrefab, keyDropPoint.position, Quaternion.identity);
    }
    #endregion
}
