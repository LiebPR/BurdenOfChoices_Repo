using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    #region Inspector States
    [Header("Door Settings")]
    [SerializeField] bool isSecondaryDoor;
    [SerializeField] Animator exitDoorAnimator; //Animator de la puerta de salida
    [SerializeField] string openExitTrigger = "Open";
    [SerializeField] float exitDoorAnimatorDuration = 1f;
    [SerializeField] Animator entryDoorAnimator; //animator de la puerta de entrada en la nueva sala
    [SerializeField] string closeEntryTrigger = "Close";
    [SerializeField] float entryDoorAnimDuration = 1f;
    [SerializeField] string idleTrigger = "Idle";
    [SerializeField] string lockedTrigger = "ILocked";

    [SerializeField] Transform playerSpawnPoint; //punto donde aparecera el jugador en la nueva sala

    [Header("CineMachine Settings")]
    [SerializeField] CinemachineCamera entryCamera;

    [Header("Obstacle Ray Settings")]
    [SerializeField] Transform rayOrigin;
    [SerializeField] Vector3 rayDirection = Vector3.forward;
    [SerializeField] float rayDistance = 0.6f;
    [SerializeField] LayerMask obstacleMask;
    #endregion

    #region Internal States
    bool isInteracting;
    bool locked;
    #endregion

    #region References
    FadeController fadeController;
    #endregion

    private void Awake()
    {
        if (fadeController == null)
        {
            fadeController = FindAnyObjectByType<FadeController>();
        }

        if (isSecondaryDoor)
            locked = true;
    }

    public void OnHighlight(){}
    public void OnRemoveHighlight(){}
    public void OnRelease(){}

    public void OnPress()
    {
        if (isInteracting) return;

        if (HasObstacleInFront()) return;

        if (locked)
        {
            PlayLockedAnimation();
            return;
        }

        StartCoroutine(HandleDoorRoutine());
    }

    void PlayLockedAnimation()
    {
        if (exitDoorAnimator != null)
        {
            exitDoorAnimator.SetTrigger(lockedTrigger);
        }
    }

    bool HasObstacleInFront()
    {
        if (rayOrigin == null) return false;

        Vector3 direction = rayOrigin.TransformDirection(rayDirection.normalized);
        return Physics.Raycast(rayOrigin.position, direction, rayDistance, obstacleMask);
    }

    #region Block System (Public API)
    public void Lock()
    {
        locked = true;
    }
    public void Unlock()
    {
        locked = false;
    }
    public bool IsLocked()
    {
        return locked;
    }
    #endregion

    #region Routine
    IEnumerator HandleDoorRoutine()
    {
        isInteracting = true;

        // Cambiar fase del juego
        GameDirector.Instance.SetPhase(GamePhase.Cutscene);

        // Abrir la puerta de salida
        if (exitDoorAnimator != null)
            exitDoorAnimator.SetTrigger(openExitTrigger);
        yield return new WaitForSeconds(exitDoorAnimatorDuration);

        // Volver a Idle después de abrir
        exitDoorAnimator.SetTrigger(idleTrigger);

        // Fade out
        if (fadeController != null)
            yield return fadeController.FadeOut();

        // Teletransportar jugador
        Transform player = FindAnyObjectByType<PlayerController>().transform;
        if (player != null && playerSpawnPoint != null)
            player.position = playerSpawnPoint.position;

        CameraManager.Instance.ActivateCamera(entryCamera);

        yield return new WaitForSeconds(entryDoorAnimDuration);

        // Cerrar puerta de entrada en la nueva sala
        if (entryDoorAnimator != null)
            entryDoorAnimator.SetTrigger(closeEntryTrigger);

        // Fade in
        if (fadeController != null)
            yield return fadeController.FadeIn();

        // Restaurar estado del juego
        GameDirector.Instance.SetPhase(GamePhase.Playing);

        isInteracting = false;

        // Volver a Idle después de cerrar
        entryDoorAnimator.SetTrigger(idleTrigger);
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        if (rayOrigin == null) return;

        Gizmos.color = Color.red;
        Vector3 dir = rayOrigin.TransformDirection(rayDirection.normalized);
        Gizmos.DrawRay(rayOrigin.position, dir * rayDistance);
    }
}
