using UnityEngine;

/// <summary>
/// RoomTrigger
/// Define el estado emocional del jugador
/// según la presencia de enmigos en la sala.
/// </summary>
public class RoomTrigger : MonoBehaviour
{
    #region Inspector VAriables
    [SerializeField] bool hasEnemies; //Indica si la sala tiene enemigos
    #endregion

    #region References
    AnimatorManager animatorManager;
    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        animatorManager = other.GetComponent<AnimatorManager>();
        if(animatorManager == null) return;

        ApplyRoomState();
    }

    private void OnTriggerExit(Collider other)
    {
        if(!other.CompareTag("Player")) return;

        if (animatorManager != null)
            animatorManager.SetRelaxed(true);

        animatorManager = null;
    }

    #region Core
    void ApplyRoomState()
    {
        //Sala hostil => jugador tenso
        animatorManager.SetRelaxed(!hasEnemies);
    }
    #endregion
}
