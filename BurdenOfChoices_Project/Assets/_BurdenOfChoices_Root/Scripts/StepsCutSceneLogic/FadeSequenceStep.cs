using UnityEngine;

public class FadeSequenceStep : MonoBehaviour, ISequenceStep
{
    public enum FadeType { In, Out }

    [SerializeField] FadeType fadeType;

    public void Play(System.Action onFinished)
    {
        if (fadeType == FadeType.Out)
            FadeController.Instance.FadeOut();
        else
            FadeController.Instance.FadeIn();
    }
}

