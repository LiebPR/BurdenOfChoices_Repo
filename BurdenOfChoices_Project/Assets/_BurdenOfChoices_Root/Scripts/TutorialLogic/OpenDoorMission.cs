using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

/// <summary>
/// Misión de abrir una puerta para tutorial.
/// Cuando el jugador interactúa con la puerta, se cambia de escena y se completa la misión.
/// </summary>
public class OpenDoorMission : MonoBehaviour, IMissionStep, IInteractable
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;

    [Header("Puerta")]
    [SerializeField] Animator doorAnimator;
    [SerializeField] string openTrigger = "Open";
    [SerializeField] string idleTrigger = "Idle";
    [SerializeField] float openDuration = 1f;
    [SerializeField] Light missionLight; // Luz que se enciende al iniciar

    [Header("Diálogos")]
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;

    [Header("Escena a cargar")]
    [SerializeField] string sceneToLoad;

    [Header("Bloqueo de interacción")]
    [SerializeField] bool startLocked = false;
    #endregion

    bool hasStarted;
    bool isCompleted;
    bool isInteracting;
    bool locked;

    public bool IsCompleted => isCompleted;
    public event Action OnMissionCompleted;

    private void Start()
    {
        locked = startLocked;
    }

    #region IMissionStep
    public void StartMission()
    {
        if (hasStarted || isCompleted) return;
        hasStarted = true;

        if (missionLight != null)
            missionLight.enabled = true;

        ShowTutorial();

        // Si no hay diálogo, mostrar tutorial inmediatamente
        if (dialogSystem != null && entryDialog != null)
        {
            dialogSystem.StartDialog(entryDialog);
        }
    }

    void ShowTutorial()
    {
        if (tutorialMenu != null)
            tutorialMenu.Show("F - INTERACT", null);
    }
    #endregion

    #region IInteractable
    public void OnPress()
    {
        if (isInteracting || locked) return;

        StartCoroutine(OpenDoorRoutine());
    }

    public void OnHighlight() { }
    public void OnRemoveHighlight() { }
    public void OnRelease() { }
    #endregion

    #region Door Logic
    IEnumerator OpenDoorRoutine()
    {
        isInteracting = true;

        // Abrir puerta
        if (doorAnimator != null)
            doorAnimator.SetTrigger(openTrigger);

        yield return new WaitForSeconds(openDuration);

        if (doorAnimator != null)
            doorAnimator.SetTrigger(idleTrigger);

        // Cargar nueva escena
        if (!string.IsNullOrEmpty(sceneToLoad))
            SceneController.Instance.LoadScene(sceneToLoad);

        CompleteMission();
    }
    #endregion

    #region Completion
    void CompleteMission()
    {
        if (isCompleted) return;
        isCompleted = true;

        // Apagar tutorial
        if (tutorialMenu != null)
            tutorialMenu.Hide();

        OnMissionCompleted?.Invoke();
    }
    #endregion
}
