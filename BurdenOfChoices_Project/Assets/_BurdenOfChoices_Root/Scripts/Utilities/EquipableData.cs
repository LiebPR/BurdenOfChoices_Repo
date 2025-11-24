using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipableData", menuName = "Equipment/Equipable Data")]
public class EquipableData : ScriptableObject
{
    [Header("Gneral")]
    public string itemName = "Item";
    public float weight = 1f; //peso del objeto
    public Material highlightMaterial; //material de cuando puedes interactuar con el objeto
    public Material originalMaterial; //material original del objeto. 
}
