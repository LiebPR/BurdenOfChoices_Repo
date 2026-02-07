using UnityEngine;

public interface IHittable
{
    void OnHit(Vector3 hitPoint, Vector3 hitDirection);
}
