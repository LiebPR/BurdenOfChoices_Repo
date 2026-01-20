using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Sistema central de diálogos.
/// Gestiona estado, flujo y avance.
/// </summary>
public class DialogSystem : MonoBehaviour
{
    #region Inspector References
    [SerializeField] DialogUI dialogUI;
    [SerializeField] PlayerController playerController;
    #endregion

    #region State
    bool isActive;
    int currentLineIndex;
    DialogData currentDialog;
    #endregion

    #region Events
    public event Action onDialogFinished;
    #endregion

    #region Public API
    public void StartDialog(DialogData dialogData)
    {
        if(isActive || dialogData == null) return;

        currentDialog = dialogData;
        currentLineIndex = 0;
        isActive = true;

        // BLOQUEOS
        if (playerController != null)
        {
            if (currentDialog.blockPlayerController)
                playerController.PausePlayer();

            playerController.LockRotation(); // bloquea rotación
        }

        dialogUI.Show();
        dialogUI.SetSpeakerName(currentDialog.speakerName);
        ShowLine();

        if (currentDialog.autoAdvance)
            StartCoroutine(AutoAdvanceRoutine());
    }

    public void NextLine()
    {
        if (!isActive) return;

        currentLineIndex++;

        if(currentLineIndex >= currentDialog.lines.Count)
        {
            EndDialog();
            return;
        }
        ShowLine();
    }
    #endregion

    #region Core
    void ShowLine()
    {
        // Si está escribiendo, completamos de golpe
        if (dialogUI.IsTyping)
        {
            dialogUI.SkipTyping();
            return;
        }

        dialogUI.SetText(currentDialog.lines[currentLineIndex]);

        // Actualizar retrato con fade
        Sprite portrait = null;
        if (currentDialog.emotions != null && currentDialog.emotions.Count > currentLineIndex)
            portrait = currentDialog.emotions[currentLineIndex].portrait;

        dialogUI.SetPortrait(portrait); // fade de 0.3 segundos
    }

    void EndDialog()
    {
        StopAllCoroutines();

        if (playerController != null)
        {
            if (currentDialog != null && currentDialog.blockPlayerController)
                playerController.ResumePlayer();

            playerController.UnlockRotation(); // desbloquea rotación
        }

        isActive = false;
        currentDialog = null;

        dialogUI.Hide();

        onDialogFinished?.Invoke();
        onDialogFinished = null;
    }
    #endregion

    #region Auto Advance
    IEnumerator AutoAdvanceRoutine()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(currentDialog.autoAdvanceDelay);
            NextLine();
        }
    }
    #endregion
}
