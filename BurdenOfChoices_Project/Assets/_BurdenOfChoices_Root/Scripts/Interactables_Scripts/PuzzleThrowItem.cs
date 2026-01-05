using UnityEngine;

public class PuzzleThrowItem : MonoBehaviour, IInteractable
{
    #region Inspector States
    [SerializeField] int objectID;
    #endregion

    #region Internal States
    Rigidbody rb;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
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
}
