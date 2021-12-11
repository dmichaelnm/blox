using System.Collections.Generic;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.GameNS;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS.CraftingWindow
{
    public class CraftingQueue : MonoBehaviour
    {
        internal class Product
        {
            public CreatableType Creatable;
            public float timer;
            public GameObject queueItem;
        }
        
        public int count => m_Products.Count;

        public event Events.CraftingQueueEvent onItemCrafted;
        
        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private GameObject m_QueueParent;
        [SerializeField] private GameObject m_QueuePrefab;
        
        private List<Product> m_Products;
    
        public void CreateItem(CreatableType creatableType)
        {
            var product = new Product();
            product.Creatable = creatableType;
            product.timer = creatableType.duration;
            product.queueItem = Instantiate(m_QueuePrefab, m_QueueParent.transform);
            UpdateQueueItem(product);
            
            m_Products.Add(product);

            foreach (var ingredient in creatableType.ingredients)
            {
                m_GameManager.inventory.RemoveItem(ingredient.entityType, ingredient.count);
            }
        }

        public void Load(JsonTextReader reader)
        {
            var config = Configuration.GetInstance();
            reader.ForEachObject("craftingQueue", index =>
            {
                reader.NextPropertyValue("entityId", out int entityId);
                reader.NextPropertyValue("timer", out float timer);

                var product = new Product();
                product.Creatable = config.GetEntityType<CreatableBlockType>(entityId);
                product.timer = timer;
                product.queueItem = Instantiate(m_QueuePrefab, m_QueueParent.transform);
                UpdateQueueItem(product);
                m_Products.Add(product);
            });
        }

        public void Save(JsonTextWriter writer)
        {
            writer.WritePropertyName("craftingQueue");
            writer.WriteStartArray();
            foreach (var product in m_Products)
            {
                writer.WriteStartObject();
                writer.WriteProperty("entityId", product.Creatable.id);
                writer.WriteProperty("timer", product.timer);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
        
        private void Awake()
        {
            m_Products = new List<Product>();
        }

        private void Update()
        {
            for (var i = m_Products.Count-1; i>=0; i--)
            {
                var product = m_Products[i];
                product.timer -= Time.deltaTime;
                if (product.timer <= 0f)
                {
                    m_Products.RemoveAt(i);
                    m_GameManager.inventory.AddItem(product.Creatable, product.Creatable.resultCount);
                    Destroy(product.queueItem);
                    onItemCrafted?.Invoke(this, product.Creatable);
                }
                else
                {
                    UpdateQueueItem(product);   
                }
            }
        }
        
        private void UpdateQueueItem(Product product)
        {
            var qi = product.queueItem;
            var image = qi.GetComponentInChildren<Image>();
            image.sprite = product.Creatable.icon;
            image.color = Color.white;
            var text = qi.GetComponentInChildren<Text>();
            text.text = Format.ToTimeStr(product.timer);
        }
    }
}