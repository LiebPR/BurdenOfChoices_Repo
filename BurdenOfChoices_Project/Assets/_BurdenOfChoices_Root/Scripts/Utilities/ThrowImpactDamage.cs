using UnityEngine;

public class ThrowImpactDamage : MonoBehaviour
{
    #region Inspector States
    [Header("Data")]
    [SerializeField] WeaponData weaponData;

    [Header("Data")]
    [SerializeField] LayerMask breakLayer;
    [SerializeField] GameObject impactPrefab;
    #endregion

    #region Internal States
    bool hasBeenThrown;
    bool hasCollided;
    bool damageConsumed;
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (hasCollided) return;
        Debug.Log($"Impact layer: {LayerMask.LayerToName(collision.collider.gameObject.layer)}");

        EnemyHealth enemy = collision.collider.GetComponent<EnemyHealth>();
        if(enemy != null)
        {
            hasCollided = true;
            ApplyDamage(enemy, collision);
            return;
        }

        // 2. ¿Es capa que rompe el objeto?
        if (IsBreakLayer(collision.gameObject.layer))
        {
            hasCollided = true;
            SpawnImpact(collision.contacts[0].point);
            Destroy(gameObject);
        }
    }

    #region Public API 
    /// <summary>
    /// Marca el objeto como arrojado y lsito para causar daño.
    /// </summary>
    public void Arm()
    {
        hasBeenThrown = true;
        hasCollided = false;
        damageConsumed = false;
    }

    /// <summary>
    /// Desactiva completamente la capacidad de daño.
    /// </summary>
    public void Disarm()
    {
        hasBeenThrown = false;
        hasCollided = false;
        damageConsumed = false;
    }
    #endregion

    #region Damage Logic
    void ApplyDamage(EnemyHealth enemy, Collision collision)
    {
        if (damageConsumed) return;

        Vector3 hitDirection = rb.linearVelocity.normalized;

        enemy.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack);

        damageConsumed = true;
        Disarm();
    }
    #endregion

    #region Break Logic
    bool IsBreakLayer(int layer)
    {
        return (breakLayer.value & (1 << layer)) != 0;
    }
    void SpawnImpact(Vector3 position)
    {
        if(impactPrefab == null) return;

        Instantiate(impactPrefab, position, Quaternion.identity);
    }
    #endregion
}
