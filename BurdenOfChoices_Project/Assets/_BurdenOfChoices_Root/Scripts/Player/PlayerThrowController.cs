using UnityEngine;

public class PlayerThrowController : MonoBehaviour
{
    #region References
    PickableBehaviour pickable;
    ThrowableBehaviour throwable;
    Transform player;
    #endregion

    private void OnEnable()
    {
        InputManager.OnThrow += HandleThrow;
    }

    private void OnDisable()
    {
        InputManager.OnThrow -= HandleThrow;
    }

    #region Throw Logic
    //Callback llamado cuando el jugador ejecuta el input de lanzar.
    void HandleThrow()
    {
        // Verificamos que haya un pickable asignado
        if (pickable == null || throwable == null || player == null) return;

        // Solo lanzar si el objeto está equipado
        if (!pickable.IsCatched) return;

        // Dirección del lanzamiento según la cámara
        Vector3 direction = player.forward;

        // Ejecutamos el lanzamiento
        throwable.OnThrow(direction);
    }
    #endregion
}
