using UnityEngine;

/// <summary>
/// ThrowableBehaviour: Es el que contiene la logica para aplicar fuerza al objeto lanzable.
/// </summary>
public class ThrowableBehaviour : MonoBehaviour
{
    PickableBehaviour pickable;

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        if (pickable == null)
            Debug.LogWarning("TrowableBehaviour requiere un PickableBehaviour en el mismo objeto.");
    }

    #region Throw
    public void OnThrow (Vector3 direction, float horizontalForce, float verticalForce)
    {
        //No lanzar si no está equipado
        if (!pickable.IsCatched) return;

        //Priemro dropeamos para reactivar física
        pickable.OnDrop();

        Vector3 appliedForce = direction.normalized * horizontalForce + Vector3.up * verticalForce;

        //Aplicamos fuerza limpia
        pickable.rb.AddForce(appliedForce, ForceMode.Impulse);
    }
    #endregion
}
