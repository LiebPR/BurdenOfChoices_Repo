using UnityEngine;

public class GameFlowContext : MonoBehaviour
{
    public static bool ReturnFromLevel;
    public static LevelData LastPlayedLevel;

    public static void Clear()
    {
        ReturnFromLevel = false;
        LastPlayedLevel = null;
    }
}
