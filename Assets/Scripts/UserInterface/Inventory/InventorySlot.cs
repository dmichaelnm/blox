using Blox.CommonNS;
using Blox.ConfigurationNS;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS.InventoryNS
{
    public class InventorySlot : MonoBehaviour
    {
        public EntityType entityType { get; private set; }
        public int count { get; private set; }

        [SerializeField] private Image m_ItemImage;
        [SerializeField] private Text m_ItemCount;
        [SerializeField] private Image m_Marker;

        private Inventory m_Inventory;

        public bool AddItem(EntityType type, int count = 1)
        {
            if (entityType == null || type.Equals(entityType))
            {
                entityType = type;
                if (m_Inventory.slotSize > count)
                {
                    this.count += count;
                    UpdateSlot();
                    return true;
                }
            }

            return false;
        }

        public bool RemoveItem(int count = 1)
        {
            return RemoveItem(out var t, count);
        }
        
        public bool RemoveItem(out EntityType entity, int count = 1)
        {
            entity = default;
            if (this.count < count)
                return false;

            this.count -= count;
            entity = entityType;
            if (this.count == 0)
                entityType = null;

            UpdateSlot();
            return true;
        }

        public bool Mark(bool selected)
        {
            if (selected && entityType == null)
                return false;

            m_Marker.enabled = selected;
            return true;
        }

        public void ResetSlot()
        {
            entityType = null;
            count = 0;
            UpdateSlot();
        }
        
        public void Load(JsonTextReader reader)
        {
            reader.NextPropertyValue("entityId", out int id);
            reader.NextPropertyValue("count", out int c);

            var config = Configuration.GetInstance();
            entityType = id == -1 ? null : config.GetEntityType<EntityType>(id);
            count = c;
            
            UpdateSlot();
        }
        
        public void Save(JsonTextWriter writer)
        {
            writer.WriteProperty("entityId", entityType != null ? entityType.id : -1);
            writer.WriteProperty("count", count);
        }
        
        private void Awake()
        {
            m_Inventory = GetComponentInParent<Inventory>();
        }

        private void UpdateSlot()
        {
            if (entityType == null)
            {
                m_ItemImage.sprite = null;
                m_ItemImage.color = Color.clear;
                m_ItemCount.text = "";
            }
            else
            {
                m_ItemImage.sprite = entityType.icon;
                m_ItemImage.color = Color.white;
                m_ItemCount.text = $"{count}";
            }
        }
    }
}