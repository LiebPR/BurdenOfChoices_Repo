using UnityEngine;
using System;

public class GameTimeSequenceStep : MonoBehaviour, ISequenceStep
{
    public enum TimeAction
    {
        Pause,
        Resume
    }

    [SerializeField] TimeAction action;

    public void Play(Action onFinished)
    {
        if (GameStopManager.Instance == null)
        {
            onFinished?.Invoke();
            return;
        }

        switch (action)
        {
            case TimeAction.Pause:
                GameStopManager.Instance.PauseGame();
                break;

            case TimeAction.Resume:
                GameStopManager.Instance.ResumeGame();
                break;
        }

        onFinished?.Invoke();
    }
}
