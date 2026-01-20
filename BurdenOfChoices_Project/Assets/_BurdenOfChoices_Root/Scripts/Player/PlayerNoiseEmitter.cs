using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    #region Inspector Variables
    [SerializeField] float walkNoiseInterval = 0.6f;

    [SerializeField] float walkNoiseDelay = 0.25f;
    [SerializeField] float runNoiseDelay = 0.15f;

    [SerializeField] float minSpeedToEmit = 0.2f;
    #endregion

    #region Internal States
    float noiseTimer;
    #endregion

    #region References
    PlayerController controller;
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (controller.IsCrouching) return;

        Vector3 planarVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float speed = planarVelocity.magnitude;

        if (speed < minSpeedToEmit) return;

        bool isRunning = speed > 5.5f; //coherente con tus speeds
        float interval = isRunning ? runNoiseDelay : walkNoiseInterval;
        float delay = isRunning ? runNoiseDelay : walkNoiseDelay;

        noiseTimer += Time.deltaTime;
        if(noiseTimer >= interval)
        {
            noiseTimer = 0f;
            EmitNoise(delay);
        }
    }

    void EmitNoise(float delay)
    {
        NoiseEvents.OnNoiseEmitted?.Invoke(new NoiseEvent(transform.position, delay));
    }
}
