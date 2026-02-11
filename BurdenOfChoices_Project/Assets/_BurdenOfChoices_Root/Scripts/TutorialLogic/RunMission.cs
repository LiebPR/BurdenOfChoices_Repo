using System;
using System.Collections;
using UnityEngine;

public class RunMission : MonoBehaviour, IMissionStep
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    [SerializeField] DialogData completeDialog;
    [SerializeField] float requiredRunTime = 2f;
    [SerializeField] float delayBeforeStart = 1f;
    #endregion

    bool isRunning;
    bool active;
    bool isCompleted;

    float runTimer;

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
        {
            dialogSystem.StartDialog(entryDialog, () =>
            {
                if (tutorialMenu != null)
                    tutorialMenu.Show("CTRL - RUN", null);
            });
        }

        runTimer = 0f;
        active = true;

        InputManager.OnRunChanged += OnRunChanged;
    }

    void Update()
    {
        if (!active || isCompleted || !isRunning) return;

        runTimer += Time.deltaTime;

        if (runTimer >= requiredRunTime)
            CompleteMission();
    }

    void OnRunChanged(bool running)
    {
        isRunning = running;
        if (!running) runTimer = 0f;
    }

    void CompleteMission()
    {
        isCompleted = true;
        active = false;

        InputManager.OnRunChanged -= OnRunChanged;

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
        InputManager.OnRunChanged -= OnRunChanged;
    }
}
