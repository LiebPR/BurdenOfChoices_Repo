using UnityEngine;

public abstract class AttackCommand
{
    protected WeaponData weaponData;
    protected Transform tOrigin;

    protected AttackCommand(WeaponData data, Transform origin)
    {
        weaponData = data;
        tOrigin = origin;
    }

    public abstract void Execute();
}
