using UnityEngine;

public class PlayerHand : MonoBehaviour, ICatcher
{
    [SerializeField] Transform catchPoint;

    public Transform GetCatchPoint()
    {
        return catchPoint;
    }
}
