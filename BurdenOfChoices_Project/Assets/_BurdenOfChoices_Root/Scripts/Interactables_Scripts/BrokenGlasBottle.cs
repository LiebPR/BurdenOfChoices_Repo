using UnityEngine;

public class BrokenGlasBottle : MonoBehaviour, IPickListener, IWeapon
{
    [SerializeField] WeaponData data;
    [SerializeField] Transform attackOrigin;

    #region IWeapon
    public WeaponData GetWeaponData() => data;
    public Transform GetAttackOrigin() => attackOrigin;
    #endregion

    #region IPickListener
    public void OnPick(ICatcher catcher)
    {

    }

    public void OnDrop()
    {

    }
    #endregion
}
