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
                    m_BlockImage.sprite = blockType.iconNormal;
                    m_BlockImage.color = Color.white;
                }
                else
                {
                    m_BlockImage.sprite = null;
                    m_BlockImage.color = new Color(0, 0, 0, 0);
                    m_BlockText.text = "";
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

        [SerializeField] private Image m_BlockImage;
        [SerializeField] private Text m_BlockText;

        private int m_Count;
        private int m_BlockTypeId;
        private Inventory m_Inventory;

        private void Awake()
        {
            m_Inventory = GetComponentInParent<Inventory>();
            m_Inventory.onInventorySlotSelected += OnInventorySlotSelected; 
        }

        private void OnInventorySlotSelected(InventorySlot slot)
        {
            var config = Configuration.GetInstance();
            var blockType = config.GetBlockType(m_BlockTypeId);

            if (m_BlockTypeId == slot.blockTypeId)
                m_BlockImage.sprite = blockType.iconSelected;
            else
                m_BlockImage.sprite = blockType.iconNormal;
        }
    }
}