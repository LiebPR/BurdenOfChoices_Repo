using UnityEngine;

public class PlayerThrowableController : MonoBehaviour
{
    [SerializeField] Transform throwPoint; //punto de lanzamiento (mano)

    #region References
    PickableBehaviour equippedItem; //objeto actualmente en la mano
    #endregion

    #region Input Subscriptions & Unsubscriptions
    private void OnEnable()
    {
        InputManager.OnThrow += HandleThrowInput;
    }

    private void OnDisable()
    {
        InputManager.OnThrow -= HandleThrowInput;
    }
    #endregion

    void HandleThrowInput()
    {
        if (equippedItem == null) return;

        ThrowableBehaviour throwable = equippedItem.GetComponent<ThrowableBehaviour>();
        if(throwable != null)
        {
            //Lanza el objeto usando su propia lógica
            throwable.OnThrow(throwPoint);

            //Soltar de la mano
            equippedItem.Drop();
            equippedItem = null;
        }
    }

    //Llamar cuando el jugador equipe un objeto.
    public void EquipItem(PickableBehaviour item)
    {
        equippedItem = item;
        equippedItem.Pick(throwPoint);
    }
}
