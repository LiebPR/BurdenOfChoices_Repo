using UnityEngine;

/// <summary>
/// PlayerThrowController: Es el que gestiona el input de lanzar objetos.
/// </summary>
public class PlayerThrowController : MonoBehaviour
{
    #region Inspector Variables
    [Header("Refs")]
    [SerializeField] Transform throwDirectionSource; //transform que indica la dirección de lanzamiento
    [SerializeField] Transform throwPreview; //imagen que indica el punto final del lanzamiento

    [Header("Hold Settings")]
    [SerializeField] float holdSpeed = 0.5f; //velocidad de carga
    [SerializeField] float minThrowDistance = 2f; //fuerza mínima
    [SerializeField] float maxThrowDistance = 10f; //fuerza máxima
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
            holdTime += Time.deltaTime * holdSpeed;
            holdTime = Mathf.Clamp01(holdTime); //siempre entre 0 y 1

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
            Vector3 minPos = throwDirectionSource.position + throwDirectionSource.forward * minThrowDistance * 0.35f;
            throwPreview.position = minPos;
        }
    }

    void ReleaseThrow()
    {
        if (!isHolding) return;

        isHolding = false;
        throwPreview.gameObject.SetActive(false);

        if (pickable == null || throwable == null) return;

        float throwDistance = Mathf.Lerp(minThrowDistance, maxThrowDistance, holdTime);

        Vector3 direction = throwDirectionSource.forward;
        throwable.OnThrow(direction, throwDistance); //el objeto decidirá cómo usarlo

        holdTime = 0f;
    }

    void UpdateThrowPreview()
    {
        if (throwPreview == null || throwDirectionSource == null) return;

        // Distancias reducidas a la mitad para el preview
        float previewMin = minThrowDistance * 0.35f;
        float previewMax = maxThrowDistance * 0.35f;

        float previewDistance = Mathf.Lerp(previewMin, previewMax, holdTime);

        Vector3 pos = throwDirectionSource.position + throwDirectionSource.forward * previewDistance;

        throwPreview.position = pos;

        if (!throwPreview.gameObject.activeSelf)
            throwPreview.gameObject.SetActive(true);
    }
}
