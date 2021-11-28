using System.Collections.Generic;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.GameNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS.CraftingWindow
{
    public class CraftingWindow : MonoBehaviour
    {
        internal class Product
        {
            public CreatableType creatable;
            public float timer;
            public GameObject queueItem;
        }

        public event Events.ComponentEvent<CraftingWindow> onOpen;
        public event Events.ComponentEvent<CraftingWindow> onClose;

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private GameObject m_ItemParent;
        [SerializeField] private GameObject m_CreateItemPrefab;
        [SerializeField] private GameObject m_IngredientsParent;
        [SerializeField] private GameObject m_IngredientPrefab;
        [SerializeField] private GameObject m_QueueParent;
        [SerializeField] private GameObject m_QueuePrefab;
        [SerializeField] private GameObject m_Result;
        [SerializeField] private Text m_TimeValue;
        [SerializeField] private Button m_ButtonCreate;

        private CreatableType m_CreatableType;
        private List<Product> m_Products;

        public void Open()
        {
            gameObject.SetActive(true);
            onOpen?.Invoke(this);

            m_ItemParent.RemoveChildren();
            var entities = Configuration.GetInstance().GetEntities<CreatableType>();
            foreach (var entity in entities)
            {
                var complete = true;
                foreach (var ingredient in entity.ingredients)
                {
                    var count = m_GameManager.inventory.Contains(ingredient.entityType);
                    if (count < ingredient.count)
                    {
                        complete = false;
                        break;
                    }
                }

                if (complete)
                {
                    var obj = Instantiate(m_CreateItemPrefab, m_ItemParent.transform);
                    var createItemButton = obj.GetComponent<CreateItemButton>();
                    createItemButton.itemImage.sprite = entity.icon;
                    createItemButton.itemName.text = entity.name;
                    createItemButton.craftingWindow = this;
                    createItemButton.creatable = entity;
                }
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            onClose?.Invoke(this);
        }

        public void OnChooseItem(CreatableType creatable)
        {
            m_CreatableType = creatable;

            m_IngredientsParent.RemoveChildren();
            foreach (var ingredient in creatable.ingredients)
            {
                var obj = Instantiate(m_IngredientPrefab, m_IngredientsParent.transform);
                var image = obj.GetComponentInChildren<Image>();
                image.sprite = ingredient.entityType.icon;
                var text = obj.GetComponentInChildren<Text>();
                text.text = $"{ingredient.count}";
            }

            SetResultEntity(creatable);

            m_ButtonCreate.gameObject.SetActive(m_Products.Count < 8);
        }

        public void OnCreateItem()
        {
            var product = new Product();
            product.creatable = m_CreatableType;
            product.timer = m_CreatableType.duration;
            product.queueItem = Instantiate(m_QueuePrefab, m_QueueParent.transform);
            UpdateQueueItem(product);
            
            m_Products.Add(product);
            
            m_IngredientsParent.RemoveChildren();
            SetResultEntity(null);
            m_ButtonCreate.gameObject.SetActive(false);
            
            foreach (var ingredient in m_CreatableType.ingredients)
            {
                m_GameManager.inventory.RemoveItem(ingredient.entityType, ingredient.count);
            }
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
                    m_GameManager.inventory.AddItem(product.creatable, product.creatable.resultCount);
                    Destroy(product.queueItem);
                }
                else
                {
                    UpdateQueueItem(product);   
                }
            }
        }

        private void SetResultEntity(CreatableType creatable)
        {
            var resultImage = m_Result.GetComponentInChildren<Image>();
            resultImage.sprite = creatable != null ? creatable.icon : null;
            resultImage.color = creatable != null ? Color.white : Color.clear;
            var resultText = m_Result.GetComponentInChildren<Text>();
            resultText.text = creatable != null ? $"{creatable.resultCount}" : "";
            m_TimeValue.text = creatable != null ? $"{creatable.duration}s" : "";
        }

        private void UpdateQueueItem(Product product)
        {
            var qi = product.queueItem;
            var image = qi.GetComponentInChildren<Image>();
            image.sprite = product.creatable.icon;
            image.color = Color.white;
            var text = qi.GetComponentInChildren<Text>();
            text.text = $"{product.timer:F0}";
        }
    }
}