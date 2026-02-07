using UnityEngine;

/// <summary>
/// SlashAttackCommand
/// Comando de ataque radial (área alrededro del origen)
/// </summary>
public class SlashAttackCommand : AttackCommand
{
    public SlashAttackCommand(WeaponData data, Transform origin) : base(data, origin) { }

    public override void Execute()
    {
        //Detecta enemigos en área
        Collider[] hits = Physics.OverlapSphere(tOrigin.position, weaponData.range);

        for (int i = 0; i < hits.Length; i++)
        {
            Vector3 hitDirection = tOrigin.forward.normalized;

            EnemyHealth enemyHealth = hits[i].GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack);
            }

            // Objetos golpeables (tutorial, props, etc.)
            IHittable hittable = hits[i].GetComponent<IHittable>();
            if (hittable != null)
            {
                hittable.OnHit(hits[i].ClosestPoint(tOrigin.position), hitDirection);
            }
        }
    }
}
