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
    [Header("Puerta")]
    [SerializeField] Animator doorAnimator;
    [SerializeField] string openTrigger = "Open";
    [SerializeField] string idleTrigger = "Idle";
    [SerializeField] float openDuration = 1f;

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

        // Dialogo inicial
        if (dialogSystem != null && entryDialog != null)
            dialogSystem.StartDialog(entryDialog);
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

        OnMissionCompleted?.Invoke();
    }
    #endregion
}
