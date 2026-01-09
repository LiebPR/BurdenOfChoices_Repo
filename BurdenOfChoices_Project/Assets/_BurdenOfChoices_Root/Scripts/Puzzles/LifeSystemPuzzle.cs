using System.Collections;
using UnityEngine;

public class LifeSystemPuzzle : MonoBehaviour
{
    #region Inspector States
    [Header("Lives")]
    [SerializeField] MonoBehaviour[] lifeFeedbacks;

    [Header("Cooldown")]
    [SerializeField] float loseLifeCooldown = 0.5f;

    [SerializeField] string loseSceneName = "SCN_Lose2Menu";
    #endregion

    #region Internal States
    ILifeFeedback[] lives;
    int currentLife;
    bool canLoseLife = true;
    #endregion

    private void Awake()
    {
        lives = new ILifeFeedback[lifeFeedbacks.Length];
        for (int i = 0; i < lifeFeedbacks.Length; i++)
            lives[i] = lifeFeedbacks[i] as ILifeFeedback;
    }

    public void LoseLife()
    {
        if (!canLoseLife) return;
        if (currentLife >= lives.Length)
            return;

        lives[currentLife].Consume();
        currentLife++;

        if (currentLife >= lives.Length)
            OnOutOfLives();

        StartCoroutine(LifeCooldown());
    }

    IEnumerator LifeCooldown()
    {
        canLoseLife = false;
        yield return new WaitForSeconds(loseLifeCooldown);
        canLoseLife = true;
    }

    void OnOutOfLives()
    {
        // Fase y resultado
        if (GameDirector.Instance != null)
        {
            GameDirector.Instance.SetOutcome(GameOutcome.NormalLose);
            GameDirector.Instance.SetPhase(GamePhase.Cutscene);
        }

        // Cambio de escena
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(loseSceneName);
        }
    }
}
