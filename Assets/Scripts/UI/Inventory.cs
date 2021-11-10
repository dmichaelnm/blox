using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    public class Inventory : MonoBehaviour
    {
        public int SlotCount = 10;

        public InventorySlot this[int index] => m_Slots[index];
        
        [SerializeField] private GameObject m_SlotPrefab;

        private InventorySlot[] m_Slots;
        private Image m_InventoryImage;
        
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

        public void SetBlock(int slotIndex, int blockTypeId, int count)
        {
            m_Slots[slotIndex].BlockTypeId = blockTypeId;
            m_Slots[slotIndex].Count = count;
        }
        
        public int GetSelectedBlockTypeID()
        {
            return (from slot in m_Slots where slot.IsBlockRemovable select slot.BlockTypeId).FirstOrDefault();
        }

        public void SetEnabled(bool enabled)
        {
            m_InventoryImage.enabled = enabled;
            foreach (var slot in m_Slots)
                slot.SetEnabled(enabled);
        }   
        
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
            
            gameObject.SetActive(false);
        }
        
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