using System;
using UnityEngine;

public class Statue : MonoBehaviour
{
    #region Inspector State
    [SerializeField] float forwardForce = 3f;
    #endregion

    #region Internal States
    bool hasFallen;
    Vector3 startPosition;
    Quaternion startRotation;
    #endregion

    #region References
    Rigidbody rb;
    #endregion

    #region Events
    public event Action<Statue> OnFallen; // Notifica a la sala que cayó
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void Start()
    {
        // Punto EXACTO encima del pilar
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    #region Public Methods
    public void DropStatue(Vector3 pushDirection)
    {
        if (hasFallen) return;

        rb.isKinematic = false;
        rb.AddForce(pushDirection * forwardForce, ForceMode.Impulse);

        hasFallen = true;
        Debug.Log("Para que funcione con el tutorial, solo sirvo para eso. Para retrasar la llamada de OnFallen sin afectar al sistema de Puzzle");
        OnFallen?.Invoke(this);
    }
    #endregion

    public void ResetStatue()
    {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = startPosition;
        transform.rotation = startRotation;

        hasFallen = false;
    }

    public bool IsFallen()
    {
        return hasFallen;
    }
}
