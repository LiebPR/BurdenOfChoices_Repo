using UnityEngine;

[CreateAssetMenu(fileName = "NewEquipableData", menuName = "Equipment/Equipable Data")]
public class EquipableData : ScriptableObject
{
    [Header("Gneral")]
    public string itemName = "Item";
    public float waight = 1f;

    [Header("State")]
    public bool isEquipped;
}
