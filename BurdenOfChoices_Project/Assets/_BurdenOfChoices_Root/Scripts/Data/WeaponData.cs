using UnityEngine;

public enum WeaponAttackType
{
    Slash, //ataque radial
    Stab  //raycast recto
}

[CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/WeaponData")]
public class WeaponData : ScriptableObject
{
    #region Config
    [Header("Attack")]
    public WeaponAttackType attackType;
    public int damage = 1;
    public float range = 1.5f;
    public float knockBack = 5f;

    [Header("Timing")]
    public float attackdelay = 0.2f;
    public float cooldown = 0.5f;
    #endregion
}
