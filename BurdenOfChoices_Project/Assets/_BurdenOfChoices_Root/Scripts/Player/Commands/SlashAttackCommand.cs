using UnityEngine;

public class SlashAttackCommand : AttackCommand
{
    public SlashAttackCommand(WeaponData data, Transform origin) : base(data, origin) { }

    public override void Execute()
    {
        Collider[] hits = Physics.OverlapSphere(tOrigin.position, weaponData.range);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 hitDirection = tOrigin.forward.normalized;

            EnemyHealth enemyHealth = hits[i].GetComponent<EnemyHealth>();
            if (enemyHealth == null) continue;

            enemyHealth.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack);
        }
    }
}
