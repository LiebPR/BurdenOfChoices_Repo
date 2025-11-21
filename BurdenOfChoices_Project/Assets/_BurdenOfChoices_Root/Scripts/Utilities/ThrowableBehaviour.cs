using System;
using UnityEngine;

public class ThrowableBehaviour : MonoBehaviour
{
    [SerializeField] float throwForce = 15f;

    PickableBehaviour pickable;

    #region Throw
    public void OnThrow (Vector3 direction)
    {
        //No lanzar si no está equipado
        if (!pickable.IsCatched) return;

        //Priemro dropeamos para reactivar física
        pickable.OnDrop();

        //Aplicamos fuerza limpia
        pickable.rb.AddForce(direction * throwForce, ForceMode.Impulse);
    }
    #endregion
}
