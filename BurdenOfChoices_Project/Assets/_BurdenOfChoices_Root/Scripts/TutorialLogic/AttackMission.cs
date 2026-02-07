using System;
using System.Collections;
using UnityEngine;

public class AttackMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] TriggerNotifier startTrigger;
    [SerializeField] TutorialHitTarget[] targets;

    [Header("Timing")]
    [SerializeField] float completeDelay = 0.5f;

    [Header("Diálogos")]
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    [SerializeField] DialogData completeDialog;
    #endregion

    bool hasStarted;
    bool isCompleted;
    bool hitRegistered;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        if (isCompleted) return;
        startTrigger.OnTriggerEntered += OnStart;
    }

    void OnStart()
    {
        if (hasStarted) return;
        hasStarted = true;

        startTrigger.OnTriggerEntered -= OnStart;

        if (dialogSystem && entryDialog)
            dialogSystem.StartDialog(entryDialog);

        foreach (var target in targets)
            target.OnHitReceived += OnTargetHit;
    }

    void OnTargetHit()
    {
        if (hitRegistered || isCompleted) return;

        hitRegistered = true;
        StartCoroutine(CompleteAfterDelay());
    }

    IEnumerator CompleteAfterDelay()
    {
        yield return new WaitForSeconds(completeDelay);

        isCompleted = true;

        foreach (var target in targets)
            target.OnHitReceived -= OnTargetHit;

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

        foreach (var target in targets)
            if (target != null)
                target.OnHitReceived -= OnTargetHit;
    }
}
