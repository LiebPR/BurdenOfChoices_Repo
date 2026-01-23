using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// PlayerController: Contiene las lógicas de movimiento del jugador con suavizado completo.
/// Separa movimiento por input y fuerzas externas. La rotación se mantiene intacta.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Inspector Variables
    [Header("Movement Config")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float runSpeed = 8f;
    [SerializeField] float crouchSpeed = 2.5f;
    [SerializeField] float accelerationTime = 0.2f;
    [SerializeField] float decelerationTime = 0.3f;

    [Header("Rotation Config")]
    [SerializeField] float rotationSpeed = 10f;
    [SerializeField] float minSpeedForRotation = 0.05f;
    [SerializeField] float inertiaFactor = 0.15f;
    [SerializeField] float rotationNoise = 0.02f;

    [Header("Free Rotation")]
    [SerializeField] float freeRotationSensitivity = 3f;

    [Header("Weight Factor")]
    [SerializeField] float weightSpeedSensitivity = 0.25f;
    [SerializeField] float weightAccelerationSensitivity = 0.1f;

    [Header("External Forces")]
    [SerializeField] float externalDamping = 8f;
    #endregion

    #region Internal State
    // INPUT
    Vector2 inputMovement;
    bool isRunning;
    bool movementLocked;

    // VELOCIDAD
    Vector3 currentVelocitySmooth;
    Vector3 externalVelocity;

    // ROTACIÓN
    Vector3 lastMoveDirection;
    bool rotationLocked;
    bool freeRotation;

    // CROUCH
    bool isCrouching;
    bool crouchLocked;

    // PESO
    float currentEquipWeight = 1f;
    float currentWeightSpeedMultiplier = 1f;

    // DRAG
    Vector3 dragAxisLocked = Vector3.zero;

    // GAME STOP
    bool wasGamePaused;
    #endregion

    #region References
    public Rigidbody rb;
    AnimatorManager animatorManager;
    #endregion

    #region Getters
    public bool IsCrouching => isCrouching;
    public Vector3 PlanarVelocity => new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void Update()
    {
        bool isPaused = GameStopManager.Instance != null && GameStopManager.Instance.isGamePaused;

        // Transición: Playing → Paused
        if (isPaused && !wasGamePaused)
            PausePlayer();
        // Transición: Paused → Playing
        else if (!isPaused && wasGamePaused)
            ResumePlayer();

        wasGamePaused = isPaused;

        if (isPaused)
            return;

        if (freeRotation)
            HandleFreeRotation();
        else
            HandleRotation();
    }

    private void FixedUpdate()
    {
        if (!movementLocked)
        {
            HandleMovementSpeed();
        }
        else
        {
            // Frenado rápido cuando se pausa
            float pauseBrakeFactor = 15f; // más alto → frena más rápido
            Vector3 planar = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            planar = Vector3.Lerp(planar, Vector3.zero, pauseBrakeFactor * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(planar.x, rb.linearVelocity.y, planar.z);

            currentVelocitySmooth = Vector3.zero; // reset de SmoothDamp
        }

        ApplyExternalVelocity(); // externalVelocity siempre intacta
        UpdateAnimatorVelocity();
    }

    private void OnEnable()
    {
        InputManager.OnMoveChanged += OnMoveChanged;
        InputManager.OnRunChanged += OnRunChanged;
        InputManager.OnCrouchChanged += OnCrouchChanged;

        PickableBehaviour.OnEquipped += OnPickableEquipped;
        PickableBehaviour.OnDropped += OnPickableDropped;
    }

    private void OnDisable()
    {
        InputManager.OnMoveChanged -= OnMoveChanged;
        InputManager.OnRunChanged -= OnRunChanged;
        InputManager.OnCrouchChanged -= OnCrouchChanged;

        PickableBehaviour.OnEquipped -= OnPickableEquipped;
        PickableBehaviour.OnDropped -= OnPickableDropped;
    }

    #region Movement Core
    void HandleMovementSpeed()
    {
        // Determinar velocidad objetivo
        float targetSpeed = (isCrouching ? crouchSpeed : isRunning ? runSpeed : walkSpeed) * currentWeightSpeedMultiplier;
        if (isCrouching) targetSpeed = crouchSpeed;
        else if (isRunning) targetSpeed = runSpeed;

        targetSpeed *= currentWeightSpeedMultiplier;

        Vector3 inputDir = new Vector3(inputMovement.x, 0f, inputMovement.y);

        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // Si no hay input o movimiento bloqueado → frenar rápido
        if (inputDir.sqrMagnitude < 0.01f || movementLocked)
        {
            // Factor de frenado extra
            float brakeFactor = 10f; // más alto → frena más rápido
            Vector3 targetVelocity = Vector3.zero;

            // Lerp fuerte hacia cero, solo en plano XZ
            planarVelocity = Vector3.Lerp(planarVelocity, targetVelocity, brakeFactor * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(planarVelocity.x, rb.linearVelocity.y, planarVelocity.z);

            // Limpiar SmoothDamp
            currentVelocitySmooth = Vector3.zero;
            return;
        }

        inputDir.Normalize();
        Vector3 desiredVelocity = inputDir * targetSpeed;

        // Bloqueo de ejes (drag)
        if (dragAxisLocked != Vector3.zero)
        {
            if (Mathf.Abs(dragAxisLocked.x) > 0f) desiredVelocity.x = 0f;
            if (Mathf.Abs(dragAxisLocked.z) > 0f) desiredVelocity.z = 0f;
        }

        // Suavizado normal de movimiento
        float smoothTime = (desiredVelocity.magnitude > planarVelocity.magnitude) ? accelerationTime : decelerationTime;
        Vector3 smoothVelocity = Vector3.SmoothDamp(planarVelocity, desiredVelocity, ref currentVelocitySmooth, smoothTime);

        rb.linearVelocity = new Vector3(smoothVelocity.x, rb.linearVelocity.y, smoothVelocity.z);
    }

    void ApplyExternalVelocity()
    {
        if (externalVelocity.sqrMagnitude < 0.0001f) return;

        // Sumamos el externalVelocity a la velocidad actual del Rigidbody
        rb.linearVelocity += new Vector3(externalVelocity.x, 0f, externalVelocity.z);

        // Reducimos gradualmente el externalVelocity
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, externalDamping * Time.fixedDeltaTime);
    }
    #endregion

    #region Pause / Resume
    public void PausePlayer()
    {
        movementLocked = true;

        // Limpiar solo input, NO fuerzas externas
        inputMovement = Vector2.zero;
        currentVelocitySmooth = Vector3.zero;
    }

    public void ResumePlayer()
    {
        movementLocked = false;
    }
    #endregion

    #region Drag Movement Lock
    public void LockMovementAxis(Vector3 axis) => dragAxisLocked = axis;
    public void UnlockMovementAxis() => dragAxisLocked = Vector3.zero;
    #endregion

    #region Crouch Locks
    public void LockCrouch() => crouchLocked = true;
    public void UnlockCrouch() => crouchLocked = false;
    #endregion

    #region Rotation
    void HandleRotation()
    {
        if (rotationLocked) return;

        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = planarVelocity.magnitude;

        Vector3 inputDir = new Vector3(inputMovement.x, 0f, inputMovement.y);
        bool hasInput = inputDir.sqrMagnitude > 0.0001f;
        if (hasInput) inputDir.Normalize();

        Vector3 desiredDir;
        if (speed >= minSpeedForRotation) desiredDir = planarVelocity.normalized;
        else if (hasInput) desiredDir = inputDir;
        else return;

        float dynamicInertia = Mathf.Lerp(inertiaFactor, inertiaFactor * 1.5f, 1f - Mathf.Clamp01(speed));
        Vector3 inertialDir = Vector3.Slerp(lastMoveDirection, desiredDir, 1f - dynamicInertia);

        if (speed > 0.5f || hasInput)
        {
            float noiseFactor = Mathf.Clamp01(speed / runSpeed);
            inertialDir += new Vector3(Random.Range(-rotationNoise, rotationNoise), 0f, Random.Range(-rotationNoise, rotationNoise)) * (1f - noiseFactor);
        }

        if (inertialDir.sqrMagnitude <= 0.05f) return;
        inertialDir.Normalize();
        lastMoveDirection = inertialDir;

        Quaternion targetRot = Quaternion.LookRotation(lastMoveDirection);

        // Aplicamos WeightFactor para hacer rotación más lenta si el objeto pesa más
        float adjustedRotationSpeed = rotationSpeed * currentWeightSpeedMultiplier;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, adjustedRotationSpeed * Time.deltaTime);
    }

    void HandleFreeRotation()
    {
        Vector2 lookInput = InputManager.LookInput;
        if (lookInput.sqrMagnitude < 0.001f) return;

        Vector3 inputDir = new Vector3(lookInput.x, 0f, lookInput.y).normalized;
        lastMoveDirection = inputDir;

        Quaternion targetRot = Quaternion.LookRotation(lastMoveDirection);

        // También afectamos free rotation con peso
        float adjustedFreeRotationSpeed = freeRotationSensitivity * 100f * currentWeightSpeedMultiplier;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, adjustedFreeRotationSpeed * Time.deltaTime);
    }

    public void LockRotation() => rotationLocked = true;
    public void UnlockRotation() => rotationLocked = false;
    public void EnableFreeRotation(bool value) => freeRotation = value;
    #endregion

    #region Input Callbacks
    void OnMoveChanged(Vector2 input) => inputMovement = input;
    void OnRunChanged(bool runState) => isRunning = runState;
    void OnCrouchChanged(bool crouchState)
    {
        if (crouchLocked) return;
        isCrouching = crouchState;
        animatorManager?.SetCrouching(isCrouching);
    }
    #endregion

    #region Weight Handling
    void OnPickableEquipped(PickableBehaviour p)
    {
        if (p == null)
        {
            currentEquipWeight = 1f;
            RecalculateWeightMultiplier();
            return;
        }

        // Obtenemos el peso directamente del Pickable
        currentEquipWeight = Mathf.Max(1f, p.Weight);
        RecalculateWeightMultiplier();
    }

    void OnPickableDropped(PickableBehaviour p)
    {
        currentEquipWeight = 1f;
        RecalculateWeightMultiplier();
    }

    void RecalculateWeightMultiplier()
    {
        if (currentEquipWeight <= 1f)
        {
            currentWeightSpeedMultiplier = 1f;
            return;
        }

        // Penalización proporcional al peso
        float penalty = (currentEquipWeight - 1f) * weightSpeedSensitivity;
        currentWeightSpeedMultiplier = Mathf.Clamp(1f - penalty, weightAccelerationSensitivity, 1f);
    }

    public void SetDraggedWeight(float weight)
    {
        currentEquipWeight = weight;
        RecalculateWeightMultiplier();
    }
    #endregion

    #region External Forces API
    public void AddExternalImpulse(Vector3 impulse)
    {
        externalVelocity += impulse;
    }
    #endregion

    #region Animator
    void UpdateAnimatorVelocity()
    {
        if (animatorManager == null) return;

        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = planarVelocity.magnitude;

        animatorManager.SetVelocity(speed);
        animatorManager.SetMovementRatio(speed / walkSpeed);
    }
    #endregion
}
