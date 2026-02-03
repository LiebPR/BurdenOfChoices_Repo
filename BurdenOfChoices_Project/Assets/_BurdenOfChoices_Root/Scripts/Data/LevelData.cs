using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Game/LevelData")]
public class LevelData : ScriptableObject
{
    public string levelName;          // Nombre visible en el panel
    public string sceneName;          // Nombre de la escena a cargar
}
