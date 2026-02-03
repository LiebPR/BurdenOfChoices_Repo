using System;
using UnityEngine;

public class DialogueSequenceStep : MonoBehaviour, ISequenceStep
{
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData data;

    public void Play(Action onFinished)
    {
        dialogSystem.onDialogFinished += onFinished;
        dialogSystem.StartDialog(data);
    }
}
