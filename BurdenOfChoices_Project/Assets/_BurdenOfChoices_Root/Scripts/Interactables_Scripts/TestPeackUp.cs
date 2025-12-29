using UnityEngine;

public class TestPeackUp : MonoBehaviour, IInteractable
{
    #region References
    Renderer meshRenderer; 
    [SerializeField] Material highlightMaterial; //material cuando está siendo apuntado. 
    #endregion

    #region Internal States
    Material originalMaterial;
    #endregion

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
        originalMaterial = meshRenderer.material;
    }

    #region Press & Release
    public void OnPress()
    {
        Destroy(gameObject);
    }
    public void OnRelease()
    {
    }
    #endregion

    #region Hightligh
    public void OnHighlight()
    {
        if(meshRenderer != null && highlightMaterial != null)
            meshRenderer.material = highlightMaterial;
    }
    public void OnRemoveHighlight()
    {
        if(meshRenderer != null && originalMaterial != null)
            meshRenderer.material = originalMaterial;
    }
    #endregion
}
