using System;
using UnityEngine;

/// <summary>
/// ThrowableBehaviour: Lógica de lanzamiento.
/// </summary>
public class ThrowableBehaviour : MonoBehaviour
{
    #region General Variables
    [SerializeField] float forwardForce = 8f;
    [SerializeField] float upwardForce = 1f;
    #endregion

    #region References
    Rigidbody rb;
    #endregion

    #region Events
    public event Action OnThrown;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        //Aegura fisicas adecuadas
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    //Lanzar el objeto en la dirección de un transform target.
    public void OnThrow(Transform target)
    {
        Debug.Log($"Throw called on {gameObject.name}");
        if (target == null) return;

        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        //Calcular dirección
        Vector3 direction = target.forward * forwardForce + target.up * upwardForce;

        //Aplicar fuerza instantánea 
        rb.AddForce(direction, ForceMode.VelocityChange);

        OnThrown?.Invoke();
    }

    //Lanzar el objeto en una dirección específica (opcional).
    public void Throw(Vector3 direction)
    {
        transform.SetParent(null);

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.AddForce(direction, ForceMode.VelocityChange);

        OnThrown?.Invoke();
    }

    //Reiniciar Rigidbody para volver a estado inicial si es necesario
    public void ResetRigidbody()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}
