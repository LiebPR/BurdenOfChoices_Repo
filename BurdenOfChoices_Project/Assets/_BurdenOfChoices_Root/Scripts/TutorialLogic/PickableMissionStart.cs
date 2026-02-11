using UnityEngine;

public class PickableMissionStart : MonoBehaviour
{
    #region Inspector
    [SerializeField] UITutorialMenu tutorialMenu;
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

        // Iniciar diálogo de entrada
        if (dialogSystem && entryDialog)
        {
            dialogSystem.StartDialog(entryDialog, () =>
            {
                // Mostrar tutorial al terminar el diálogo
                if (tutorialMenu != null)
                    tutorialMenu.Show("R.CLICK - PICK", null);

                // Iniciar la misión
                targetMission.StartMission();
            });
        }

        targetMission.StartMission();
    }
}
