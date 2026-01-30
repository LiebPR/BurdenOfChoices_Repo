using UnityEngine;

public class WeightManager : MonoBehaviour
{
    [SerializeField] PlayerController playerController;

    float currentWeight = 1f;

    private void OnEnable()
    {
        PickSystem.OnPickStarted += HandlePickStarted;
        PickSystem.OnPickEnded += HandlePickEnded;
    }

    private void OnDisable()
    {
        PickSystem.OnPickStarted -= HandlePickStarted;
        PickSystem.OnPickEnded -= HandlePickEnded;
    }

    #region Pick Handles
    void HandlePickStarted(PickableBehaviour pickable)
    {
        if (pickable == null) return;
        currentWeight = Mathf.Max(1f, pickable.Weight);
        ApplyWeightToPlayer();
    }

    void HandlePickEnded(PickableBehaviour pickable)
    {
        currentWeight = 1f;
        ApplyWeightToPlayer();
    }
    #endregion 

    void ApplyWeightToPlayer()
    {
        playerController.SetWeight(currentWeight);
    }
}
