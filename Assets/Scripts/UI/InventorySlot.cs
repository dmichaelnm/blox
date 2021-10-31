using System;
using Blox.Environment.Config;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UI
{
    public class InventorySlot : MonoBehaviour
    {
        public int blockTypeId
        {
            get => m_BlockTypeId;
            set
            {
                m_BlockTypeId = value;
                if (m_BlockTypeId > 0)
                {
                    var config = Configuration.GetInstance();
                    var blockType = config.GetBlockType(m_BlockTypeId);
                    m_BlockButton.image.sprite = blockType.iconNormal;
                    m_BlockButton.image.color = Color.white;
                }
            }   
        }

        public int count
        {
            get => m_Count;
            set
            {
                m_Count = value;
                m_BlockText.text = m_Count.ToString();
            }
        }

        [SerializeField] private Button m_BlockButton;
        [SerializeField] private Text m_BlockText;

        private int m_Count;
        private int m_BlockTypeId;
        private Inventory m_Inventory;

        private void Awake()
        {
            m_Inventory = GetComponentInParent<Inventory>();
            m_Inventory.onInventorySlotSelected += OnInventorySlotSelected; 
            m_BlockButton.onClick.AddListener(OnButtonClicked);
        }

        private void OnInventorySlotSelected(InventorySlot slot)
        {
            var config = Configuration.GetInstance();
            var blockType = config.GetBlockType(m_BlockTypeId);

            if (m_BlockTypeId == slot.blockTypeId)
                m_BlockButton.image.sprite = blockType.iconSelected;
            else
                m_BlockButton.image.sprite = blockType.iconNormal;
        }

        private void OnButtonClicked()
        {
            m_Inventory.UpdateSelectionStates(this);
        }
    }
}