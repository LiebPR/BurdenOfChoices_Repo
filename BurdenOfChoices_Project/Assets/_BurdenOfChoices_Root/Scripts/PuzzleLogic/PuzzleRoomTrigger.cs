using Unity.Cinemachine;
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

    [Header("Puzzle Respawn Player")]
    [SerializeField] Transform puzzleRespawnPoint;
    [SerializeField] CinemachineCamera puzzleRespawnCamera;

    [Header("VFX Puzzle Start")]
    [SerializeField] GameObject puzzleStartVFXPrefab; // Prefab del VFX
    [SerializeField] Transform puzzleVFXSpawnPoint;   // Punto donde se instanciará
    #endregion

    #region Internal States
    bool puzzleActive;
    bool puzzleCompleted;
    bool isPlaying;
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

        // Instanciar VFX al iniciar el puzzle
        if (puzzleStartVFXPrefab != null && puzzleVFXSpawnPoint != null)
        {
            Instantiate(puzzleStartVFXPrefab, puzzleVFXSpawnPoint.position, puzzleVFXSpawnPoint.rotation);
        }

        var cameraShake = FindAnyObjectByType<CameraShackePuzzle>();
        if (cameraShake != null)
            cameraShake.TriggerShake(2f, 0.05f);

        for (int i = 0; i < doorsToLock.Length; i++)
            doorsToLock[i].Lock();


        var playerHealth = FindAnyObjectByType<PlayerHealth>();
        if(playerHealth != null && puzzleRespawnPoint != null)
            playerHealth.SetOverrideRespawnPoint(puzzleRespawnPoint, puzzleRespawnCamera);

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
       
        var playerHealth = FindAnyObjectByType<PlayerHealth>();
        playerHealth.ClearOverrideRespawn();

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
