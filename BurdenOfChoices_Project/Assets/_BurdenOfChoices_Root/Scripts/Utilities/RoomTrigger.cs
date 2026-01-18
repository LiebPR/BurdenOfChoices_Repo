using UnityEngine;

/// <summary>
/// RoomTrigger
/// Define el estado emocional del jugador
/// según la presencia de enemigos en la sala.
/// </summary>
public class RoomTrigger : MonoBehaviour
{
    #region Inspector
    [SerializeField] bool hasEnemies;
    #endregion

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        AnimatorManager animator = other.GetComponent<AnimatorManager>();
        if (animator == null) return;

        ApplyRoomState(animator);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        AnimatorManager animator = other.GetComponent<AnimatorManager>();
        if (animator == null) return;

        animator.SetRelaxed(1f);
    }

    #region Core
    void ApplyRoomState(AnimatorManager animator)
    {
        float relaxedValue = hasEnemies ? 0f : 1f;
        animator.SetRelaxed(relaxedValue);
    }
    #endregion
}
