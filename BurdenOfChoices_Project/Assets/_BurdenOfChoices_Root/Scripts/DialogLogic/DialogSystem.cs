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
    public event Action OnDialogFinished;
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

    public void StartDialog(DialogData dialog, Action onFinished)
    {
        StartDialog(dialog); // Llama al método original

        // Suscribirse a un evento interno que se dispara al terminar el diálogo
        OnDialogFinished += onFinished;
    }

    public void NextLine()
    {
        if (!isActive) return;

        // Si el texto aún se está escribiendo, lo completamos y NO avanzamos
        if (dialogUI.IsTyping)
        {
            dialogUI.SkipTyping();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentDialog.lines.Count)
        {
            EndDialog();
            return;
        }

        ShowLine();
    }

    /// <summary>
    /// Llama cuando se presiona el botón de avance/skip.
    /// Completa la línea si está escribiendo, o avanza si ya terminó.
    /// </summary>
    public void SkipOrNext()
    {
        if (!isActive) return;

        if (dialogUI.IsTyping)
        {
            dialogUI.SkipTyping(); // Completa la línea
        }
        else
        {
            NextLine(); // Avanza a la siguiente línea
        }
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

        dialogUI.SetText(currentDialog.lines[currentLineIndex], currentDialog.typeSpeed);

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

        OnDialogFinished?.Invoke();
        OnDialogFinished = null;
    }
    #endregion

    #region Auto Advance
    IEnumerator AutoAdvanceRoutine()
    {
        while (isActive)
        {
            // Espera a que termine la línea actual
            while (dialogUI.IsTyping)
                yield return null;

            // Espera el delay antes de avanzar
            yield return new WaitForSeconds(currentDialog.autoAdvanceDelay);

            NextLine();
        }
    }
    #endregion
}
