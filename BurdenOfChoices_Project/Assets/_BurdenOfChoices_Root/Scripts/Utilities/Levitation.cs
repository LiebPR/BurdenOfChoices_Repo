using UnityEngine;

public class Levitation : MonoBehaviour
{
    [Header("Amplitude")]
    [SerializeField] Vector2 verticalAmplitudeRange = new Vector2(0.15f, 0.35f);
    [SerializeField] Vector2 horizontalAmplitudeRange = new Vector2(0.05f, 0.15f);

    [Header("Frequency")]
    [SerializeField] Vector2 frequencyRange = new Vector2(0.3f, 0.6f);

    Vector3 startPosition;

    float verticalAmplitude;
    float horizontalAmplitude;
    float frequency;

    float phaseY;
    float phaseX;
    float phaseZ;

    void Start()
    {
        startPosition = transform.localPosition;

        // Randomización por instancia
        verticalAmplitude = Random.Range(verticalAmplitudeRange.x, verticalAmplitudeRange.y);
        horizontalAmplitude = Random.Range(horizontalAmplitudeRange.x, horizontalAmplitudeRange.y);
        frequency = Random.Range(frequencyRange.x, frequencyRange.y);

        // Fases independientes para romper simetría
        phaseY = Random.Range(0f, Mathf.PI * 2f);
        phaseX = Random.Range(0f, Mathf.PI * 2f);
        phaseZ = Random.Range(0f, Mathf.PI * 2f);
    }

    void Update()
    {
        float time = Time.time * frequency;

        float offsetY = Mathf.Sin(time + phaseY) * verticalAmplitude;
        float offsetX = Mathf.Sin(time * 0.5f + phaseX) * horizontalAmplitude;
        float offsetZ = Mathf.Sin(time * 0.6f + phaseZ) * horizontalAmplitude;

        transform.localPosition = startPosition + new Vector3(offsetX, offsetY, offsetZ);
    }
}
