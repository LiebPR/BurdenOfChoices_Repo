using System;
using UnityEngine;

public class CrouchMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] TriggerNotifier startTrigger;
    [SerializeField] TriggerNotifier completeTrigger;
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    #endregion

    bool isCompleted;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        startTrigger.OnTriggerEntered += OnStart;
        completeTrigger.OnTriggerEntered += CompleteMission;
    }

    void OnStart()
    {
        startTrigger.OnTriggerEntered -= OnStart;

        if (dialogSystem && entryDialog)
            dialogSystem.StartDialog(entryDialog);
    }

    void CompleteMission()
    {
        if (isCompleted) return;

        isCompleted = true;
        completeTrigger.OnTriggerEntered -= CompleteMission;

        Debug.Log("[CrouchMission] Completada");
        OnMissionCompleted?.Invoke();
    }

    void OnDisable()
    {
        startTrigger.OnTriggerEntered -= OnStart;
        completeTrigger.OnTriggerEntered -= CompleteMission;
    }
}
