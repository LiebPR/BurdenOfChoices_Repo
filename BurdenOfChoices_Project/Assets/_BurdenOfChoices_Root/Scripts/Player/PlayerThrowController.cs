using UnityEngine;

/// <summary>
/// PlayerThrowController: Es el que gestiona el input de lanzar objetos.
/// </summary>
public class PlayerThrowController : MonoBehaviour
{
    #region Inspector Variables
    [Header("Refs")]
    [SerializeField] Transform throwDirectionSource; //transform que indica la dirección de lanzamiento
    [SerializeField] Transform throwPreview; //Imagen que indica el punto final del lanzamiento

    [Header("Hold Settings")]
    [SerializeField] float maxHoldTime = 2f; //tiempo máximo de carga
    [SerializeField] float minThrowForce = 2f; //fuerza mínima
    [SerializeField] float maxThrowForce = 10f; //fuerza máxima

    [Header("HoldPreview Settings")]
    [SerializeField] float minPreviewDistance = 2f;
    [SerializeField] float maxPreviewDistance = 10f;
    #endregion

    #region Internal States
    float holdTime;
    bool isHolding;
    #endregion

    #region References
    PickableBehaviour pickable;
    ThrowableBehaviour throwable;
    #endregion

    private void Start()
    {
        if (throwPreview != null)
        {
            throwPreview.gameObject.SetActive(false);
        }
            
    }

    private void OnEnable()
    {
        InputManager.OnThrowPressed += StartHold;
        InputManager.OnThrowReleased += ReleaseThrow;

        PickableBehaviour.OnEquipped += SetCurrentPickable;
        PickableBehaviour.OnDropped += ClearCurrentPickable;
    }

    private void OnDisable()
    {
        InputManager.OnThrowPressed -= StartHold;
        InputManager.OnThrowReleased -= ReleaseThrow;

        PickableBehaviour.OnEquipped -= SetCurrentPickable;
        PickableBehaviour.OnDropped -= ClearCurrentPickable;
    }

    private void Update()
    {
        if(isHolding && pickable != null)
        {
            holdTime += Time.deltaTime;

            //Limitar el tiempo máximo
            if(holdTime > maxHoldTime)
            {
                holdTime = maxHoldTime;
            }

            UpdateThrowPreview();
        }
    }

    void SetCurrentPickable(PickableBehaviour p)
    {
        pickable = p;
        throwable = p.GetComponent<ThrowableBehaviour>();
    }

    void ClearCurrentPickable(PickableBehaviour p)
    {
        if (pickable == p)
        {
            pickable = null;
            throwable = null;

            if (throwPreview != null)
                throwPreview.gameObject.SetActive(false);
        }
    }

    void StartHold()
    {
        if (pickable == null || !pickable.IsCatched) return;

        isHolding = true;
        holdTime = 0f;

        if (throwPreview != null)
        {
            throwPreview.gameObject.SetActive(true);

            // Lo colocamos EXACTAMENTE en el mínimo desde el primer frame
            Vector3 minPos = throwDirectionSource.position + throwDirectionSource.forward * minPreviewDistance;
            throwPreview.position = minPos;
        }
    }

    void ReleaseThrow()
    {
        if (!isHolding) return;

        isHolding = false;

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        if (pickable == null || throwable == null) return;

        float force = Mathf.Lerp(minThrowForce, maxThrowForce, holdTime / maxHoldTime);
        Vector3 direction = throwDirectionSource.forward;

        throwable.OnThrow(direction, force);
    }

    void UpdateThrowPreview()
    {
        if (throwPreview == null || throwDirectionSource == null || pickable == null) return;

        // Fuerza proporcional al tiempo de carga
        float currentForce = Mathf.Lerp(minThrowForce, maxThrowForce, holdTime / maxHoldTime);

        // Distancia proyectada según la fuerza actual
        float projectedDistance = currentForce; // o multiplicar por una constante si quieres escalar visualmente

        // Si aún no alcanza la mínima distancia, no mostrar preview
        if (projectedDistance < minThrowForce)
        {
            throwPreview.gameObject.SetActive(false);
            return;
        }

        if (!throwPreview.gameObject.activeSelf)
            throwPreview.gameObject.SetActive(true);

        // Limitar la distancia para que no supere el máximo
        projectedDistance = Mathf.Min(projectedDistance, maxThrowForce);

        // Calcular distancia relativa desde mínimo
        float relativeDistance = projectedDistance - minThrowForce;

        // Posición base del lanzamiento
        Vector3 basePos = throwDirectionSource.position + throwDirectionSource.forward * minPreviewDistance;

        // Posición final del preview (desde minPreviewDistance hasta maxPreviewDistance)
        float maxRelative = maxPreviewDistance - minPreviewDistance;
        float t = Mathf.Clamp01(relativeDistance / (maxThrowForce - minThrowForce));
        Vector3 previewPos = basePos + throwDirectionSource.forward * (maxRelative * t);

        throwPreview.position = previewPos;
    }
}
