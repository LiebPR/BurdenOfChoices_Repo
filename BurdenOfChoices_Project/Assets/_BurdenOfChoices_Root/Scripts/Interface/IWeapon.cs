using UnityEngine;

public interface IWeapon
{
    WeaponData GetWeaponData();
    Transform GetAttackOrigin();
}
