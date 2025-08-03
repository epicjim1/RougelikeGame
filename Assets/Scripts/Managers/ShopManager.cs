using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Manages the Shop UI, populating it with purchasable items and handling transactions.
/// </summary>
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject shopItemPrefab;       // Prefab for a single shop item entry (must have ShopItemUI script)
    public Transform shopItemContainer;     // The parent object where shop item prefabs will be instantiated
    public TextMeshProUGUI currencyText;    // UI Text to display the player's current money

    [Header("Shop Configuration")]
    public int startingCurrency = 500;

    private int playerCurrency;

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
        }
    }

    private void OnEnable()
    {
        // Subscribe to the event to refresh the shop when an item is unlocked
        InventoryManager.OnInventoryChanged += PopulateShop;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= PopulateShop;
    }

    private void Start()
    {
        playerCurrency = GameManager.Instance.GetTotalCoins();
        UpdateCurrencyDisplay();
        PopulateShop();
    }

    /// <summary>
    /// Clears and rebuilds the shop UI based on which items are not yet unlocked.
    /// </summary>
    public void PopulateShop()
    {
        // Clear existing shop items to prevent duplicates
        foreach (Transform child in shopItemContainer)
        {
            Destroy(child.gameObject);
        }

        // Get all possible items and the ones the player already has
        List<SlotItem> allItems = InventoryManager.Instance.allPossibleItems;
        List<SlotItem> allUnlockedItems = new List<SlotItem>();
        foreach (var list in InventoryManager.Instance.unlockedItemsByType.Values)
        {
            allUnlockedItems.AddRange(list);
        }

        // Find which items are available to be purchased
        var itemsForSale = allItems.Where(item => !allUnlockedItems.Contains(item) && !item.isUnlockedByDefault);

        foreach (SlotItem item in itemsForSale)
        {
            GameObject shopItemObj = Instantiate(shopItemPrefab, shopItemContainer);
            ShopItemUI shopItemUI = shopItemObj.GetComponent<ShopItemUI>();
            if (shopItemUI != null)
            {
                shopItemUI.Setup(item);
            }
            else
            {
                Debug.LogError($"Shop Item Prefab is missing the required 'ShopItemUI' script.", shopItemPrefab);
            }
        }
    }

    /// <summary>
    /// Attempts to purchase an item. Called by ShopItemUI.
    /// </summary>
    public bool TryPurchaseItem(SlotItem item)
    {
        if (playerCurrency >= item.cost)
        {
            // Sufficient funds
            playerCurrency -= item.cost;
            GameManager.Instance.UsedTotalCoins(item.cost);
            UpdateCurrencyDisplay();

            // Unlock the item via the InventoryManager
            InventoryManager.Instance.UnlockItem(item.itemID);

            Debug.Log($"Successfully purchased {item.itemID} for {item.cost}.");
            return true;
        }
        else
        {
            // Insufficient funds
            Debug.Log($"Not enough currency to purchase {item.itemID}. Need {item.cost}, have {playerCurrency}.");
            return false;
        }
    }

    private void UpdateCurrencyDisplay()
    {
        if (currencyText != null)
        {
            currencyText.text = $"{playerCurrency}";
        }
    }
}
