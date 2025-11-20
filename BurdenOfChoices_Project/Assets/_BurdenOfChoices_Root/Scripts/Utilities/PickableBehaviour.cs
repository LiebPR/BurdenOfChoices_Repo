using System;
using UnityEngine;

/// <summary>
/// PickableBehaviour:
///     - Guarda posición, rotación y escala originales.
///     - Permite Pick/Drop sin hacer el objeto hijo del player.
///     - Sigue al target usando Rigidbody.Move para evitar atravesar paredes.
/// </summary>
public class PickableBehaviour : MonoBehaviour
{
    #region Internal State
    Vector3 originalPosition;
    Quaternion originalRotation;
    Vector3 originalScale;
    Transform originalParent;

    Transform followTarget;
    bool isPicked;

    Rigidbody rb;
    #endregion

    #region Events
    public event Action OnPicked;
    public event Action OnDropped;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        originalParent = transform.parent;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        if (!isPicked || followTarget == null) return;

        //Seguimiento suave al target sin hacer hijo
        Vector3 targetPos = followTarget.position;
        Quaternion targetRot = followTarget.rotation;

        rb.MovePosition(targetPos);
        rb.MoveRotation(targetRot);
    }

    //Recoge el objeto y lo prepara para seguir al target
    public void Pick(Transform target)
    {
        if (target == null) return;

        followTarget = target;
        isPicked = true;

        rb.useGravity = false;

        transform.localScale = originalScale;

        OnPicked?.Invoke();
    }

    public void Drop()
    {
        if(!isPicked) return;

        isPicked = false;
        followTarget = null;

        rb.useGravity = true;

        transform.SetParent(null);

        transform.localScale = originalScale;

        OnDropped?.Invoke();  
    }

    //Devuelve al objeto EXACTAMENTE a su estado inicial.
    public void ResetToOriginalState()
    {
        isPicked = false;
        followTarget = null;

        transform.SetParent(originalParent);
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        rb.useGravity = true; 
    }
}
