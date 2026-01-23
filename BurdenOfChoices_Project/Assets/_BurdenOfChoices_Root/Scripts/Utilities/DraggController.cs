using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class DragController : MonoBehaviour
{
    PlayerController playerController;
    DraggableObject currentDragObject;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (currentDragObject != null && currentDragObject.isBeingDragged)
        {
            Vector3 axis = currentDragObject.dragAxis.normalized;

            // 🔹 Bloqueo de eje
            Vector3 lockedAxis = Vector3.zero;
            if (Mathf.Abs(axis.x) > Mathf.Abs(axis.z))
                lockedAxis = Vector3.right;
            else
                lockedAxis = Vector3.forward;

            playerController.LockMovementAxis(lockedAxis);

            // 🔹 Fuerza al objeto
            Vector3 inputDir = new Vector3(
                playerController.InputMovement.x,
                0f,
                playerController.InputMovement.y
            );

            currentDragObject.ApplyForce(inputDir, playerController.WeightSpeedMultiplier);

            // 🔹 Peso
            playerController.SetDraggedWeight(currentDragObject.Weight);

            // 🔹 SNAP DE ROTACIÓN CORRECTO
            SnapPlayerRotation(currentDragObject);

            playerController.LockRotation();
        }
        else
        {
            playerController.UnlockMovementAxis();
            playerController.ResetDraggedWeight();
            playerController.UnlockRotation();
        }
    }

    private void SnapPlayerRotation(DraggableObject obj)
    {
        if (obj.grabFaceLocal == Vector3.zero) return;

        // Cara agarrada en mundo
        Vector3 grabbedFaceWorld = obj.transform.TransformDirection(obj.grabFaceLocal);

        // Mirar a la dirección CONTRARIA
        Vector3 lookDir = -grabbedFaceWorld;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
            playerController.transform.rotation = Quaternion.LookRotation(lookDir);
    }

    public bool TryStartDrag(DraggableObject obj)
    {
        if (obj == null || !obj.CanBeGrabbedBy(transform))
            return false;

        if (currentDragObject != null)
            StopDrag();

        currentDragObject = obj;
        currentDragObject.StartDragging(transform);
        return true;
    }

    public void StopDrag()
    {
        if (currentDragObject == null) return;

        currentDragObject.StopDragging();
        currentDragObject = null;

        playerController.UnlockMovementAxis();
        playerController.ResetDraggedWeight();
        playerController.UnlockRotation();
    }
}
