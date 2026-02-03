using UnityEngine;

public class CellTriggerCutScene : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Cell cell;
    [SerializeField] Animator bibboAnimator;
    [SerializeField] RepercusionRemorse remorseRepercus;

    [Header("Cineamatics Because Remorse")]
    [SerializeField] SequenceController sequenceRegular;
    [SerializeField] SequenceController sequenceMid;
    [SerializeField] SequenceController sequenceMax;

    #region Animator Parameters
    static readonly int IFreedomHash = Animator.StringToHash("IFreedom");
    #endregion
    
    bool hasPlayed;

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
        if (hasPlayed) return;
        hasPlayed = true;

        RemorseLevel level = remorseRepercus.GetRemorseLevel();

        if (level == RemorseLevel.Regular)
        {
            bibboAnimator.SetBool(IFreedomHash, true);
            sequenceRegular.Play();
        }
        else if (level == RemorseLevel.Mid)
        {
            bibboAnimator.SetBool(IFreedomHash, true);
            sequenceMid.Play();
        }
        else
        {
            bibboAnimator.SetBool(IFreedomHash, false);
            sequenceMax.Play();
        }
    }
}
