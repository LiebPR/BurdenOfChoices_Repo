using UnityEngine;

public class PickableMissionTarget : MonoBehaviour, IPickListener
{
    #region Inspector
    [SerializeField] PickableObjectMission mission;
    #endregion

    public void OnPick(ICatcher catcher)
    {
        mission.NotifyPicked(this);
    }

    public void OnDrop() { }
}
