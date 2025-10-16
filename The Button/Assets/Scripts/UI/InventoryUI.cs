using System.Collections.Generic;
using TheButton.Player;
using TheButton.Items;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheButton.UI
{
    /// <summary>
    /// Displays player inventory UI (4-5 slots at bottom of screen)
    /// Shows item icons and allows usage via number keys or clicking
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform inventorySlotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("Settings")]
        [SerializeField] private int maxSlots = 5;

        private List<GameObject> slotObjects = new List<GameObject>();
        private PlayerInventory playerInventory;

        private void Start()
        {
            CreateInventorySlots();
            FindLocalPlayerInventory();
        }

        private void Update()
        {
            if (playerInventory == null)
            {
                FindLocalPlayerInventory();
                return;
            }

            UpdateInventoryDisplay();
            HandleInventoryInput();
        }

        private void CreateInventorySlots()
        {
            // Clear existing slots
            foreach (var slot in slotObjects)
            {
                if (slot != null) Destroy(slot);
            }
            slotObjects.Clear();

            // Create new slots
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slot = Instantiate(slotPrefab, inventorySlotsContainer);
                
                // Setup slot UI
                var slotImage = slot.GetComponent<Image>();
                var itemIcon = slot.transform.Find("ItemIcon")?.GetComponent<Image>();
                var itemText = slot.transform.Find("ItemText")?.GetComponent<TextMeshProUGUI>();
                var slotNumber = slot.transform.Find("SlotNumber")?.GetComponent<TextMeshProUGUI>();

                if (slotNumber != null)
                    slotNumber.text = (i + 1).ToString();

                // Add button component for clicking
                var button = slot.GetComponent<Button>();
                if (button != null)
                {
                    int slotIndex = i;
                    button.onClick.AddListener(() => OnSlotClicked(slotIndex));
                }

                slotObjects.Add(slot);
            }
        }

        private void FindLocalPlayerInventory()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
                return;

            var players = FindObjectsOfType<PlayerInventory>();
            foreach (var inventory in players)
            {
                if (inventory.IsOwner)
                {
                    playerInventory = inventory;
                    playerInventory.OnInventoryChanged += OnInventoryChanged;
                    break;
                }
            }
        }

        private void UpdateInventoryDisplay()
        {
            var items = playerInventory.GetAllItems();

            for (int i = 0; i < slotObjects.Count; i++)
            {
                if (slotObjects[i] == null) continue;

                var itemIcon = slotObjects[i].transform.Find("ItemIcon")?.GetComponent<Image>();
                var itemText = slotObjects[i].transform.Find("ItemText")?.GetComponent<TextMeshProUGUI>();

                if (i < items.Count && items[i] != null)
                {
                    // Slot has an item
                    ItemData itemData = items[i];
                    
                    if (itemIcon != null)
                    {
                        itemIcon.enabled = true;
                        
                        // Set item sprite/icon if available
                        if (itemData.icon != null)
                        {
                            itemIcon.sprite = itemData.icon;
                        }
                        
                        // Set color based on item category
                        itemIcon.color = GetItemCategoryColor(itemData.category);
                    }
                    
                    if (itemText != null)
                    {
                        itemText.text = itemData.itemName;
                    }
                }
                else
                {
                    // Empty slot
                    if (itemIcon != null)
                        itemIcon.enabled = false;
                    
                    if (itemText != null)
                        itemText.text = "";
                }
            }
        }

        private void HandleInventoryInput()
        {
            // Use number keys 1-5 to use items
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseItemInSlot(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) UseItemInSlot(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) UseItemInSlot(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) UseItemInSlot(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) UseItemInSlot(4);
        }

        private void OnSlotClicked(int slotIndex)
        {
            UseItemInSlot(slotIndex);
        }

        private void UseItemInSlot(int slotIndex)
        {
            if (playerInventory == null) return;
            
            ItemData itemData = playerInventory.GetItemAtSlot(slotIndex);
            if (itemData == null) return;

            Debug.Log($"[InventoryUI] Using item in slot {slotIndex}: {itemData.itemName}");
            playerInventory.UseItemServerRpc(slotIndex);
        }

        private void OnInventoryChanged()
        {
            UpdateInventoryDisplay();
        }

        private string GetItemName(ItemData itemData)
        {
            return itemData != null ? itemData.itemName : "Unknown";
        }
        
        private Color GetItemTypeColor(ItemType itemType)
        {
            // Use simpler category-based coloring
            return Color.white;  // Simple white for all items
        }
        
        private Color GetItemCategoryColor(ItemCategory category)
        {
            return category switch
            {
                ItemCategory.Consumable => Color.green,      // Green for consumables
                ItemCategory.Collectible => Color.cyan,      // Cyan for collectibles
                ItemCategory.Usable => Color.yellow,         // Yellow for tools
                ItemCategory.Key => new Color(1f, 0.8f, 0f), // Gold for keys
                _ => Color.white
            };
        }

        private void OnDestroy()
        {
            if (playerInventory != null)
            {
                playerInventory.OnInventoryChanged -= OnInventoryChanged;
            }
        }
    }
}

