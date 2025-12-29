using UnityEngine;

/// <summary>
/// Tipo de ataque posible para un arma.
/// </summary>
public enum WeaponAttackType
{
    Slash, //ataque radial
    Stab  //raycast recto
}

/// <summary>
/// WeaponData
/// ScriptableObject que contiene datos configurables de un arma.
/// </summary>
[CreateAssetMenu(fileName = "WeaponData", menuName = "Combat/WeaponData")]
public class WeaponData : ScriptableObject
{
    #region Config
    [Header("Attack")]
    [Tooltip("Define el tipo de ataque del arma.")]
    public WeaponAttackType attackType;
    public int damage = 1;
    public float range = 1.5f;
    public float knockBack = 5f;

    [Header("Timing")]
    [Tooltip("Tiempo de retraso antes de que el ataque se ejecute.")]
    public float attackdelay = 0.2f;
    public float cooldown = 0.5f;
    #endregion
}
