using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A helper script that goes on the Shop Item Prefab. It displays the item's
/// information and handles the button click to initiate a purchase.
/// </summary>
public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemCostText;
    public Button purchaseButton;

    private SlotItem associatedItem;

    /// <summary>
    /// Configures the UI elements with the data from a given SlotItem.
    /// </summary>
    public void Setup(SlotItem item)
    {
        associatedItem = item;

        itemIcon.sprite = item.sprite;
        itemIcon.SetNativeSize();
        itemNameText.text = item.itemID; // Using the ScriptableObject's asset name
        itemCostText.text = $"${item.cost}";

        // Add a listener to the button to call the purchase method
        purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    /// <summary>
    /// Called when the purchase button is clicked.
    /// </summary>
    private void OnPurchaseClicked()
    {
        // Attempt to buy the item through the ShopManager
        bool success = ShopManager.Instance.TryPurchaseItem(associatedItem);

        if (success)
        {
            // Optional: Disable the button or play a sound effect after purchase.
            // The ShopManager will automatically handle removing this item from the shop
            // by repopulating it after the inventory changes.
        }
    }
}
