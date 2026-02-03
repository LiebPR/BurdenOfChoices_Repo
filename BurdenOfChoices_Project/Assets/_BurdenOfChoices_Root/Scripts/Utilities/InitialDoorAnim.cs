using UnityEngine;

public class InitialDoorAnim : MonoBehaviour
{
    [SerializeField] Animator animDoorLeft;
    [SerializeField] Animator animDoorRight;

    static readonly int DoorCloseHash = Animator.StringToHash("CloseDoor");

    private void Start()
    {
        animDoorLeft.SetTrigger(DoorCloseHash);
        animDoorRight.SetTrigger(DoorCloseHash);
    }
}
