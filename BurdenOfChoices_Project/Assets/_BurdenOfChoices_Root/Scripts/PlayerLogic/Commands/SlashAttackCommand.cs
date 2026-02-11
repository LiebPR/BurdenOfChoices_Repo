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
                // Calcula punto exacto de impacto
                Vector3 hitPoint = hits[i].ClosestPoint(tOrigin.position);
                enemyHealth.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack, hitPoint);
                AudioManager.Instance.PlaySFX2D("SFX_Impact_Slash", 0.3f);
            }

            // Objetos golpeables (tutorial, props, etc.)
            IHittable hittable = hits[i].GetComponent<IHittable>();
            if (hittable != null)
            {
                Vector3 hitPoint = hits[i].ClosestPoint(tOrigin.position);
                hittable.OnHit(hitPoint, hitDirection);
            }
        }
    }
}
