using UnityEngine;
using UnityEngine.InputSystem.XR;

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
    [SerializeField] float minSpeedForIntent = 0.2f; //input domina cuando la velocidad es baja
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
        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float speed = planarVelocity.magnitude;

        //Dirección natural basada en velocidad
        Vector3 velocityDir = speed > minSpeedForRotation ? planarVelocity.normalized : Vector3.zero;

        //Dirección basada en la intención del input
        Vector3 inputDir = new Vector3(inputMovement.x, 0, inputMovement.y).normalized;

        //Selección inteligente entre velocidad o intención
        Vector3 targetDir = lastMoveDirection;

        if(speed > minSpeedForIntent)
        {
            //Cuando hay movimiento claro se usa la dirección real
            targetDir = velocityDir;
        }
        else if(inputDir.sqrMagnitude > 0.1f)
        {
            //El jugador ya no se mueve mucho pero aún "quiere" ir hacia una dirección
            targetDir = inputDir;
        }

        //Inercia (amortiguación del cambio), humaniza al player
        targetDir = Vector3.Slerp(lastMoveDirection, targetDir, 1f - inertiaFactor);

        //Micro impredecibilidad en la rotación del jugador
        if(speed > 0.1f)
        {
            targetDir += new Vector3(Random.Range(-rotationNoise, rotationNoise), 0, Random.Range(-rotationNoise, rotationNoise));
        }

        //Noramlizamos
        targetDir.Normalize();

        //Guardamos última dirección válida
        if(targetDir.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = targetDir;
        }

        //Aplicamos rotación suave
        if(lastMoveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Ipunts Callbacks
    void OnMoveChanged(Vector2 input) => inputMovement = input;
    void OnRunChanged(bool runState) => isRuning = runState;
    void OnCrouchChanged(bool crouchState) => isCrouching = crouchState;
    #endregion
}
