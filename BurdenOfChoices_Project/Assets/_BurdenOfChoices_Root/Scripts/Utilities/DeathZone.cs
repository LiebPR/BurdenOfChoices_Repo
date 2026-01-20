using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] string loseSceneMenu = "SCN_FallLoseMenu";

    private void OnTriggerEnter(Collider other)
    {
        //Solo el jugador activa la zona de muerte
        if(other.TryGetComponent<PlayerController>(out _))
        {
            HandlePlayerFallenDeath();
            return;
        }

        //Destruccion del enemigo
        if (other.TryGetComponent<EnemyFSM>(out var enmy))
        {
            Destroy(enmy.gameObject);
        }
    }

    void HandlePlayerFallenDeath()
    {
        //Estado de juego
        if(GameDirector.Instance != null)
        {
            GameDirector.Instance.SetOutcome(GameOutcome.RespawnLose);
            GameDirector.Instance.SetPhase(GamePhase.Cutscene);
        }

        //Cambio de escena con fade
        if(SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(loseSceneMenu);
        }
        else
        {
            Debug.LogError("DeathZone:SceneController no encontrado");
        }
    }
}
