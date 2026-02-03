using UnityEngine;
using System;

public class AnimationFreezeManager : MonoBehaviour
{
    public static AnimationFreezeManager Instance { get; private set; }

    public event Action OnFreeze;   // Se dispara cuando se congela
    public event Action OnRestore;  // Se dispara cuando se restaura

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #region Public API

    public void FreezeAnimator(Animator animator)
    {
        if (animator == null) return;
        animator.speed = 0f;
        OnFreeze?.Invoke();
    }

    public void RestoreAnimator(Animator animator)
    {
        if (animator == null) return;
        animator.speed = 1f;
        OnRestore?.Invoke();
    }

    public void FreezeAnimators(params Animator[] animators)
    {
        for (int i = 0; i < animators.Length; i++)
            FreezeAnimator(animators[i]);
    }

    public void RestoreAnimators(params Animator[] animators)
    {
        for (int i = 0; i < animators.Length; i++)
            RestoreAnimator(animators[i]);
    }

    #endregion
}
