using System;
using UnityEngine;

public class CrouchMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] TriggerNotifier startTrigger;
    [SerializeField] TriggerNotifier completeTrigger;

    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    [SerializeField] DialogData completeDialog;
    #endregion

    bool isCompleted;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        startTrigger.OnTriggerEntered += OnStart;
        completeTrigger.OnTriggerEntered += OnCompleteTrigger;
    }

    void OnStart()
    {
        startTrigger.OnTriggerEntered -= OnStart;

        if (dialogSystem && entryDialog)
            dialogSystem.StartDialog(entryDialog);
    }

    void OnCompleteTrigger()
    {
        if (isCompleted) return;

        isCompleted = true;
        completeTrigger.OnTriggerEntered -= OnCompleteTrigger;

        if (dialogSystem && completeDialog)
        {
            dialogSystem.StartDialog(completeDialog, () =>
            {
                OnMissionCompleted?.Invoke();
            });
        }
        else
        {
            OnMissionCompleted?.Invoke();
        }
    }

    void OnDisable()
    {
        startTrigger.OnTriggerEntered -= OnStart;
        completeTrigger.OnTriggerEntered -= OnCompleteTrigger;
    }
}
