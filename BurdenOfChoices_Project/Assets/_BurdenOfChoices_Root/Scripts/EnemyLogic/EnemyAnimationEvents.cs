using UnityEngine;

public class EnemyAnimationEvents : MonoBehaviour
{
    [SerializeField] EnemyAttack attack;

    public void OnAttackHit()
    {
        if(attack == null) return;

        attack.ResolveAttackHit();
    }
}
