using Blox.ConfigurationNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component represents a single slot in the inventory.
    /// </summary>
    public class InventorySlot : MonoBehaviour
    {
        /// <summary>
        /// The ID of the block type stored by this slot.
        /// </summary>
        public int BlockTypeId
        {
            get => m_BlockTypeId;
            set
            {
                m_BlockTypeId = value;
                if (m_BlockTypeId == 0)
                {
                    SetEnabled(false);
                }
                else
                {
                    SetEnabled(true);
                    var config = Configuration.GetInstance();
                    var blockType = config.GetBlockType(m_BlockTypeId);
                    m_BlockIcon.sprite = m_Selected ? blockType.IconSelected : blockType.Icon;
                    m_Counter.text = $"{m_Count}";
                }
            }
        }

        /// <summary>
        /// Returns the count of blocks in this slot.
        /// </summary>
        public int Count
        {
            get => m_Count;
            set
            {
                m_Count = value;
                m_Counter.text = $"{m_Count}";
                if (m_Count == 0)
                {
                    BlockTypeId = 0;
                }
            }
        }

        /// <summary>
        /// Set the selected state of this slot. This property should not be set directly.
        /// </summary>
        public  bool Selected
        {
            get => m_Selected;
            set
            {
                var config = Configuration.GetInstance();
                var blockType = config.GetBlockType(m_BlockTypeId);
                m_Selected = value;
                m_BlockIcon.sprite = m_Selected ? blockType.IconSelected : blockType.Icon;
            }
        }

        /// <summary>
        /// Returns true when from this slot a block can be removed.
        /// </summary>
        public bool IsBlockRemovable => Selected && Count > 0;
        
        /// <summary>
        /// The icon the block type.
        /// </summary>
        [SerializeField] private Image m_BlockIcon;

        /// <summary>
        /// The count text of the block type    
        /// </summary>
        [SerializeField] private Text m_Counter;

        /// <summary>
        /// The internal block type ID.
        /// </summary>
        private int m_BlockTypeId;

        /// <summary>
        /// The internal number of blocks in this slot.
        /// </summary>
        private int m_Count;

        /// <summary>
        /// The selected flag for this slot.
        /// </summary>
        private bool m_Selected;

        /// <summary>
        /// Shows or hides this inventory slot.
        /// </summary>
        /// <param name="enabled">True to show or false to hide.</param>
        public void SetEnabled(bool enabled)
        {
            if (BlockTypeId > 0)
            {
                m_BlockIcon.enabled = enabled;
                m_Counter.enabled = enabled;
            }
            else
            {
                m_BlockIcon.enabled = false;
                m_Counter.enabled = false;
            }
        }
    }
}