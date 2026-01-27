using UnityEngine;

public class MaterialMesh : MonoBehaviour, IButtonVisual
{
    [SerializeField] MeshRenderer targetMesh;
    [SerializeField] Material normal;
    [SerializeField] Material hover;
    [SerializeField] Material selected;
    [SerializeField] Material disabled;

    Material current;

    private void Awake()
    {
        if (targetMesh == null)
        {
            Debug.LogError("Asiganar targetMesh en" + name);
            enabled = false;
            return;
        }

        if (normal == null)
            normal = targetMesh.material;

        current = normal;
        targetMesh.material = normal;
    }

    public void SetNormal() => Set(normal);
    public void SetHover() => Set(hover ?? normal);
    public void SetSelected() => Set(selected ?? normal);
    public void SetDisabled() => Set(disabled ?? normal);

    void Set(Material mat)
    {
        if (current == mat) return;
        current = mat;
        targetMesh.material = mat;
    }
}
