using UnityEngine;
using System.Collections;

/// <summary>
/// PlayerThrowController: Gestiona el input, carga y lanzamiento de objetos.
/// Toda la lógica de gameplay vive aquí. El Animator es solo visual.
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

    // Valores efectivos según peso
    float currentWeight = 1f;
    float weightFactor = 1f;
    float effectiveHoldSpeed;
    float effectiveMinThrowDistance;
    float effectiveMaxThrowDistance;
    float effectiveVerticalThrowForce;

    // Dirección suavizada durante el hold
    Vector3 throwDirectionSmoothed;

    // 🔒 Valores congelados al soltar
    Vector3 cachedThrowDirection;
    float cachedThrowDistance;

    Coroutine throwCoroutine;

    #endregion

    #region References

    PickableBehaviour pickable;
    ThrowableBehaviour throwable;
    AnimatorManager animatorManager;

    #endregion

    #region Unity Lifecycle

    void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
    }

    void Start()
    {
        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        ResetEffectiveValues();
    }

    void Update()
    {
        if (!isHolding) return;

        if (pickable == null || throwable == null || !pickable.IsCatched)
        {
            CancelHold();
            return;
        }

        holdTime += Time.deltaTime * effectiveHoldSpeed;
        holdTime = Mathf.Clamp01(holdTime);

        UpdateThrowPreview();
    }

    #endregion

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

        var equipableItem = p.GetComponent<EquipableItem>();
        currentWeight = (equipableItem != null && equipableItem.Data != null)
            ? equipableItem.Data.weight
            : 1f;

        weightFactor = Mathf.Clamp(1f / Mathf.Max(currentWeight, 0.1f), 0.15f, 1f);

        effectiveHoldSpeed = holdSpeed * weightFactor;
        effectiveMinThrowDistance = minThrowDistance * weightFactor;
        effectiveMaxThrowDistance = maxThrowDistance * weightFactor;
        effectiveVerticalThrowForce = verticalThrowForce * weightFactor;
    }

    void ClearCurrentPickable(PickableBehaviour p)
    {
        if (pickable != p) return;

        CancelHold();
        pickable = null;
        throwable = null;
        ResetEffectiveValues();

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);
    }

    #endregion

    #region Hold / Release Logic

    void StartHold()
    {
        var player = GetComponent<PlayerController>();

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

        // Congelamos datos definitivos
        cachedThrowDirection = throwDirectionSmoothed;
        cachedThrowDistance = Mathf.Lerp(
            effectiveMinThrowDistance,
            effectiveMaxThrowDistance,
            holdTime
        );

        animatorManager.TriggerThrow();

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        var player = GetComponent<PlayerController>();
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

        // Liberación física
        pickable.transform.parent = null;

        Rigidbody rb = pickable.rb;
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.None;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.WakeUp();

        // Esperamos a que el solver registre el cuerpo
        yield return new WaitForFixedUpdate();

        // Lanzamiento REAL
        throwable.OnThrow(
            cachedThrowDirection,
            cachedThrowDistance,
            effectiveVerticalThrowForce
        );
    }

    void CancelHold()
    {
        isHolding = false;
        holdTime = 0f;

        if (throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        animatorManager.EndHold();

        var player = GetComponent<PlayerController>();
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

        float force = Mathf.Lerp(
            effectiveMinThrowDistance,
            effectiveMaxThrowDistance,
            holdTime
        );

        Vector3 predicted = PredictLandingPoint(
            throwDirectionSource.position,
            throwDirectionSmoothed,
            force,
            effectiveVerticalThrowForce
        );

        throwPreview.position = predicted;
    }

    Vector3 PredictLandingPoint(Vector3 origin, Vector3 direction, float forceImpulse, float verticalImpulse)
    {
        Vector3 dir = direction.normalized;

        float mass = (pickable != null && pickable.rb != null)
            ? Mathf.Max(0.0001f, pickable.rb.mass)
            : 1f;

        Vector3 velocity = (dir * forceImpulse + Vector3.up * verticalImpulse) / mass;
        Vector3 pos = origin;

        float dt = 0.02f;
        float maxTime = 5f;

        for (float t = 0f; t < maxTime; t += dt)
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

    #region Utils

    void ResetEffectiveValues()
    {
        currentWeight = 1f;
        weightFactor = 1f;
        effectiveHoldSpeed = holdSpeed;
        effectiveMinThrowDistance = minThrowDistance;
        effectiveMaxThrowDistance = maxThrowDistance;
        effectiveVerticalThrowForce = verticalThrowForce;
    }

    #endregion
}
