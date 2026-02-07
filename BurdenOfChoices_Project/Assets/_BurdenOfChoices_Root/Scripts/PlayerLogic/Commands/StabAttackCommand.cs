using UnityEngine;

//Comando de ataque lineal (raycast desde el origen hacía adelante)
public class StabAttackCommand : AttackCommand
{
    public StabAttackCommand(WeaponData data, Transform origin) : base(data, origin) { }

    public override void Execute()
    {
        Vector3 origin = tOrigin.position;
        Vector3 direction = tOrigin.forward;

        //Dibujar rayo para depuración
        Debug.DrawRay(origin, direction * weaponData.range, Color.red, 1f);

        Ray ray = new Ray(tOrigin.position, tOrigin.forward);

        if(Physics.Raycast(ray, out RaycastHit hit, weaponData.range))
        {

            Vector3 hitDirection = tOrigin.forward.normalized;

            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if(enemyHealth != null)
            {
                //Aplicamos el daño definido en el WeaponData
                enemyHealth.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack);
            }

            IHittable hittable = hit.collider.GetComponent<IHittable>();
            if (hittable != null)
            {
                hittable.OnHit(hit.point, hitDirection);
            }

            BrokenGlass brokenGlass = hit.collider.GetComponent<BrokenGlass>();
            if (brokenGlass != null)
            {
                brokenGlass.BreakImmediate(hit.point);
            }
        }
    }
}
