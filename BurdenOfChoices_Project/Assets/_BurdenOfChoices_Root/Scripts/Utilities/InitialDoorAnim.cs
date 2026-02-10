using System.Collections;
using UnityEngine;

public class InitialDoorAnim : MonoBehaviour
{
    [SerializeField] Animator animDoorLeft;
    [SerializeField] Animator animDoorRight;

    [Header("Audio")]
    [SerializeField] string audioAnimationSFX = "SFX_BigDoor_Close";
    [SerializeField] float volumen = 0.5f;

    static readonly int DoorCloseHash = Animator.StringToHash("CloseDoor");

    private void Start()
    {
        StartCoroutine(AnimationRoutine());
    }

    IEnumerator AnimationRoutine()
    {
        yield return new WaitForSeconds(0.4f);
        animDoorLeft.SetTrigger(DoorCloseHash);
        animDoorRight.SetTrigger(DoorCloseHash);

        yield return new WaitForSeconds(0.7f);
        AudioManager.Instance.PlaySFX2D(audioAnimationSFX, volumen);
    }
}
