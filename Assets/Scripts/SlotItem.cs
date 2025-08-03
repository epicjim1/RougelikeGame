using UnityEngine;

public enum SlotItemType
{
    Level,
    Weapon,
    Modifier,
    Boss
}

[CreateAssetMenu(fileName = "NewSlotItem", menuName = "Slot Machine/Slot Item")]
public class SlotItem : ScriptableObject
{
    [Header("Item Properties")]
    public string itemID;
    public SlotItemType itemType;
    public Sprite sprite;
    public string value;
    public bool rotateDisplay;

    [Header("Shop Properties")]
    public int cost;
    public bool isUnlockedByDefault = false;
}
