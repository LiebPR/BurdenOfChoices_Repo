using UnityEngine;

public struct NoiseEvent
{
    public Vector3 position;
    public float delay;

    public NoiseEvent(Vector3 position, float delay)
    {
        this.position = position;
        this.delay = delay;
    }
}
