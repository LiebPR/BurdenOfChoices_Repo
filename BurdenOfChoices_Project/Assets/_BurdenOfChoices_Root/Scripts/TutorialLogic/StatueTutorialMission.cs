using System;
using UnityEngine;

/// <summary>
/// Misión de tutorial: se completa cuando la estatua cae sobre el pilar.
/// </summary>
public class StatueTutorialMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;

    [Header("Referencias del puzzle")]
    [SerializeField] Pillar pillar;         // Pilares del tutorial
    [SerializeField] Statue statue;         // Estatua asociada al pilar

    [Header("Diálogos")]
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    [SerializeField] DialogData completeDialog;
    #endregion

    bool hasStarted;
    bool isCompleted;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        if (hasStarted || isCompleted) return;
        hasStarted = true;

        if (tutorialMenu != null)
        {
            tutorialMenu.Show("Q - THROW", null);
        }

        if (statue != null)
        {
            statue.OnFallen += OnStatueFallen;
        }
        else
            Debug.LogError("Statue reference is null in the mission!");

        if (dialogSystem != null)
            dialogSystem.StartDialog(entryDialog);
    }

    private void OnStatueFallen(Statue fallenStatue)
    {
        if (isCompleted) return; // ignorar si ya completada
        CompleteMission();
    }

    private void CompleteMission()
    {
        isCompleted = true;

        if (statue != null)
            statue.OnFallen -= OnStatueFallen;

        if (tutorialMenu != null)
            tutorialMenu.Hide();

        if (dialogSystem != null && completeDialog != null)
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

    private void OnDisable()
    {
        if (statue != null)
            statue.OnFallen -= OnStatueFallen;
    }
}
