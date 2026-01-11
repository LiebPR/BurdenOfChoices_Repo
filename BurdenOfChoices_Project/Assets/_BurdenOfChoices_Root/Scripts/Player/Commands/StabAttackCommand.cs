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
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();
            if(enemyHealth != null)
            {
                Vector3 hitDirection = tOrigin.forward.normalized;

                //Aplicamos el daño definido en el WeaponData
                enemyHealth.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack);
            }

            BrokenGlass brokenGlass = tOrigin.GetComponentInParent<BrokenGlass>();
            if (brokenGlass != null)
            {
                brokenGlass.BreakImmediate(hit.point);
            }
        }
    }
}
