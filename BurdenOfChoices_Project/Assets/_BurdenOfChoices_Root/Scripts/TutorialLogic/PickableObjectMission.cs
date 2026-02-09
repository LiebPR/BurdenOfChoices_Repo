using System;
using UnityEngine;

public class PickableObjectMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData completeDialog;
    #endregion

    bool isCompleted;
    bool isActive;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        if (isCompleted) return;
        isActive = true;
    }

    public void NotifyPicked(PickableMissionTarget target)
    {
        if (!isActive || isCompleted) return;

        isCompleted = true;
        isActive = false;

        // Apagar tutorial al completar
        if (tutorialMenu != null)
            tutorialMenu.Hide();

        // Diálogo final
        if (dialogSystem && completeDialog)
        {
            dialogSystem.StartDialog(completeDialog, () =>
            {
                Debug.Log("[PickableObjectMission] Misión completada (1 objeto)");
                OnMissionCompleted?.Invoke();
            });
        }
        else
        {
            OnMissionCompleted?.Invoke();
        }
    }
}
