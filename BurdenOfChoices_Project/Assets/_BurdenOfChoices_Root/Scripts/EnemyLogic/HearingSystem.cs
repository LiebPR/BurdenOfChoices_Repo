using System.Collections;
using UnityEngine;

public class HearingSystem : MonoBehaviour
{
    #region Inspector States
    [SerializeField] float hearingRadius = 12f;
    #endregion

    #region Events
    public event System.Action<Vector3> OnHearSound;
    #endregion

    private void OnEnable()
    {
        NoiseEvents.OnNoiseEmitted += HandleNoise;
    }

    private void OnDisable()
    {
        NoiseEvents.OnNoiseEmitted -= HandleNoise;
    }

    void HandleNoise(NoiseEvent noise)
    {
        float distance = Vector3.Distance(transform.position, noise.position);
        if (distance > hearingRadius) return;

        StartCoroutine(DelayedHear(noise));
    }

    IEnumerator DelayedHear(NoiseEvent noise)
    {
        yield return new WaitForSeconds(noise.delay);
        OnHearSound?.Invoke(noise.position);
    }
}
