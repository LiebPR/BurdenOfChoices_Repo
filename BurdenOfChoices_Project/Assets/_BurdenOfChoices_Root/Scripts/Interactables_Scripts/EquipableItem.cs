    using UnityEngine;

    public class EquipableItem : MonoBehaviour, IInteractable
    {
        #region References
        PickableBehaviour pickable; //componente que gestiona el estado físico y equipar
        ThrowableBehaviour throwable; //componente que gestiona el lanzamiento
        PlayerHand playerHand;
        DataProvider dataProvider; //componente que expone SO
        Renderer meshRenderer;
        #endregion

        #region Getters
        public EquipableData Data => dataProvider != null ? dataProvider.GetData<EquipableData>() : null;
        #endregion

        void Awake()
        {
            pickable = GetComponent<PickableBehaviour>();
            throwable = GetComponent<ThrowableBehaviour>();
            dataProvider = GetComponent<DataProvider>();
            meshRenderer = GetComponentInChildren<Renderer>();

            if (playerHand == null)
            {
                playerHand = FindAnyObjectByType<PlayerHand>();
                if(playerHand == null)
                {
                    Debug.LogWarning("No se encontro ningún PlayerHand en la escena.");
                }
            }
        }

        #region Interaction
        public void OnPress()
        {
            
        }

        public void OnRelease() 
        {
            
        }

        public void OnHighlight()
        {
            if (meshRenderer == null || Data == null) return;
            meshRenderer.material = Data.highlightMaterial;
        }

        public void OnRemoveHighlight()
        {
            if(meshRenderer == null || Data == null) return;
            meshRenderer.material = Data.originalMaterial;
        }
        #endregion
    }