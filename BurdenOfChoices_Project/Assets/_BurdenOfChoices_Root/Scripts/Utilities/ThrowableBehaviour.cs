using UnityEngine;

/// <summary>
/// ThrowableBehaviour: Es el que contiene la logica para aplicar fuerza al objeto lanzable.
/// </summary>
public class ThrowableBehaviour : MonoBehaviour
{
    PickableBehaviour pickable;
    ThrowImpactDamage impactDamage;

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        if (pickable == null)
            Debug.LogWarning("TrowableBehaviour requiere un PickableBehaviour en el mismo objeto.");
        impactDamage = GetComponent<ThrowImpactDamage>();
    }

    #region Throw
    public void OnThrow (Vector3 direction, float horizontalForce, float verticalForce)
    {
        if(!pickable.IsCatched) return;

        //Drop para reactivar física 
        pickable.OnDrop(true);

        //Considerar el peso del objeto
        Vector3 appliedForce = direction.normalized * horizontalForce + Vector3.up * verticalForce;

        // Aplicamos fuerza proporcional al peso
        pickable.rb.AddForce(appliedForce, ForceMode.Impulse);

        if (impactDamage != null)
        {
            impactDamage.Arm();
        }
    }
    #endregion
}
