using UnityEngine;

public class PeackUpHandler : MonoBehaviour
{
    [SerializeField] LevelData levelData;

    private void Awake()
    {
        levelData.ResetSession();
    }

    public void RegisterPeackUp()
    {
        if (levelData.sessionPeackUps >= levelData.maxPeackUps)
            return;

        levelData.RegisterSessionPeackUp();
    }

    void OnDisable()
    {
        levelData.CommitSessionIfBetter();
    }
}
