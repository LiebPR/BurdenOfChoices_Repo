using UnityEngine;
using System;

/// <summary>
/// PickableBehaviour: Es el que gestiona la logica de recoger un objeto, soltarlo o reseteralo.
/// </summary>
public class PickableBehaviour : MonoBehaviour
{
    #region Inspector Variables
    [Tooltip("Si es falso, el objeto no se equipa y solo se usa para interacción/drag.")]
    public bool AllowEquip = true;
    [Header("Drop / GroundCheck")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] float groundCheckDistance = 0.25f;
    [SerializeField] float groundStickTime = 0.05f; //tiempo de pegado al suelo

    [Header("Debug")]
    [SerializeField] bool debugDrawGroundRay = true;
    [SerializeField] Color debugRayColor = Color.red;

    [Header("Restore")]
    [SerializeField] bool isRestoreWithTime = true;
    [SerializeField] float restoreDelay = 1.5f;

    [Header("Grab Point")]
    [SerializeField] Transform grabPoint;

    [Header("Collider")]
    [SerializeField] Collider coll;
    #endregion

    #region Internal States
    Transform catchPoint;
    
    bool isCatched;
    bool pendingDropRequest;
    bool restoreRunning;
    bool blockDrop;

    //Original States:
    Vector3 originalPosition;
    Quaternion originalRotation;
    Vector3 originalScale;

    float restoreTimer;
    float groundedStableTimer;
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
        if(rb == null)
            rb = GetComponent<Rigidbody>();

        if(coll == null)
            coll = GetComponent<Collider>();

        //Guardamos el estado original del objeto
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    private void Update()
    {
        //Dibujar el rayo de comprobación del suelo sólo cuando el objeto esta cogido
#if UNITY_EDITOR
        if(debugDrawGroundRay && isCatched && groundCheckDistance > 0)
        {
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            float maxDistance = groundCheckDistance + 0.1f;
            Debug.DrawRay(origin, Vector3.down * maxDistance, debugRayColor);
        }
#endif

        //Si hay una petición pediente de soltado y ahora está en suelo, ejecutarla
        if(pendingDropRequest && isCatched)
        {
            if (IsGrounded())
            {
                pendingDropRequest = false;
                OnDrop();
            }
        }
    }

    #region Equip
    //Coloca el obejto en la mano del jugador. 
    public void OnEquip(ICatcher catcher)
    {
        //Cancelar cualquier restore pendiente
        CancelInvoke(nameof(RestoreInternal));
        CancelInvoke(nameof(UpdateRestoreTimer));
        restoreRunning = false;

        if(catcher == null)
        {
            Debug.LogWarning("No se proporcionó un ICatcher válido");
            return;
        }

        if (!AllowEquip)
        {
            //Solo notifica que fue pickeado, sin equipar
            NotifyEquipListeners(catcher);
            return;
        }


        catchPoint = catcher.GetCatchPoint();
        isCatched = true;

        if(rb != null)
        {
            //Desactivar la física completamente
            rb.isKinematic = true;
            rb.useGravity = false;
            coll.isTrigger = true;
        }

        //Parent al catchPoint
        SnapTopGrabPoint();

        OnEquipped?.Invoke(this); //notifica que se equipó
        NotifyEquipListeners(catcher);
        NotifyEquipListeners(catcher);
    }

    /// <summary>
    /// Marca el objeto como cogido pero sin equiparlo en la mano
    /// Pensado para objetos arrastrables.
    /// </summary>
    public void OnDragEquip(ICatcher catcher)
    {
        CancelInvoke(nameof(RestoreInternal));
        CancelInvoke(nameof(UpdateRestoreTimer));
        restoreRunning = false;

        isCatched = true;
        catchPoint = null;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            coll.isTrigger = false;
        }

