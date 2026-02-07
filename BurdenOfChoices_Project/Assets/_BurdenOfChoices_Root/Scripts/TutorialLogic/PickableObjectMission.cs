using System;
using UnityEngine;

public class PickableObjectMission : MonoBehaviour, IMissionStep
{
    #region Inspector
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
