using UnityEngine;
using UnityEngine.InputSystem.XR;

/// <summary>
/// PlayerController: Contiene las logicas de movimiento del jugador con suavizado de velocidad completo. 
/// Acelera y desacelera suavemente en todas las transiciones. Aparte controla la logica de agacharse por animación.
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region General Variables
    [Header("Movement")]
    [SerializeField] float walkSpeed = 5f; //velocidad al caminar
    [SerializeField] float runSpeed = 8f; //velocidad al correr
    [SerializeField] float crouchSpeed = 2.5f; //velocidad agachado
    [SerializeField] float accelerationTime = 0.2f; //timepo para acelerar
    [SerializeField] float decelerationTime = 0.3f; //tiempo para desacelerar
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
    bool lastCrouchState;
    int hashIsCrouching;
    #endregion

    #region References
    Rigidbody rb;
    Animator animator;
    #endregion

    private void Awake()
    {
        //REFERENCES:
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        //Caches de hashes
        hashIsCrouching = Animator.StringToHash("isCrouching");

        lastCrouchState = false; //evitar trigger inicial
    }

    #region Input Event Subscriptions
    private void OnEnable()
    {
        //Subscripción a eventos del InputManager
        InputManager.OnMoveChanged += OnMoveChanged;
        InputManager.OnRunChanged += OnRunChanged;
        InputManager.OnCrouchChanged += OnCrouchChanged;

        //Asegurar que el animator empieza en un estado coherente
        animator.SetBool(hashIsCrouching, false);
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

    private void Update()
    {
        HandleCrouchAnimation();
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

    #region Crouch Animation
    void HandleCrouchAnimation()
    {
        if (animator == null) return;
        if (lastCrouchState == isCrouching) return;

        if (isCrouching)
        {
            animator.SetBool(hashIsCrouching, true);
        }
        else
        {
            animator.SetBool(hashIsCrouching, false);
        }

        lastCrouchState = isCrouching;
    }
    #endregion

    #region Ipunts Callbacks
    void OnMoveChanged(Vector2 input) => inputMovement = input;
    void OnRunChanged(bool runState) => isRuning = runState;
    void OnCrouchChanged(bool crouchState) => isCrouching = crouchState;
    #endregion
}
