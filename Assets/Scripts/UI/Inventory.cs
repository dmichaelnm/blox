using Blox.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UI
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory GetInstance()
        {
            return GameObject.Find("Inventory").GetComponent<Inventory>();
        }

        public delegate void InventorySlotSelected(InventorySlot slot);
        public event InventorySlotSelected onInventorySlotSelected; 
        
        [SerializeField] private GameObject m_SlotPrefab;
        private Image m_InventoryImage;
        private InventorySlot[] m_Slots;

        public bool Put(int blockTypeId)
        {
            var slotIndex = -1;
            for (var i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i].blockTypeId == 0 || m_Slots[i].blockTypeId == blockTypeId)
                {
                    slotIndex = i;
                    break;
                }
            }

            m_Slots[slotIndex].blockTypeId = blockTypeId;
            m_Slots[slotIndex].count++;
            
            return slotIndex > -1;
        }

        public void UpdateSelectionStates(InventorySlot slot)
        {
            onInventorySlotSelected?.Invoke(slot);
        }

        private void Awake()
        {
            var chunkManager = GameObject.Find("Chunk Manager").GetComponent<ChunkManager>();
            chunkManager.onInitialized += OnChunkManagerInitialized;

            m_InventoryImage = GetComponent<Image>();

            for (var i = 0; i < 10; i++)
                Instantiate(m_SlotPrefab, transform);
            
            m_Slots = GetComponentsInChildren<InventorySlot>();
        }

        private void OnChunkManagerInitialized()
        {
            m_InventoryImage.enabled = true;
        }
    }
}