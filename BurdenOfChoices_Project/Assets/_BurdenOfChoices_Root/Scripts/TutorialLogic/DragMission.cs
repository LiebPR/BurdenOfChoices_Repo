using System;
using UnityEngine;

public class DragMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;

    [Header("References")]
    [SerializeField] DraggController dragController;
    [SerializeField] TriggerNotifier missionTrigger;

    [Header("Completion")]
    [SerializeField] float completionDistance = 0.1f;

    [Header("Dialog")]
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    [SerializeField] DialogData completeDialog;
    #endregion

    bool isCompleted;
    bool isActive;
    bool hasEnteredTrigger;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        isActive = true;
        hasEnteredTrigger = false;

        // Mostrar tutorial al terminar el diálogo de entrada
        if (tutorialMenu != null)
            tutorialMenu.Show("R.CLICK - DRAG", null);

        if (missionTrigger != null)
            missionTrigger.OnTriggerEntered += OnTriggerEntered;

        if (dialogSystem && entryDialog)
        {
            dialogSystem.StartDialog(entryDialog);
        }
    }

    void Update()
    {
        if (!isActive || isCompleted) return;
        if (!hasEnteredTrigger) return;
        if (dragController == null || !dragController.IsDragging) return;

        DraggableObject drag = dragController.CurrentDrag;
        if (drag == null || drag.CarrilB == null) return;

        float distanceToEnd = Vector3.Distance(
            drag.transform.position,
            drag.CarrilB.position
        );

        if (distanceToEnd <= completionDistance)
        {
            CompleteMission();
        }
    }

    void OnTriggerEntered()
    {
        hasEnteredTrigger = true;
    }

    void CompleteMission()
    {
        isCompleted = true;
        isActive = false;

        Cleanup();

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

    void Cleanup()
    {
        if (missionTrigger != null)
            missionTrigger.OnTriggerEntered -= OnTriggerEntered;
    }

    void OnDisable()
    {
        Cleanup();
    }
}
