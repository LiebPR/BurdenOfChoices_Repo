using UnityEngine;

public class BaseWeaponHandler : MonoBehaviour, IPickListener, IWeapon
{
    #region Inspector States
    [Header("References")]
    [SerializeField] Animator anim;
    [SerializeField] WeaponData data;
    [SerializeField] Transform attackOrigin;
    #endregion

    #region References
    PickableBehaviour pickable;
    PlayerHand playerHand;
    DataProvider dataProvider;
    #endregion

    #region Animator Params
    static readonly int IsCatchHash = Animator.StringToHash("IsCatch");
    static readonly int IsAttackHash = Animator.StringToHash("IsAttack");
    #endregion

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        dataProvider = GetComponent<DataProvider>();

        playerHand = FindAnyObjectByType<PlayerHand>();
        if (playerHand == null)
        {
            Debug.LogWarning("No se encontró PlayerHand en la escena.");
        }
    }

    #region IWeapon
    public WeaponData GetWeaponData() => data;
    public Transform GetAttackOrigin() => attackOrigin;
    #endregion

    #region IPickListener
    public void OnPick(ICatcher catcher)
    {
        if (anim != null)
            anim.SetBool(IsCatchHash, true);
    }

    public void OnDrop()
    {
        if(anim != null)
            anim.SetBool(IsCatchHash, false);
    }
    #endregion

    public void PlayWeaponAttack()
    {
        if(anim != null)
        {
            anim.SetTrigger(IsAttackHash);
            AudioManager.Instance.PlaySFX2D(data.weaponSFX);
            AudioManager.Instance.PlaySFX2D("SFX_Grace_Attack1");
        }
    }
}
