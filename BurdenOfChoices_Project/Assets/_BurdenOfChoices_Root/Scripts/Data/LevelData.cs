using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;          // Nombre visible en el panel
    public string sceneName;          // Nombre de la escena a cargar

    [Header("PeackUps")]
    public int maxPeackUps = 3;
    public int collectedPeackUps; //mejor resultado histórico
    public int sessionPeackUps; //resultado de la partida actual

    [Header("Remorse")]
    public float lastSessionRemorse; //porcentaje 0-1, siempre se actualiza

    [Header("Session Flags")]
    public bool lastSessionWasCaught;

    [Header("Menu Return")]
    public bool restoreMenuCameraOnReturn = true;

    public void ResetSession()
    {
        sessionPeackUps = 0;
    }

    public void RegisterSessionPeackUp()
    {
        sessionPeackUps++;
    }

    public void CommitSessionIfBetter()
    {
        if(sessionPeackUps > collectedPeackUps)
            collectedPeackUps = sessionPeackUps;
    }
}
