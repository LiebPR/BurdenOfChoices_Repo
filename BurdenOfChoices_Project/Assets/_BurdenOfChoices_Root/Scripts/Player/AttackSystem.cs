using System.Collections;
using UnityEngine;

/// <summary>
/// AttackSystem
/// Gestiona la ejecución de ataques según el arma equipada.
/// Controla cooldown y creación de comandos de ataque.
/// </summary>
public class AttackSystem : MonoBehaviour
{
    #region References
    PickSystem pickSystem; //Para obtener el arma actualmente equipada
    AnimatorManager animatorManager;
    PlayerController playerController;
    #endregion

    #region Internal States
    bool onCooldown; //Bloqueo temporal entre ataques
    #endregion

    private void Awake()
    {
        pickSystem = GetComponent<PickSystem>();
        animatorManager = GetComponent<AnimatorManager>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        InputManager.OnAttack += HandleAttack;
    }

    private void OnDisable()
    {
        InputManager.OnAttack -= HandleAttack;
    }

    #region Handles
    /// <summary>
    /// Evento de ataque, crea y ejecuta comando según el arma.
    /// </summary>
    void HandleAttack()
    {
        if (onCooldown) return;

        PickableBehaviour pickable = pickSystem.GetCurrentPickable();
        if (pickable == null) return;

        IWeapon weapon = pickable.GetComponent<IWeapon>();

        WeaponData data = weapon != null ? weapon.GetWeaponData() : null;

        // ---- Animación del jugador ----
        // Mantener exactamente como estaba
        float slashingValue = (data != null && data.attackType == WeaponAttackType.Slash) ? 1f : 0f;
        animatorManager.PlayAttack(slashingValue);

        if (weapon != null && data != null)
        {
            // Ejecutar comando de ataque (daño, área, etc.)
            AttackCommand command = CreateCommand(weapon, data);
            command?.Execute();

            // ---- Animación del arma ----
            BaseWeaponHandler weaponHandler = weapon as BaseWeaponHandler;
            weaponHandler?.PlayWeaponAttack();

            StartCoroutine(AttackRoutine(weapon, data));
        }
        else
        {
            // Si no hay arma, aún aplicamos cooldown
            StartCoroutine(AttackRoutine(null, null));
        }
    }
    #endregion

    #region Routine
    IEnumerator AttackRoutine(IWeapon weapon, WeaponData data)
    {
        onCooldown = true; 

        yield return new WaitForSeconds(data.cooldown); //espera el tiempo de cooldown
        onCooldown = false;
    }
    #endregion

    /// <summary>
    /// Fabrica el comando correspondiente a Slash o Stab.
    /// </summary>
    AttackCommand CreateCommand(IWeapon weapon, WeaponData data)
    {
        switch (data.attackType)
        {
            case WeaponAttackType.Slash:
                return new SlashAttackCommand(data, weapon.GetAttackOrigin());

            case WeaponAttackType.Stab:
                return new StabAttackCommand(data, weapon.GetAttackOrigin());
        }

        return null;
    }
}
