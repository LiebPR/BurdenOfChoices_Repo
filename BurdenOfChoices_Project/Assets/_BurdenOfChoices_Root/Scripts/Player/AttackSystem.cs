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
    private void HandleAttack()
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
            // ---- Animación del arma ----
            BaseWeaponHandler weaponHandler = weapon as BaseWeaponHandler;
            weaponHandler?.PlayWeaponAttack();

            // ---- Ataque con delay ----
            StartCoroutine(DelayedAttack(weapon, data));

            // ---- Cooldown ----
            StartCoroutine(AttackRoutine(data));
        }
        else
        {
            // Si no hay arma, aplicar cooldown por defecto
            StartCoroutine(AttackRoutine(new WeaponData { cooldown = 0.5f }));
        }
    }
    #endregion

    #region Coroutines
    /// <summary>
    /// Ejecuta el daño y knockback después de attackDelay
    /// </summary>
    private IEnumerator DelayedAttack(IWeapon weapon, WeaponData data)
    {
        yield return new WaitForSeconds(data.attackdelay);

        AttackCommand command = CreateCommand(weapon, data);
        command?.Execute(); // Aquí se aplicará daño y knockback
    }

    /// <summary>
    /// Controla el cooldown entre ataques
    /// </summary>
    private IEnumerator AttackRoutine(WeaponData data)
    {
        onCooldown = true;

        yield return new WaitForSeconds(data.cooldown);

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
