using System;
using System.Collections.Generic;
using System.Linq;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.GameNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS.CraftingWindow
{
    public class CraftingWindow : MonoBehaviour
    {
        public event Events.ComponentEvent<CraftingWindow> onOpen;
        public event Events.ComponentEvent<CraftingWindow> onClose;

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private CraftingQueue m_CraftingQueue;
        [SerializeField] private GameObject m_ItemParent;
        [SerializeField] private GameObject m_CreateItemPrefab;
        [SerializeField] private GameObject m_IngredientsParent;
        [SerializeField] private GameObject m_IngredientPrefab;
        [SerializeField] private GameObject m_Result;
        [SerializeField] private Text m_TimeValue;
        [SerializeField] private Button m_ButtonCreate;

        private CreatableType m_CreatableType;
        private List<CreatableType> m_Creatables;

        public void Open()
        {
            gameObject.SetActive(true);
            onOpen?.Invoke(this);
            RefreshList();
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

            m_ButtonCreate.gameObject.SetActive(m_CraftingQueue.count < 7);
        }

        public void OnCreateItem()
        {
            m_CraftingQueue.CreateItem(m_CreatableType);
            m_IngredientsParent.RemoveChildren();
            RefreshList();

            if (m_CraftingQueue.count == 7 || !m_Creatables.Contains(m_CreatableType))
                SetResultEntity(null);
        }

        private void Awake()
        {
            m_CraftingQueue.onItemCrafted += OnItemCrafted;
        }

        private void OnItemCrafted(CraftingQueue component, CreatableType creatable)
        {
            if (gameObject.activeSelf)
                RefreshList();
        }

        private void SetResultEntity(CreatableType creatable)
        {
            var resultImage = m_Result.GetComponentInChildren<Image>();
            resultImage.sprite = creatable != null ? creatable.icon : null;
            resultImage.color = creatable != null ? Color.white : Color.clear;
            var resultText = m_Result.GetComponentInChildren<Text>();
            resultText.text = creatable != null ? $"{creatable.resultCount}" : "";
            m_TimeValue.text = creatable != null ? $"{creatable.duration}s" : "";
            m_ButtonCreate.gameObject.SetActive(creatable != null);
        }

        private void RefreshList()
        {
            m_Creatables = new List<CreatableType>();
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

                    m_Creatables.Add(entity);
                }
            }
        }
    }
}