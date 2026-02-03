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
    public void OnThrow(Vector3 direction, float horizontalForce, float verticalForce)
    {
        if (!pickable.IsCatched) return;

        // Liberar objeto para reactivar física
        pickable.OnDrop(true);

        // Obtener el peso centralizado
        float weight = pickable.Weight; // Usamos el DataProvider

        // Calculamos la fuerza considerando peso (objetos más pesados se lanzan más lento)
        Vector3 appliedForce = (direction.normalized * horizontalForce + Vector3.up * verticalForce) / Mathf.Max(weight, 0.1f);

        pickable.rb.AddForce(appliedForce, ForceMode.Impulse);

        // Armamos el daño por impacto si existe
        if (impactDamage != null)
        {
            impactDamage.Arm();
        }
    }
    #endregion
}
