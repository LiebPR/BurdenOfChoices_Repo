using UnityEngine;

public class CellTriggerCutScene : MonoBehaviour
{
    [SerializeField] Cell cell;
    [SerializeField] SequenceController freedomSequenceController;
    [SerializeField] Animator bibboAnimator;

    #region Animator Parameters
    static readonly int IFreedomHash = Animator.StringToHash("IFreedom");
    #endregion

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
        bibboAnimator.SetBool(IFreedomHash, true);
    }
}
