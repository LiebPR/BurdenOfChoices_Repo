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

    [Header("Cooldown")]
    [SerializeField] float throwCooldown = 1.0f; //segundos entre lanzamientos

    [Header("Hold Settings")]
    [SerializeField] float holdSpeed = 0.5f; //velocidad de carga
    [SerializeField] float verticalThrowForce = 0.25f; //fuerza vertical aplicada al lanzar
    [SerializeField] float minThrowDistance = 2f; //fuerza mínima
    [SerializeField] float maxThrowDistance = 10f; //fuerza máxima

    [Header("PreviewPoint")]
    [SerializeField] float simulationAirResistance = 0.1f; //resistencia al aire usada en la preview
    [SerializeField] float previewHeightOffset = 0.05f; //altura por encima del suelo del preview
    [SerializeField] float throwFollowSpeed = 8f; //cuán ráìdo sigue la rotación del jugador
    #endregion

    #region Internal States
    float holdTime;
    bool isHolding;

    //Cooldown
    float cooldownEndTime;

    //Valores ajustados en tiempo de ejecución
    float currentWeight = 1f; 
    float weightFactor = 1f;
    float effectiveHoldSpeed;
    float effectiveMinThrowDistance;
    float effectiveMaxThrowDistance;
    float effectiveVerticalThrowForce;

    Vector3 throwDirectionSmoothed; //direccion suavizada para la previa
    #endregion

    #region References
    PickableBehaviour pickable;
    ThrowableBehaviour throwable;
    AnimatorManager animatorManager;
    #endregion

    private void Awake()
    {
        animatorManager = GetComponent<AnimatorManager>();
    }

    private void Start()
    {
        if (throwPreview != null)
        {
            throwPreview.gameObject.SetActive(false);
        }
        ResetEffectiveValues();    
    }

    #region Input Event Subscriptions
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
    #endregion

    private void Update()
    {
        if(isHolding && pickable != null)
        {
            if (IsInCooldown())
            {
                CancelHold();
                return;
            }
            holdTime += Time.deltaTime * effectiveHoldSpeed;
            holdTime = Mathf.Clamp01(holdTime); //siempre entre 0 y 1

            UpdateThrowPreview();
        }
    }

    void SetCurrentPickable(PickableBehaviour p)
    {
        pickable = p;
        throwable = p.GetComponent<ThrowableBehaviour>();

        //Intentamos obtener el EquipableData desde el componente EquipableItem
        var equipableItem = p.GetComponent<EquipableItem>();
        if(equipableItem != null && equipableItem.Data != null)
        {
            currentWeight = equipableItem.Data.weight;
        }
        else
        {
            currentWeight = 1f; //valor por defecto
        }

        //Calculamos el factor de peso.
        //Evitamos división por cer y limitamos entre 0.15 y 1 para que no sea 0.
        weightFactor = 1f / Mathf.Max(currentWeight, 0.1f);
        weightFactor = Mathf.Clamp(weightFactor, 0.15f, 1f);

        //Aplicamos el factor a las variables efectivas
        effectiveHoldSpeed = holdSpeed * weightFactor;
        effectiveMinThrowDistance = minThrowDistance * weightFactor;
        effectiveMaxThrowDistance = maxThrowDistance * weightFactor;
        effectiveVerticalThrowForce = verticalThrowForce * weightFactor;
    }

    void ClearCurrentPickable(PickableBehaviour p)
    {
        if (pickable == p)
        {
            pickable = null;
            throwable = null;

            if (throwPreview != null)
                throwPreview.gameObject.SetActive(false);

            //Restauramos valores por defecto
            ResetEffectiveValues();

            if (isHolding)
            {
                isHolding = false;
                animatorManager.EndHold();
            }

            //Restauramos movimiento y agachado
            var playerController = GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.ResumePlayer();//desbloqueamos movimiento
                playerController.LockCrouch(); //desbloqueamos agachar
                playerController.EnableFreeRotation(false); //vuelvea rotación normal en movimiento 
            }
        }
    }

    void StartHold()
    {
        if(IsInCooldown()) return;
        if (pickable == null || !pickable.IsCatched) return;

        animatorManager.StartHold();
        isHolding = true;
        holdTime = 0f;

        var playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.PausePlayer();
            playerController.LockCrouch();
            playerController.EnableFreeRotation(true);
        }

        // Inicializamos la dirección suavizada al inicio del hold
        throwDirectionSmoothed = throwDirectionSource.forward;

        if (throwPreview != null && throwDirectionSource != null)
        {
            throwPreview.gameObject.SetActive(true);

            float previewForce = effectiveMinThrowDistance;
            Vector3 predicted = PredictLandingPoint(throwDirectionSource.position, throwDirectionSmoothed, previewForce, effectiveVerticalThrowForce);
            throwPreview.position = predicted;
        }
    }

    void CancelHold()
    {
        isHolding = false;
        holdTime = 0f;

        if(throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        animatorManager.EndHold();

        var playerController = GetComponent<PlayerController>();
        if(playerController != null)
        {
            playerController.ResumePlayer();
            playerController.LockCrouch();
            playerController.EnableFreeRotation(false);
        }
    }

    void ReleaseThrow()
    {
        if (!isHolding) return;

        if (IsInCooldown())
        {
            CancelHold();
            return;
        }

        isHolding = false;
        animatorManager.TriggerThrow();

        if(throwPreview != null)
            throwPreview.gameObject.SetActive(false);

        //Restauramos movimiento y agachado
        var playerController = GetComponent<PlayerController>();
        if(playerController != null)
        {
            playerController.ResumePlayer();//desbloqueamos movimiento
            playerController.LockCrouch(); //desbloqueamos agachar
            playerController.EnableFreeRotation(false); //vuelvea rotación normal en movimiento 
        }
        //Guardamos la fuerza final, pero NO lanzamos aquí
        holdTime = Mathf.Clamp01(holdTime);
    }

    public void ExecuteThrow()
    {
        if (pickable == null || throwable == null) return;

        float throwDistance = Mathf.Lerp(effectiveMinThrowDistance, effectiveMaxThrowDistance, holdTime);

        // Usamos la dirección suavizada en lugar de la dirección directa
        throwable.OnThrow(throwDirectionSmoothed, throwDistance, effectiveVerticalThrowForce);

        StartCooldown();
        holdTime = 0f;
    }

    #region Cooldown Logic
    bool IsInCooldown()
    {
        return Time.time < cooldownEndTime;
    }

    void StartCooldown()
    {
        cooldownEndTime = Time.time + throwCooldown;
    }
    #endregion

    #region Preview Hold Methods
    void UpdateThrowPreview()
    {
        if (throwPreview == null || throwDirectionSource == null) return;

        float currentForce = Mathf.Lerp(effectiveMinThrowDistance, effectiveMaxThrowDistance, holdTime);
        Vector3 origin = throwDirectionSource.position;

        // Suavizamos la dirección hacia la actual del jugador
        throwDirectionSmoothed = Vector3.Slerp(throwDirectionSmoothed, throwDirectionSource.forward, throwFollowSpeed * Time.deltaTime);

        // Calculamos la predicción usando la dirección suavizada
        Vector3 predicted = PredictLandingPoint(origin, throwDirectionSmoothed, currentForce, effectiveVerticalThrowForce);
        throwPreview.position = predicted;

        if (!throwPreview.gameObject.activeSelf)
            throwPreview.gameObject.SetActive(true);
    }

    //Predice el punto de impacto de un impulso aplicado en 'origin' en la dirección y componente vertical indicadas.
    Vector3 PredictLandingPoint(Vector3 origin, Vector3 direction, float forceImpulse, float verticalImpulse)
    {
        // Usamos la dirección tal cual (incluye su componente Y) para reflejar exactamente
        Vector3 dirNormalized = (direction.sqrMagnitude > 0.0001f) ? direction.normalized : Vector3.forward;

        // Determinar masa para convertir impulso a velocidad inicial
        float mass = 1f;
        if (pickable != null && pickable.rb != null)
        {
            mass = Mathf.Max(0.0001f, pickable.rb.mass);
        }

        // Usar simulationAirResistance en lugar de Rigidbody.drag (evita uso de propiedad obsoleta)
        float simDrag = Mathf.Max(0f, simulationAirResistance);

        // Calculamos el impulso exactamente como en el lanzamiento real
        Vector3 impulse = dirNormalized * forceImpulse + Vector3.up * verticalImpulse;
        Vector3 velocity = impulse / mass;

        // Simulación discreta
        float dt = 0.02f; // paso de simulación
        float maxSimTime = 5f; // tiempo máximo a simular
        Vector3 pos = origin;

        for (float t = 0f; t < maxSimTime; t += dt)
        {
            // Integración velocidad con gravedad
            velocity += Physics.gravity * dt;

            // Aproximación simple del efecto de resistencia del aire
            if (simDrag > 0f)
            {
                velocity *= Mathf.Exp(-simDrag * dt);
            }

            Vector3 nextPos = pos + velocity * dt;

            // Raycast entre pos y nextPos para detectar colisión con el mundo
            Vector3 segment = nextPos - pos;
            float segmentLength = segment.magnitude;
            if (segmentLength > 0f)
            {
                RaycastHit hit;
                if(Physics.Raycast(pos, segment.normalized, out hit, segmentLength + 0.001f))
                {
                    return hit.point + Vector3.up * previewHeightOffset; //ajustamos un poco hacia arriba
                }
            }

            pos = nextPos;

            // Si hemos caído muy por debajo del origen y no hay colisión, cortamos
            if (pos.y < origin.y - 50f) break;
        }

        // Si no colisionamos durante la simulación, hacemos un raycast hacia abajo desde la última posición simulada
        RaycastHit groundHit;
        Vector3 rayStart = pos + Vector3.up * 2f;
        if (Physics.Raycast(rayStart, Vector3.down, out groundHit, 100f))
        {
            return groundHit.point;
        }

        // Fallback: devolver la última posición simulada manteniendo la altura de origen
        pos.y = origin.y;
        return pos;
    }
    #endregion

    void ResetEffectiveValues()
    {
        currentWeight = 1f;
        weightFactor = 1f;
        effectiveHoldSpeed = holdSpeed;
        effectiveMinThrowDistance = minThrowDistance;
        effectiveMaxThrowDistance = maxThrowDistance;
        effectiveVerticalThrowForce = verticalThrowForce;
    }
}
