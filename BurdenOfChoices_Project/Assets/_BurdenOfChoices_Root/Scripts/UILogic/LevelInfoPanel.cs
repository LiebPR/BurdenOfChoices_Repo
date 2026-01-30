using UnityEngine;

public class LevelInfoPanel : MonoBehaviour
{
    // Este método solo oculta el panel
    public void OnPlayButtonPressed()
    {
        gameObject.SetActive(false);
    }
}
