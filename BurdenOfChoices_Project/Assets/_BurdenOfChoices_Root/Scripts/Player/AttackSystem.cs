using System.Collections;
using UnityEngine;

public class AttackSystem : MonoBehaviour
{
    #region References
    PickSystem pickSystem;
    #endregion

    #region Internal States
    bool onCooldown;
    #endregion

    private void Awake()
    {
        if(pickSystem == null)
            pickSystem = GetComponent<PickSystem>();
    }

    private void OnEnable()
    {
        InputManager.OnAttack += HandleAttack;
    }

    private void OnDisable()
    {
        InputManager.OnAttack -= HandleAttack;
    }

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
            command.Execute();

        StartCoroutine(AttackRoutine(weapon, data));
    }

    
    
    #region Routine
    IEnumerator AttackRoutine(IWeapon weapon, WeaponData data)
    {
        onCooldown = true; 

        yield return new WaitForSeconds(data.cooldown);
        onCooldown = false;
    }
    #endregion

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
