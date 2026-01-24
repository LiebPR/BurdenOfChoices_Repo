using UnityEngine;

public class FadeSequenceStep : MonoBehaviour, ISequenceStep
{
    public enum FadeType { In, Out }

    [SerializeField] FadeType fadeType;
    [SerializeField] float duration = -1f;

    public void Play(System.Action onFinished)
    {
        if (fadeType == FadeType.Out)
            FadeController.Instance.FadeOut(onFinished, duration);
        else
            FadeController.Instance.FadeIn(onFinished, duration);
    }
}

