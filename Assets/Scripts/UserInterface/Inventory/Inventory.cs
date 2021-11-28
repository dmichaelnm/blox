using System.Linq;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.UserInterfaceNS.InventoryNS
{
    public class Inventory : MonoBehaviour
    {
        private static readonly KeyCode[] _SlotCodes =
        {
            KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
            KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0
        };

        public int slotCount;
        public int slotSize;
        public GameObject slotPrefab;

        public InventorySlot selectedSlot => selectedIndex != -1 ? m_Slots[selectedIndex] : null;

        private InventorySlot[] m_Slots;
        private int selectedIndex = -1;

        public bool AddItem(EntityType type, int count = 1)
        {
            return m_Slots.Any(slot => slot.AddItem(type, count));
        }

        public bool RemoveItem(EntityType entityType, int count = 1)
        {
            foreach (var slot in m_Slots)
            {
                if (entityType.Equals(slot.entityType) && slot.RemoveItem(count))
                    return true;
            }

            return false;
        }
        
        public bool RemoveItem(out EntityType entityType, int count = 1)
        {
            entityType = default;
            
            if (selectedSlot != null && selectedSlot.RemoveItem(out entityType))
            {
                if (selectedSlot.count == 0)
                {
                    selectedSlot.Mark(false);
                    selectedIndex = -1;
                }
                
                return true;
            }

            return false;
        }

        public int Contains(EntityType entityType)
        {
            return (from slot in m_Slots where slot.entityType.Equals(entityType) select slot.count).FirstOrDefault();
        }
        
        public void ResetInventory()
        {
            foreach (var slot in m_Slots)
                slot.ResetSlot();
        }
        
        public void Load(JsonTextReader reader)
        {
            reader.NextPropertyNameIs("inventory");
            reader.NextTokenIsStartObject();
            for (var i = 0; i < m_Slots.Length; i++)
            {
                reader.NextPropertyNameIs($"slot{i}");
                reader.NextTokenIsStartObject();
                m_Slots[i].Load(reader);
                reader.NextTokenIsEndObject();
            }
            reader.NextTokenIsEndObject();
        }
        
        public void Save(JsonTextWriter writer)
        {
            writer.WritePropertyName("inventory");
            writer.WriteStartObject();
            for (var i = 0; i < m_Slots.Length; i++)
            {
                writer.WritePropertyName($"slot{i}");
                writer.WriteStartObject();
                m_Slots[i].Save(writer);
                writer.WriteEndObject();
            }
            writer.WriteEndObject();
        }
        
        private void Awake()
        {
            for (var i = 0; i < slotCount; i++)
                Instantiate(slotPrefab, transform);

            m_Slots = GetComponentsInChildren<InventorySlot>();
        }

        private void Update()
        {
            for (var i = 0; i < _SlotCodes.Length; i++)
            {
                if (Input.GetKeyDown(_SlotCodes[i]))
                {
                    MarkSlot(i);
                    break;
                }
            }
        }

        private void MarkSlot(int index)
        {
            if (m_Slots[index].Mark(true))
            {
                selectedIndex = index;
                for (var i = 0; i < m_Slots.Length; i++)
                {
                    if (i != index)
                        m_Slots[i].Mark(false);
                }
            }
        }
    }
}