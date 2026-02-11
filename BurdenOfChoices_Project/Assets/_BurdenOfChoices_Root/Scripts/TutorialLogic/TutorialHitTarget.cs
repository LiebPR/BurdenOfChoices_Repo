using System;
using UnityEngine;

public class TutorialHitTarget : MonoBehaviour, IHittable
{
    [SerializeField] Animator enemyBustAnim;

    public event Action OnHitReceived;
    static readonly int OnHitHash = Animator.StringToHash("OnHit");

    public void OnHit(Vector3 hitPoint, Vector3 hitDirection)
    {
        enemyBustAnim.SetTrigger(OnHitHash);
        OnHitReceived?.Invoke();
        AudioManager.Instance.PlaySFX2D("SFX_EnemyBust_Hit");
    }
}