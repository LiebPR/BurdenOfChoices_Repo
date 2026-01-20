using UnityEngine;
using System;

public class DialogTrigger : MonoBehaviour
{
    #region Inspector
    [SerializeField] DialogData dialogData;
    [SerializeField] DialogSystem dialogSystem;

    [Tooltip("Si es verdadero, solo se activa una vez")]
    [SerializeField] bool triggerOnce = false;

    [Tooltip("Si es verdadero, se repetirá hasta que la condición externa sea verdadera")]
    [SerializeField] bool conditionalRepeat = false;
    #endregion

    #region Internal
    bool hasBeenTriggered = false;

    /// <summary>
    /// Delegado externo para evaluar si la condición de repetición se cumple
    /// </summary>
    public Func<bool> ConditionMet;
    #endregion

    #region Trigger
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!CanTrigger()) return;

        dialogSystem.StartDialog(dialogData);
        hasBeenTriggered = true;
    }
    #endregion

    #region Logic
    public bool CanTrigger()
    {
        if (triggerOnce && hasBeenTriggered) return false;
        if (conditionalRepeat && ConditionMet != null && !ConditionMet()) return false;
        if (!triggerOnce) return true;

        return !hasBeenTriggered;
    }
    #endregion
}
