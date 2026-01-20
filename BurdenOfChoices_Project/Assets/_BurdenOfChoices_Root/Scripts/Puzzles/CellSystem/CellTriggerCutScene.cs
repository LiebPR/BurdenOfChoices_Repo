using UnityEngine;

public class CellTriggerCutScene : MonoBehaviour
{
    [SerializeField] Cell cell;
    [SerializeField] SequenceController freedomSequenceController;

    private void OnEnable()
    {
        if (cell != null)
            cell.OnCellUnlocked += PlayCutscene;
    }

    private void OnDisable()
    {
        if (cell != null)
            cell.OnCellUnlocked -= PlayCutscene;
    }

    void PlayCutscene()
    {
        if (freedomSequenceController != null)
            freedomSequenceController.Play();
    }
}
