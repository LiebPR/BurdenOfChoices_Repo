using UnityEngine;

public class DraggableObject : MonoBehaviour, IPickListener, IInteractable
{
    #region References
    PickableBehaviour pickable;
    DraggableBehaviour draggable;
    DataProvider dataProvider;
    PlayerController playerController;
    #endregion

    public float Weight
    {
        get
        {
            if (dataProvider != null)
            {
                var equipData = dataProvider.GetData<EquipableData>();
                if (equipData != null)
                    return Mathf.Max(1f, equipData.weight);
            }

            return 1f; // peso neutro si no hay DataProvider o EquipableData
        }
    }

    private void Awake()
    {
        pickable = GetComponent<PickableBehaviour>();
        draggable = GetComponent<DraggableBehaviour>();
        dataProvider = GetComponent<DataProvider>();

        // Esto evita que se equipe normalmente
        pickable.AllowEquip = false;
    }

    #region IPickListener
    public void OnPick(ICatcher catcher)
    {
        if (draggable == null || catcher == null)
            return;

        Transform playerRoot = catcher.GetCatchPoint().root;
        playerController = playerRoot.GetComponent<PlayerController>();

        // Inicia drag
        draggable.StartDrag(playerRoot);

        // Aplicar peso al jugador
        if (playerController != null)
            playerController.SetDraggedWeight(Weight);

        // Lanzar animación de coger
        AnimatorManager animatorManager = playerRoot.GetComponent<AnimatorManager>();
        if (animatorManager != null)
            animatorManager.SetGrabbing(true);
    }

    public void OnDrop()
    {
        if (draggable != null)
            draggable.StopDrag();

        // Resetear peso
        if (playerController != null)
        {
            playerController.SetDraggedWeight(1f);

            // Restaurar animación
            AnimatorManager animatorManager = playerController.GetComponent<AnimatorManager>();
            if (animatorManager != null)
                animatorManager.SetGrabbing(false);

            playerController = null;
        }
    }
    #endregion

    #region IInteractable
    public void OnPress() { }
    public void OnRelease() { }
    public void OnHighlight() { }
    public void OnRemoveHighlight() { }
    #endregion
}