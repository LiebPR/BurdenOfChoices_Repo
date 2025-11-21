using UnityEngine;
using System;

/// <summary>
/// PickableBehaviour: Es el que gestiona la logica de recoger un objeto, soltarlo o reseteralo.
/// </summary>
public class PickableBehaviour : MonoBehaviour
{
    Transform catchPoint;

    #region Internal States
    bool isCatched;

    //Original States:
    Vector3 originalPosition;
    Quaternion originalRotation;
    Vector3 originalScale;
    #endregion

    #region Rferences
    public Rigidbody rb;
    #endregion

    #region Getters
    public bool IsCatched => isCatched;
    #endregion

    #region Eventos
    public static event Action<PickableBehaviour> OnEquipped;
    public static event Action<PickableBehaviour> OnDropped;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        //Guardamos el estado original del objeto
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    #region Equip
    //Coloca el obejto en la mano del jugador. 
    public void OnEquip(ICatcher catcher)
    {
        if(catcher == null)
        {
            Debug.LogWarning("No se proporcionó un ICatcher válido");
            return;
        }

        catchPoint = catcher.GetCatchPoint();

        isCatched = true;

        //Desactivar la física completamente
        rb.isKinematic = true;
        rb.useGravity = false;

        //Parent al catchPoint
        transform.SetParent(catchPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        OnEquipped?.Invoke(this); //notifica que se equipó
    }
    #endregion

    #region Drop
    //Suelta el objeto en el mundo.
    public void OnDrop()
    {
        isCatched = false;

        //Quita el parent
        transform.SetParent(null);

        //Activa la física
        rb.isKinematic = false;
        rb.useGravity = true;

        //Restauramos el tamaño si se a alterado
        transform.localScale = originalScale;

        OnDropped?.Invoke(this); //notifica que se soltó
    }
    #endregion

    #region Restore
    //Suelta y restaura tras un tiempo
    public void OnRestoreWithTime(float delay)
    {
        OnDrop();
        Invoke(nameof(RestoreInternal), delay);
    }

    //Suelta y restaura inmediatamente
    public void OnRestore()
    {
        OnDrop();
        RestoreInternal();
    }

    void RestoreInternal()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.localPosition = originalPosition;
        transform.rotation = originalRotation;
    }
    #endregion
}
