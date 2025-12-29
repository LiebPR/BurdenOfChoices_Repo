using UnityEngine;

/// <summary>
/// AttackCommand
/// Clase abstarcta para los comandos de ataque.
/// Contiene referencia a datos de arma y origen del ataque.
/// </summary>
public abstract class AttackCommand
{
    protected WeaponData weaponData; //datos del arma
    protected Transform tOrigin; //origen del ataque

    protected AttackCommand(WeaponData data, Transform origin)
    {
        weaponData = data;
        tOrigin = origin;
    }

    /// <summary>
    /// Metodo que ejecuta el ataque.
    /// </summary>
    public abstract void Execute();
}
