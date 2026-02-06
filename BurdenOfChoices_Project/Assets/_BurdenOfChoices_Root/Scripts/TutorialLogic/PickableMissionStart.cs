using UnityEngine;

public class PickableMissionStart : MonoBehaviour
{
    #region Inspector
    [SerializeField] PickableObjectMission targetMission;
    [SerializeField] DialogSystem dialogSystem;
    [SerializeField] DialogData entryDialog;
    #endregion

    bool hasStarted;

    private void OnTriggerEnter(Collider other)
    {
        if (hasStarted) return;
        if (!other.CompareTag("Player")) return;

        hasStarted = true;

        if (dialogSystem && entryDialog)
            dialogSystem.StartDialog(entryDialog);

        targetMission.StartMission();
    }
}
