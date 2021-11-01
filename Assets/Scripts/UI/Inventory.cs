using System;
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
        public int selectedBlockTypeId { get; private set; }
        
        [SerializeField] private GameObject m_SlotPrefab;
        [SerializeField] private AudioSource m_ErrorSound;
        
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

            if (slotIndex > -1)
                return true;
            
            m_ErrorSound.Play();
            return false;
        }

        public bool Remove(int blockTypeId)
        {
            for (var i = 0; i < m_Slots.Length; i++)
            {
                if (m_Slots[i].blockTypeId == blockTypeId)
                {
                    m_Slots[i].count--;
                    if (m_Slots[i].count == 0)
                    {
                        m_Slots[i].blockTypeId = 0;
                    }
                    return true;
                }
            }
            m_ErrorSound.Play();
            return false;
        }
        
        public void UpdateSelectionStates(InventorySlot slot)
        {
            selectedBlockTypeId = slot.blockTypeId;
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

        private void Update()
        {
            InventorySlot slot = null;
            if (Input.GetKeyUp(KeyCode.Alpha1))
                slot = m_Slots[0];
            else if (Input.GetKeyUp(KeyCode.Alpha2))
                slot = m_Slots[1];
            else if (Input.GetKeyUp(KeyCode.Alpha3))
                slot = m_Slots[2];
            else if (Input.GetKeyUp(KeyCode.Alpha4))
                slot = m_Slots[3];
            else if (Input.GetKeyUp(KeyCode.Alpha5))
                slot = m_Slots[4];
            else if (Input.GetKeyUp(KeyCode.Alpha6))
                slot = m_Slots[5];
            else if (Input.GetKeyUp(KeyCode.Alpha7))
                slot = m_Slots[6];
            else if (Input.GetKeyUp(KeyCode.Alpha8))
                slot = m_Slots[7];
            else if (Input.GetKeyUp(KeyCode.Alpha9))
                slot = m_Slots[8];
            else if (Input.GetKeyUp(KeyCode.Alpha0))
                slot = m_Slots[9];

            if (slot != null && slot.blockTypeId > 0)
            {
                UpdateSelectionStates(slot);                
            }
        }
    }
}