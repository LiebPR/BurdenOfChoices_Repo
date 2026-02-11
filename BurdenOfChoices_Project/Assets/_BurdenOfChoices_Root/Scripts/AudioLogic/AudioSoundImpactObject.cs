using UnityEngine;

public class AudioSoundImpactObject : MonoBehaviour
{
    #region Inspector
    [Header("Layers")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask soundEmitterLayer;

    [SerializeField] float rayDistance = 0.3f;

    [SerializeField] string impactSFXID = "SFX_Object_";
    [SerializeField] float volume = 0.2f;
    #endregion

    #region State
    bool isGrounded;
    bool wasAirborne;
    #endregion

    private void Update()
    {
        UpdateGroundState();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!wasAirborne) return;

        int layer = collision.gameObject.layer;

        if (IsInLayerMask(layer, groundLayer) ||
            IsInLayerMask(layer, soundEmitterLayer))
        {
            PlayImpact();
        }
    }

    #region Ground Logic
    void UpdateGroundState()
    {
        bool groundNow = Physics.Raycast(
            transform.position,
            Vector3.down,
            rayDistance,
            groundLayer
        );

        // Si en algún momento dejó de detectar suelo → estuvo en el aire
        if (isGrounded && !groundNow)
        {
            wasAirborne = true;
        }

        isGrounded = groundNow;
    }
    #endregion

    #region Sound
    void PlayImpact()
    {
        AudioManager.Instance.PlaySFXAtPosition(
            impactSFXID,
            transform.position
        );

        // Consumimos el estado de caída
        wasAirborne = false;
    }
    #endregion

    #region Utils
    bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
    #endregion
}
