using System;
using UnityEngine;

/// <summary>
/// Misión de tutorial: se completa cuando la estatua cae sobre el pilar.
/// </summary>
public class StatueTutorialMission : MonoBehaviour, IMissionStep
{
    #region Inspector
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
            dialogSystem.StartDialog(entryDialog);

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

        // Desuscribir evento
        if (statue != null)
            statue.OnFallen -= OnStatueFallen;

        // Lanzar diálogo de finalización si existe
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
