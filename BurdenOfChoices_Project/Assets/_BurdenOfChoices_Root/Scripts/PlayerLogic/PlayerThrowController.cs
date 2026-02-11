using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerThrowController: Gestiona input, carga y lanzamiento de objetos.
/// Ahora el peso del objeto se maneja centralmente en PlayerController.
/// </summary>
public class PlayerThrowController : MonoBehaviour
{
    #region Inspector Variables
    [Header("Refs")]
    [SerializeField] Transform throwDirectionSource;
    [SerializeField] Transform throwPreview;

    [Header("Throw Settings")]
    [SerializeField] float throwDelay = 0.25f;

    [Header("Hold Settings")]
    [SerializeField] float holdSpeed = 0.5f;
    [SerializeField] float verticalThrowForce = 0.25f;
    [SerializeField] float minThrowDistance = 2f;
    [SerializeField] float maxThrowDistance = 10f;

    [Header("Preview Settings")]
    [SerializeField] float simulationAirResistance = 0.1f;
    [SerializeField] float previewHeightOffset = 0.05f;
    [SerializeField] float throwFollowSpeed = 8f;
    #endregion

    #region Internal State
    float holdTime;
    bool isHolding;

    Vector3 throwDirectionSmoothed;
    Vector3 cachedThrowDirection;
    float cachedThrowDistance;

    Coroutine throwCoroutine;
    #endregion

    #region References
    PickableBehaviour pickable;
    ThrowableBehaviour throwable;
    AnimatorManager animatorManager;
    PlayerController player;
    #endregion

    void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
        player = GetComponent<PlayerController>();
    }

    void Start()
    {
        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isHolding) return;

        if (pickable == null || throwable == null || !pickable.IsCatched)
        {
            CancelHold();
            return;
        }

        holdTime += Time.deltaTime * holdSpeed;
        holdTime = Mathf.Clamp01(holdTime);

        UpdateThrowPreview();
    }

    #region Input Subscriptions
    void OnEnable()
    {
        InputManager.OnThrowPressed += StartHold;
        InputManager.OnThrowReleased += ReleaseThrow;

        PickableBehaviour.OnEquipped += SetCurrentPickable;
        PickableBehaviour.OnDropped += ClearCurrentPickable;
    }

    void OnDisable()
    {
        InputManager.OnThrowPressed -= StartHold;
        InputManager.OnThrowReleased -= ReleaseThrow;

        PickableBehaviour.OnEquipped -= SetCurrentPickable;
        PickableBehaviour.OnDropped -= ClearCurrentPickable;
    }
    #endregion

    #region Pickable Handling
    void SetCurrentPickable(PickableBehaviour p)
    {
        pickable = p;
        throwable = p.GetComponent<ThrowableBehaviour>();

        // Aplicamos el peso centralizado al jugador
        if (player != null)
            player.SetWeight(pickable != null ? pickable.Weight : 1f);
    }

    void ClearCurrentPickable(PickableBehaviour p)
    {
        if (pickable != p) return;

        CancelHold();
        pickable = null;
        throwable = null;

        // Restauramos peso base del jugador
        if (player != null)
            player.SetWeight(1f);

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);
    }
    #endregion

    #region Hold / Release Logic
    void StartHold()
    {
        if (player != null && player.IsCrouching) return;
        if (pickable == null || throwable == null || !pickable.IsCatched) return;

        isHolding = true;
        holdTime = 0f;

        animatorManager.StartHold();

        if (player != null)
        {
            player.PausePlayer();
            player.LockCrouch();
            player.EnableFreeRotation(true);
        }

        throwDirectionSmoothed = throwDirectionSource.forward;

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(true);
    }

    void ReleaseThrow()
    {
        if (!isHolding || throwable == null)
        {
            CancelHold();
            return;
        }

        isHolding = false;

        cachedThrowDirection = throwDirectionSmoothed;
        cachedThrowDistance = Mathf.Lerp(minThrowDistance, maxThrowDistance, holdTime);

        animatorManager.TriggerThrow();
        AudioManager.Instance.PlaySFX2D("SFX_Grace_Throw", 0.8f);
        AudioManager.Instance.PlaySFX2D("SFX_Object_Throw", 0.8f);

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        if (player != null)
        {
            player.ResumePlayer();
            player.UnlockCrouch();
            player.EnableFreeRotation(false);
        }

        if (throwCoroutine != null)
            StopCoroutine(throwCoroutine);

        throwCoroutine = StartCoroutine(DelayedThrow());
    }

    IEnumerator DelayedThrow()
    {
        yield return new WaitForSeconds(throwDelay);
        yield return StartCoroutine(ExecuteThrowRoutine());
    }

    IEnumerator ExecuteThrowRoutine()
    {
        if (pickable == null || throwable == null)
            yield break;

        pickable.transform.parent = null;

        Rigidbody rb = pickable.rb;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        yield return new WaitForFixedUpdate();

        // Lanzamiento REAL
        throwable.OnThrow(cachedThrowDirection, cachedThrowDistance, verticalThrowForce);
    }

    void CancelHold()
    {
        isHolding = false;
        holdTime = 0f;

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        animatorManager.EndHold();

        if (player != null)
        {
            player.ResumePlayer();
            player.UnlockCrouch();
            player.EnableFreeRotation(false);
        }
    }
    #endregion

    #region Preview
    void UpdateThrowPreview()
    {
        if (throwPreview == null || throwDirectionSource == null) return;

        throwDirectionSmoothed = Vector3.Slerp(
            throwDirectionSmoothed,
            throwDirectionSource.forward,
            throwFollowSpeed * Time.deltaTime
        );

        if (pickable == null)
            return;

        // Factor según peso (igual que en el lanzamiento real)
        float weightFactor = 1f / Mathf.Max(pickable.Weight, 0.1f);
        weightFactor = Mathf.Clamp(weightFactor, 0.15f, 1f);

        float force = Mathf.Lerp(minThrowDistance, maxThrowDistance, holdTime) * weightFactor;
        float vertical = verticalThrowForce * weightFactor;

        Vector3 predicted = PredictLandingPoint(
            throwDirectionSource.position,
            throwDirectionSmoothed,
            force,
            vertical
        );

        throwPreview.position = predicted;
    }

    Vector3 PredictLandingPoint(Vector3 origin, Vector3 direction, float forceImpulse, float verticalImpulse)
    {
        Vector3 dir = direction.normalized;
        float mass = (pickable != null && pickable.rb != null) ? Mathf.Max(0.0001f, pickable.rb.mass) : 1f;
        Vector3 velocity = (dir * forceImpulse + Vector3.up * verticalImpulse) / mass;
        Vector3 pos = origin;

        float dt = 0.02f;
        float maxTime = 5f;

        for (float t = 0; t < maxTime; t += dt)
        {
            velocity += Physics.gravity * dt;
            velocity *= Mathf.Exp(-simulationAirResistance * dt);

            Vector3 nextPos = pos + velocity * dt;

            if (Physics.Raycast(pos, nextPos - pos, out RaycastHit hit, (nextPos - pos).magnitude))
                return hit.point + Vector3.up * previewHeightOffset;

            pos = nextPos;
        }

        return pos;
    }
    #endregion
}
