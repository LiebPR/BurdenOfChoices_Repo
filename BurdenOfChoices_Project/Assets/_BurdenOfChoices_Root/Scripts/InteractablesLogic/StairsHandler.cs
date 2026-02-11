using UnityEngine;
using UnityEngine.SceneManagement;

public class StairsHandler : MonoBehaviour
{
    #region Inspector States
    [Header("References")]
    [SerializeField] Cell cell;

    [Header("Scene Config")]
    [SerializeField] string nextScene;

    [Header("Level Data")]
    [SerializeField] LevelData currentLevelData;
    [SerializeField] Remorse remorse;
    [SerializeField] PlayerHealth playerHealth;
    #endregion

    void OnTriggerEnter(Collider other)
    {
        // Solo el jugador puede activar la escalera
        if (!other.TryGetComponent<PlayerController>(out _))
            return;

        if (cell == null)
        {
            Debug.LogError("StairsHandler: No Cell asignada.");
            return;
        }

        if (!cell.AreAllLocksUnlocked)
        {
            // Aquí irá el sistema de diálogos
            return;
        }

        LoadNextScene();
    }

    #region Private
    void LoadNextScene()
    {
        if (SceneController.Instance != null)
        {
            //Consolidar porgreso
            currentLevelData.CommitSessionIfBetter();

            //Remorse (siempre se actualiza)
            if(remorse != null)
                currentLevelData.lastSessionRemorse = remorse.RemorsePercentage;

            //Caought (siempre)
            if(playerHealth != null)
                currentLevelData.lastSessionWasCaught = playerHealth.WasCaughtThisRun;

            //Contexto de retorno al Menú
            GameFlowContext.ReturnFromLevel = true;
            GameFlowContext.LastPlayedLevel = currentLevelData;

            SceneController.Instance.LoadScene(nextScene);
        }
        else
        {
            Debug.LogError("StairsHandler: SceneController no encontrado.");
        }
    }
    #endregion
}
