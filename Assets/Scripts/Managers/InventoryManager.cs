using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the player's unlocked slot items. This is a singleton.
/// Place this on a persistent GameObject in your scene (e.g., "GameManager").
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Item Database")]
    // Assign ALL possible SlotItem assets here in the Inspector.
    public List<SlotItem> allPossibleItems;

    // This list will be populated at runtime with the items the player owns.
    [HideInInspector]
    public Dictionary<SlotItemType, List<SlotItem>> unlockedItemsByType;

    // We can use an event to notify other systems (like the spinner) when the inventory changes.
    public static event System.Action OnInventoryChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make it persist between scenes
            InitializeInventory();
            LoadInventory();
        }
    }

    private void InitializeInventory()
    {
        unlockedItemsByType = new Dictionary<SlotItemType, List<SlotItem>>();
        foreach (SlotItemType type in System.Enum.GetValues(typeof(SlotItemType)))
        {
            unlockedItemsByType[type] = new List<SlotItem>();
        }
    }

    /// <summary>
    /// Unlocks an item, adds it to the list, saves, and notifies listeners.
    /// </summary>
    public void UnlockItem(string itemID)
    {
        SlotItem itemToUnlock = allPossibleItems.FirstOrDefault(item => item.itemID == itemID);

        if (unlockedItemsByType[itemToUnlock.itemType].Any(item => item.itemID == itemID))
        {
            Debug.LogWarning($"Item '{itemID}' is already unlocked.");
            return;
        }


        if (itemToUnlock != null)
        {
            unlockedItemsByType[itemToUnlock.itemType].Add(itemToUnlock);
            Debug.Log($"Unlocked item: {itemToUnlock.name}");
            SaveInventory();
            OnInventoryChanged?.Invoke(); // Fire the event
        }
        else
        {
            Debug.LogError($"Could not find item with ID '{itemID}' in All Possible Items list.");
        }
    }

    /// <summary>
    /// Gets the list of unlocked items for a specific type.
    /// </summary>
    public List<SlotItem> GetUnlockedItemsByType(SlotItemType type) // <--- NEW: Getter for specific type
    {
        if (unlockedItemsByType.ContainsKey(type))
        {
            return unlockedItemsByType[type];
        }
        Debug.LogWarning($"No unlocked items found for type: {type}");
        return new List<SlotItem>(); // Return an empty list if type not found (shouldn't happen with InitializeInventory)
    }

    /// <summary>
    /// Loads the list of unlocked item IDs from PlayerPrefs.
    /// </summary>
    private void LoadInventory()
    {
        InitializeInventory();

        // Add items that are unlocked by default
        foreach (var item in allPossibleItems)
        {
            if (item.isUnlockedByDefault)
            {
                if (!unlockedItemsByType[item.itemType].Contains(item)) // Prevent duplicates if already added by save data
                {
                    unlockedItemsByType[item.itemType].Add(item);
                }
            }
        }

        // Load saved items
        foreach (SlotItemType type in System.Enum.GetValues(typeof(SlotItemType)))
        {
            string playerPrefKey = $"UnlockedItems_{type}"; // Key per type
            string savedItemsString = PlayerPrefs.GetString(playerPrefKey, "");

            if (!string.IsNullOrEmpty(savedItemsString))
            {
                string[] savedItemIDs = savedItemsString.Split(',');
                foreach (string id in savedItemIDs)
                {
                    SlotItem item = allPossibleItems.FirstOrDefault(i => i.itemID == id && i.itemType == type); // <--- IMPORTANT: Match by type
                    if (item != null && !unlockedItemsByType[type].Contains(item))
                    {
                        unlockedItemsByType[type].Add(item);
                    }
                }
            }
        }

        int totalUnlocked = 0;
        foreach (var list in unlockedItemsByType.Values)
        {
            totalUnlocked += list.Count;
        }
        Debug.Log($"Inventory Loaded. {totalUnlocked} items unlocked across all types.");
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Saves the list of unlocked item IDs to PlayerPrefs as a single string.
    /// </summary>
    private void SaveInventory()
    {
        // We only save items that are NOT unlocked by default
        foreach (SlotItemType type in System.Enum.GetValues(typeof(SlotItemType)))
        {
            List<string> idsToSave = unlockedItemsByType[type]
                .Where(item => !item.isUnlockedByDefault) // Only save non-default items
                .Select(item => item.itemID)
                .ToList();

            string playerPrefKey = $"UnlockedItems_{type}";
            string savedItemsString = string.Join(",", idsToSave);
            PlayerPrefs.SetString(playerPrefKey, savedItemsString);
        }
        PlayerPrefs.Save();
        Debug.Log("Inventory Saved!");
    }
}
