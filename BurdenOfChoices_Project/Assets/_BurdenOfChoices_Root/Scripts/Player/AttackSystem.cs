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
        if(onCooldown) return;

        PickableBehaviour pickable = pickSystem.GetCurrentPickable();
        if (pickable == null) return;

        IWeapon weapon = pickable.GetComponent<IWeapon>();

        WeaponData data = weapon.GetWeaponData();
        if(data == null) return;

        // Crear y ejecutar comando según el tipo de ataque
        AttackCommand command = CreateCommand(weapon, data);
        if (command != null)
        {
            animatorManager.PlayAttack(data.attackType);
            command.Execute();
        }
            

        StartCoroutine(AttackRoutine(weapon, data));
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
