using UnityEngine;

public class SimpleThrowableItem : MonoBehaviour
{
    #region Reefrences
    PickableBehaviour pickable;
    PlayerHand playerHand;
    DataProvider dataProvider;
    #endregion

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        dataProvider = GetComponent<DataProvider>();

        playerHand = GetComponent<PlayerHand>();
        if(playerHand == null)
        {
            Debug.LogWarning("No se encontró PlayerHand en la escena.");
        }
    }
}
