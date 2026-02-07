using System;
using UnityEngine;

public class TutorialHitTarget : MonoBehaviour, IHittable
{
    public event Action OnHitReceived;

    public void OnHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        OnHitReceived?.Invoke();
    }
}