using UnityEngine;
using UnityEngine.SceneManagement;

public class StairsHandler : MonoBehaviour, IInteractable
{
    #region Inspector States
    [Header("References")]
    [SerializeField] Cell cell;

    [Header("Scene Config")]
    [SerializeField] string nextScene;
    #endregion

    #region IInteractable
    public void OnPress()
    {
        if(cell == null)
        {
            Debug.LogError("StairHandler: No Cell asignada.");
            return;
        }

        if (!cell.AreAllLocksUnlocked)
        {
            //Poner algo para el sistema de dialogos
            Debug.Log("Tendría que salvar al MOCOSO");
            return;
        }

        LoadNextScene();
    }

    public void OnRelease() { }
    public void OnHighlight() { }
    public void OnRemoveHighlight() { }
    #endregion

    #region Private API
    void LoadNextScene()
    {
        SceneManager.LoadScene(nextScene);
    }
    #endregion
}
