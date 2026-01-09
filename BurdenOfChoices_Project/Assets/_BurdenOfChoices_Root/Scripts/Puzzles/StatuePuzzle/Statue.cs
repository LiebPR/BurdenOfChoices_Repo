using System;
using UnityEngine;

public class Statue : MonoBehaviour
{
    #region Inspector State
    [SerializeField] float forwardForce = 3f;
    #endregion

    #region Internal States
    bool hasFallen;
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

    #region Public Methods
    public void DropStatue(Vector3 pushDirection)
    {
        rb.isKinematic = false;
        rb.AddForce(pushDirection * forwardForce, ForceMode.Impulse);

        //Avisar de que cayó
        hasFallen = true;
        OnFallen?.Invoke(this);
    }
    #endregion

    public bool IsFallen()
    {
        return hasFallen;
    }
}