        Debug.Log("OnDragEquip -> Disparando OnEquipped para: " + gameObject.name);
        OnEquipped?.Invoke(this);
        NotifyEquipListeners(catcher);
    }

    void NotifyEquipListeners(ICatcher catcher)
    {
        var listeners = GetComponents<IPickListener>();
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].OnPick(catcher);
    }
    #endregion

    #region Drop
    //Suelta el objeto en el mundo.
    //Si force == false, sólo soltará si detecta suelo debajo.
    //Si force == true, obligará el drop.
    public void OnDrop(bool force = false)
    {
        //Si forzamos y no hay suelo detectado, no soltamos (se queda en la mano)
        if(!force && !IsGrounded())
        {
            pendingDropRequest = true;

            //Asegurar que el estado permanece como 'cogido' y que siga parentado al cathPoint
            isCatched = true;
            SnapTopGrabPoint();
            if(rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                coll.isTrigger = true;
            }
            return;
        }

        //Drop normal
        pendingDropRequest = false;
        isCatched = false;

        //Quita el parent
        transform.SetParent(null);

        // Revisar si es arrastrable
        var draggable = GetComponent<DraggableBehaviour>();
        if (draggable != null)
        {
            // Siempre kinematic si es arrastrable
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
                coll.isTrigger = false;
            }
        }
        else
        {
            // Física normal para objetos normales
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                coll.isTrigger = false;
            }
        }

        //Restauramos el tamaño si se a alterado
        transform.localScale = originalScale;

        OnDropped?.Invoke(this); //notifica que se soltó
        NotifyDropListeners();

        //Restore condicional
        if (isRestoreWithTime)
        {
            restoreTimer = restoreDelay;
            restoreRunning = true;

            InvokeRepeating(nameof(UpdateRestoreTimer), 0f, Time.deltaTime);
            Invoke(nameof(RestoreInternal), restoreDelay);
        }
    }

    public void OnDropWithoutPhysics()
    {
        pendingDropRequest = false;
        isCatched = false;

        //Quitar parent
        transform.SetParent(null);

        //Restaurar escala si fue alterada
        transform.localScale = originalScale;

        //notificar eventos
        OnDropped?.Invoke(this);
        NotifyDropListeners();
        restoreRunning = false;
        CancelInvoke(nameof(UpdateRestoreTimer));
        CancelInvoke(nameof(RestoreInternal));
    }

    public void BlockDrop()
    {
        blockDrop = true;
    }

    void NotifyDropListeners()
    {
        var listeners = GetComponents<IPickListener>();
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].OnDrop();
    }
    #endregion

    #region Restore
    //Suelta y restaura tras un tiempo
    public void OnRestoreWithTime(float delay)
    {
        OnDrop(true);
        Invoke(nameof(RestoreInternal), delay);
    }

    //Suelta y restaura inmediatamente
    public void OnRestore()
    {
        OnDrop(true);
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

    #region Public API
    public void RequestDrop()
    {
        if (blockDrop)
            return;
        //Llamamos a OnDrop sin forzar: se encargará de encolar si no hay suelo
        OnDrop(false);
    }

    public bool CanBeDropped()
    {
        return IsGrounded();
    }
    #endregion

    #region Ground Check
    bool IsGrounded()
    {
        //Si no esta cogido, no ejecutamos el raycast
        if (!isCatched) return true;

        //Si la distancia es 0, asumimos que puede soltarse
        if(groundCheckDistance <= 0f) return true;

        //Origen del raycast un poco por encima del centro del objeto para evitar empezar dentro del suelo
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float maxDistance = groundCheckDistance + 0.1f;

        bool hitGround = Physics.Raycast(origin, Vector3.down, maxDistance, groundLayer);

        if (!hitGround)
        {
            //si no hay suelo, reiniciamos el contador
            groundedStableTimer = 0f;
            return false;
        }
        
        //Hay suelo -> empezamos a contar estabilidad
        groundedStableTimer += Time.deltaTime;

        //Mientras no supere el buffer, seguimos considerándolo grounded
        return groundedStableTimer >= groundStickTime;
    }
    #endregion

    void SnapTopGrabPoint()
    {
        if (catchPoint == null) return;

        transform.SetParent(catchPoint);

        if (grabPoint != null)
        {
            transform.localPosition = -grabPoint.localPosition;
            transform.localRotation = Quaternion.Inverse(grabPoint.localRotation);
        }
        else
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }

        // No tocar velocities
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            coll.isTrigger = true;
        }
    }

    #region Timer Visuals
    void UpdateRestoreTimer()
    {
        restoreTimer -= Time.deltaTime;

        if (restoreTimer <= 0f)
        {
            restoreTimer = 0f;
            CancelInvoke(nameof(UpdateRestoreTimer));
        }
    }

    public float GetRestoreRmainingTime()
    {
        return restoreRunning ? restoreTimer : 0f;
    }

    public float GetRestoreTotalTime()
    {
        return restoreDelay;
    }
    #endregion
}
