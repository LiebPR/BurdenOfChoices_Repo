using UnityEngine;
public class Highlight : MonoBehaviour
{
    [SerializeField] Material highlightMaterial;
    [SerializeField] PickableBehaviour pickable;

    Renderer rend;
    Material[] baseMaterials;

    bool isHighlighted;
    bool blocked;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        baseMaterials = rend.materials;
    }

    void OnEnable()
    {
        PickSystem.OnPickStarted += OnPicked;
        PickSystem.OnPickEnded += OnDropped;
    }

    void OnDisable()
    {
        PickSystem.OnPickStarted -= OnPicked;
        PickSystem.OnPickEnded -= OnDropped;
    }

    public void EnableHighlight()
    {
        if (isHighlighted) return;
        if (blocked) return;
        if (pickable.IsCatched) return;

        Material[] mats = new Material[baseMaterials.Length + 1];
        baseMaterials.CopyTo(mats, 0);
        mats[mats.Length - 1] = highlightMaterial;

        rend.materials = mats;
        isHighlighted = true;
    }

    public void DisableHighlight()
    {
        if (!isHighlighted) return;

        rend.materials = baseMaterials;
        isHighlighted = false;
    }

    void OnPicked(PickableBehaviour pb)
    {
        if (pb != pickable) return;

        DisableHighlight();
        blocked = true;
    }

    void OnDropped(PickableBehaviour pb)
    {
        if (pb != pickable) return;

        blocked = false;
    }
}
