using System;
using System.Collections;
using UnityEngine;

public class AttackMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;

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
        {
            dialogSystem.StartDialog(entryDialog, () =>
            {
                // Mostrar tutorial al terminar el diálogo
                if (tutorialMenu != null)
                    tutorialMenu.Show("RIGHT CLICK + LEFT CLICK - Attack", null);
            });
        }

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

        // Apagar tutorial
        if (tutorialMenu != null)
            tutorialMenu.Hide();

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
