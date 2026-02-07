using UnityEngine;

public class PuzzleThrowItem : MonoBehaviour, IInteractable, IPickListener
{
    #region Inspector States
    [SerializeField] int objectID;
    #endregion

    #region Internal States
    Rigidbody rb;
    DataProvider dataProvider;
    #endregion

    #region Getter
    EquipableData Data => dataProvider.GetData<EquipableData>();
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        dataProvider = GetComponent<DataProvider>();

        if(rb != null && Data != null)
        {
            rb.mass = Data.weight; //aquí se aplica el peso
        }
    }

    #region Accessors
    public int GetObjectID()
    {
        return objectID;
    }
    #endregion

    #region Press & Release
    public void OnPress()
    {

    }

    public void OnRelease()
    {

    }

    public void OnHighlight()
    {

    }

    public void OnRemoveHighlight()
    {

    }
    #endregion

    #region Pick 
    public void OnPick(ICatcher catcher)
    {

    }

    public void OnDrop()
    {

    }
    #endregion
}
