using System;
using System.Collections;
using UnityEngine;

public class MoveMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    [SerializeField] DialogData completeDialog;
    [SerializeField] Transform player;
    [SerializeField] float delayBeforeStart = 1f;
    [SerializeField] float movementThreshold = 0.1f;
    #endregion

    bool forward, back, left, right;
    bool active;
    bool isCompleted;

    Vector3 lastPos;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    public void StartMission()
    {
        if (isCompleted) return;
        StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        if (dialogSystem && entryDialog)
            dialogSystem.StartDialog(entryDialog);

        lastPos = player.position;
        active = true;
    }

    void Update()
    {
        if (!active || isCompleted) return;

        Vector3 delta = player.position - lastPos;

        if (!forward && delta.z > movementThreshold) forward = true;
        if (!back && delta.z < -movementThreshold) back = true;
        if (!right && delta.x > movementThreshold) right = true;
        if (!left && delta.x < -movementThreshold) left = true;

        lastPos = player.position;

        if (forward && back && left && right)
            CompleteMission();
    }

    void CompleteMission()
    {
        isCompleted = true;
        active = false;

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
}
