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
        
        [Header("Highlight Settings")]
        [SerializeField] private Color normalSlotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        [SerializeField] private Color selectedSlotColor = new Color(1f, 1f, 0f, 1f);
        [SerializeField] private float selectedSlotScale = 1.1f;

        private List<GameObject> slotObjects = new List<GameObject>();
        private PlayerInventory playerInventory;
        private int currentSelectedSlot = 0;

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
                    playerInventory.OnSelectedSlotChanged += OnSelectedSlotChanged;
                    
                    // Initialize selection
                    currentSelectedSlot = inventory.GetSelectedSlot();
                    UpdateSlotHighlights();
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
            // NOTE: Slot selection is now handled by PlayerItemUsage
            // This method is kept for backwards compatibility but does nothing
        }

        private void OnSlotClicked(int slotIndex)
        {
            // Clicking a slot now selects it instead of using it
            if (playerInventory != null)
            {
                playerInventory.SetSelectedSlot(slotIndex);
            }
        }

        private void OnInventoryChanged()
        {
            UpdateInventoryDisplay();
        }
        
        private void OnSelectedSlotChanged(int newSlotIndex)
        {
            currentSelectedSlot = newSlotIndex;
            UpdateSlotHighlights();
        }
        
        private void UpdateSlotHighlights()
        {
            for (int i = 0; i < slotObjects.Count; i++)
            {
                if (slotObjects[i] == null) continue;
                
                var slotImage = slotObjects[i].GetComponent<Image>();
                if (slotImage != null)
                {
                    // Set color based on selection
                    slotImage.color = (i == currentSelectedSlot) ? selectedSlotColor : normalSlotColor;
                }
                
                // Set scale based on selection
                float scale = (i == currentSelectedSlot) ? selectedSlotScale : 1f;
                slotObjects[i].transform.localScale = Vector3.one * scale;
            }
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
                playerInventory.OnSelectedSlotChanged -= OnSelectedSlotChanged;
            }
        }
    }
}

