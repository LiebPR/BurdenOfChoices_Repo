using UnityEngine;

public class ThrowImpactDamage : MonoBehaviour
{
    #region Inspector States
    [Header("Data")]
    [SerializeField] WeaponData weaponData;

    [Header("Settings")]
    [SerializeField] bool isBreakeable;
    [SerializeField] LayerMask breakLayer;
    [SerializeField] GameObject impactPrefab;
    [SerializeField] GameObject brokenGlasVFX;

    [Header("GroundCheck")]
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;
    #endregion

    #region Internal States
    bool hasBeenThrown;
    bool hasCollided;
    bool damageConsumed;
    Rigidbody rb;
    bool hasLeftGround;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (!hasBeenThrown) return;

        if (!IsGrounded())
            hasLeftGround = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown) return;
        if (hasCollided) return;

        EnemyHealth enemy = collision.collider.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            hasCollided = true;
            ApplyDamage(enemy, collision);
            Break(collision);
            return;
        }

        // 2. ¿Es capa que rompe el objeto?
        if (IsBreakLayer(collision.gameObject.layer) && hasLeftGround)
        {
            hasCollided = true;
            Break(collision);
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
        hasLeftGround = false;
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

        enemy.TakeHit(weaponData.damage, hitDirection, weaponData.knockBack, collision.contacts[0].point);

        damageConsumed = true;
        Disarm();
    }
    #endregion

    #region Break Logic
    void Break(Collision collision)
    {
        if (!isBreakeable) return;
        SpawnBreakVFX();
        SpawnImpact(collision.contacts[0].point);
        Destroy(gameObject);
    }
    bool IsBreakLayer(int layer)
    {
        return (breakLayer.value & (1 << layer)) != 0;
    }
    void SpawnImpact(Vector3 position)
    {
        if (impactPrefab == null) return;

        Instantiate(impactPrefab, position, Quaternion.identity);
    }
    #endregion

    #region Ground Check
    bool IsGrounded()
    {
        return Physics.Raycast(
        transform.position,
        Vector3.down,
        groundCheckDistance,
        groundLayer);
    }
    #endregion

    #region VFX 
    void SpawnBreakVFX()
    {
        if (!isBreakeable) return;
        if (brokenGlasVFX == null) return;

        Instantiate(
            brokenGlasVFX,
            transform.position,
            Quaternion.identity
        );
    }
    #endregion
}
