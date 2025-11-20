using UnityEngine;

/// <summary>
/// PlayerController: Contiene las logicas de movimiento del jugador con suavizado de velocidad completo. 
/// Acelera y desacelera suavemente en todas las transiciones. Aparte controla la logica de agacharse por animación.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region General Variables
    [Header("Movement Config")]
    [SerializeField] float walkSpeed = 5f; //velocidad al caminar
    [SerializeField] float runSpeed = 8f; //velocidad al correr
    [SerializeField] float crouchSpeed = 2.5f; //velocidad agachado
    [SerializeField] float accelerationTime = 0.2f; //timepo para acelerar
    [SerializeField] float decelerationTime = 0.3f; //tiempo para desacelerar

    [Header("Rotation Config")]
    [SerializeField] float rotationSpeed = 10f; //suavizado de rotación
    [SerializeField] float minSpeedForRotation = 0.05f; //mínima velocidad para permitir rotación
    [SerializeField] float inertiaFactor = 0.15f; //amortiguación del cambio de dirección
    [SerializeField] float rotationNoise = 0.02f; //variación humana sutil
    #endregion

    #region Internal States
    //RUIDO:
    //bool ruido; //Estado interno que indica que el player hace ruido

    //MOVIMIENTO:
    Vector2 inputMovement; //entrada de movimiento
    Vector3 currentVelocitySmooth; //usado para SmoothDamp
    bool isRuning; //estado interno que indica que el player esta corriendo.
    
    //AGACHARSE:
    bool isCrouching; //estado interno que indica que el player esta agachado.

    //ROTACIÓN:
    Vector3 lastMoveDirection; //ultima dirección válida de movimiento
    #endregion

    #region Getters
    public bool IsCrouching => isCrouching;
    #endregion

    #region References
    Rigidbody rb;
    Animator animator;
    #endregion

    private void Awake()
    {
        //REFERENCES:
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleRotation();
    }

    #region Input Event Subscriptions
    private void OnEnable()
    {
        //Subscripción a eventos del InputManager
        InputManager.OnMoveChanged += OnMoveChanged;
        InputManager.OnRunChanged += OnRunChanged;
        InputManager.OnCrouchChanged += OnCrouchChanged;
    }

    private void OnDisable()
    {
        //Desubscripción
        InputManager.OnMoveChanged -= OnMoveChanged;
        InputManager.OnRunChanged -= OnRunChanged;
        InputManager.OnCrouchChanged -= OnCrouchChanged;

    }
    #endregion

    private void FixedUpdate()
    {
        HandleMovementSpeed();
    }

    #region Movement Logic
    void HandleMovementSpeed()
    {
        //Determina velocidad objetivo según estado
        float targetSpeed = walkSpeed;
        if (isCrouching) targetSpeed = crouchSpeed;
        else if(isRuning) targetSpeed = runSpeed;

        //Dirección de movimiento
        Vector3 inputDir = new Vector3(inputMovement.x, 0, inputMovement.y).normalized;
        Vector3 desiredVelocity = inputDir * targetSpeed;

        //Selecciona el tiempo de suavizado según si aceleramos o desaceleramos
        float smoothTime = (desiredVelocity.magnitude > new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude) ? accelerationTime : decelerationTime;

        //Suavizado de velocidad
        Vector3 smoothVelocity = Vector3.SmoothDamp(new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z), desiredVelocity, ref currentVelocitySmooth, smoothTime);

        //Aplicamos velocidad final manteniendo Y
        rb.linearVelocity = new Vector3(smoothVelocity.x, rb.linearVelocity.y, smoothVelocity.z);
    }
    #endregion

    #region Rotation Logic
    void HandleRotation()
    {
        //Velocidad planaer y magnitud
        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = planarVelocity.magnitude;

        //Si no hay movimiento (vel) significativo, no rotamos (mantener orientación actual)
        if (speed < minSpeedForRotation) return;

        //Dirección pura basada únicamente en la velocidad real
        Vector3 velocityDir = planarVelocity.normalized;

        float dynamicInertia = Mathf.Lerp(inertiaFactor, inertiaFactor * 1.5f, 1f - Mathf.Clamp01(speed)); //dinamizamos el valor de inerciaFactor.
        //Aplicar inercia: Entre la última dirección y la nueva dirección por velocidad
        Vector3 inertialDir = Vector3.Slerp(lastMoveDirection, velocityDir, 1f - dynamicInertia);

        //Añadir micro-ruido humanoide solo si estamo moviéndonos
        if(speed > 0.5f)
        {
            float noiseFactor = Mathf.Clamp01(speed / runSpeed);
            inertialDir += new Vector3(Random.Range(-rotationNoise, rotationNoise), 0f, Random.Range(-rotationNoise, rotationNoise));
        }

        //Normalizar y validar
        if (inertialDir.sqrMagnitude <= 0.05f) return;
        inertialDir.Normalize();

        //Guardar última dirección válida
        lastMoveDirection = inertialDir;

        //Aplicar rotación suave hacia la dirección resultante
        Quaternion targetRot = Quaternion.LookRotation(lastMoveDirection);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
    #endregion

    #region Ipunts Callbacks
    void OnMoveChanged(Vector2 input) => inputMovement = input;
    void OnRunChanged(bool runState) => isRuning = runState;
    void OnCrouchChanged(bool crouchState) => isCrouching = crouchState;
    #endregion
}
