using System;
using UnityEngine;

public class DialogueSequenceStep : MonoBehaviour, ISequenceStep
{
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData data;

    public void Play(Action onFinished)
    {
        dialogSystem.OnDialogFinished += onFinished;
        dialogSystem.StartDialog(data);
    }
}
