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

        // Mostrar diálogo de entrada
        if (dialogSystem && entryDialog)
        {
            dialogSystem.StartDialog(entryDialog, () =>
            {
                // Mostrar tutorial al terminar el diálogo de entrada
                if (tutorialMenu != null)
                    tutorialMenu.Show("Q - Throw", null);
            });
        }

        // Suscribirse al evento de la estatua
        if (statue != null)
            statue.OnFallen += OnStatueFallen;
    }

    private void OnStatueFallen(Statue fallenStatue)
    {
        if (isCompleted) return;

        CompleteMission();
    }

    private void CompleteMission()
    {
        isCompleted = true;

        if (statue != null)
            statue.OnFallen -= OnStatueFallen;

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

    private void OnDisable()
    {
        if (statue != null)
            statue.OnFallen -= OnStatueFallen;
    }
}
