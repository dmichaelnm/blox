using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component manages the appearance of the inventory.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        /// <summary>
        /// The number of slots in the inventory.
        /// </summary>
        public int SlotCount = 10;

        /// <summary>
        /// The prefab for an inventory slot.
        /// </summary>
        [SerializeField] private GameObject m_SlotPrefab;

        /// <summary>
        /// An array with all inventory slots.
        /// </summary>
        private InventorySlot[] m_Slots;

        /// <summary>
        /// The image of the inventory slots.
        /// </summary>
        private Image m_InventoryImage;
        
        /// <summary>
        /// Adds a new block to the inventory.
        /// </summary>
        /// <param name="blockTypeId">The type of the block</param>
        public bool AddBlock(int blockTypeId)
        {
            foreach (var slot in m_Slots)
            {
                if (slot.BlockTypeId == 0 || slot.BlockTypeId == blockTypeId)
                {
                    // We found a suitable slot to store that block, so add the block
                    slot.BlockTypeId = blockTypeId;
                    slot.Count++;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes a block from the selected slot.
        /// </summary>
        /// <returns>True, if removing the block was successful, otherwise false</returns>
        public bool RemoveBlock()
        {
            foreach (var slot in m_Slots)
            {
                if (slot.IsBlockRemovable)
                {
                    slot.Count--;
                    return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// Returns the ID of the currently selected block type in this inventory.
        /// </summary>
        /// <returns>The ID of the selected block type</returns>
        public int GetSelectedBlockTypeID()
        {
            return (from slot in m_Slots where slot.IsBlockRemovable select slot.BlockTypeId).FirstOrDefault();
        }

        /// <summary>
        /// Shows or hides the inventory.
        /// </summary>
        /// <param name="enabled">True to show or false to hide.</param>
        public void SetEnabled(bool enabled)
        {
            m_InventoryImage.enabled = enabled;
            foreach (var slot in m_Slots)
                slot.SetEnabled(enabled);
        }   
        
        /// <summary>
        /// This method is called when the component is created.
        /// </summary>
        private void Awake()
        {
            m_InventoryImage = GetComponent<Image>();
            
            // Create the inventory slots
            m_Slots = new InventorySlot[SlotCount];
            for (var i = 0; i < SlotCount; i++)
            {
                var slotObj = Instantiate(m_SlotPrefab, transform);
                slotObj.name = $"Slot {i}";
                m_Slots[i] = slotObj.GetComponent<InventorySlot>();
                m_Slots[i].BlockTypeId = 0;
            }
        }
        
        /// <summary>
        /// This method is called every frame.
        /// </summary>
        private void Update()
        {
            var slotIndex = -1;
            if (Input.GetKeyUp(KeyCode.Alpha1))
                slotIndex = 0;
            if (Input.GetKeyUp(KeyCode.Alpha2))
                slotIndex = 1;
            if (Input.GetKeyUp(KeyCode.Alpha3))
                slotIndex = 2;
            if (Input.GetKeyUp(KeyCode.Alpha4))
                slotIndex = 3;
            if (Input.GetKeyUp(KeyCode.Alpha5))
                slotIndex = 4;
            if (Input.GetKeyUp(KeyCode.Alpha6))
                slotIndex = 5;
            if (Input.GetKeyUp(KeyCode.Alpha7))
                slotIndex = 6;
            if (Input.GetKeyUp(KeyCode.Alpha8))
                slotIndex = 7;
            if (Input.GetKeyUp(KeyCode.Alpha9))
                slotIndex = 8;
            if (Input.GetKeyUp(KeyCode.Alpha0))
                slotIndex = 9;

            if (slotIndex > -1)
            {
                if (m_Slots[slotIndex].BlockTypeId > 0)
                {
                    for (var i = 0; i < m_Slots.Length; i++)
                        m_Slots[i].Selected = i == slotIndex;
                }
            }           
        }
    }
}