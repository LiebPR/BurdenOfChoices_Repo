using UnityEngine;

public class SimpleThrowableItem : MonoBehaviour
{
    #region Reefrences
    PickableBehaviour pickable;
    PlayerHand playerHand;
    #endregion

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();

        playerHand = FindAnyObjectByType<PlayerHand>();
        if (playerHand == null)
        {
            Debug.LogWarning("No se encontró PlayerHand en la escena.");
        }
    }
}
