using System;
using Blox.ConfigurationNS;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS.CraftingWindow
{
    public class CreateItemButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public CraftingWindow craftingWindow;
        public CreatableType creatable;
        public Image itemImage;
        public Text itemName;
        public Color selectedBackground;
        
        private Image m_ButtonImage;

        private void Awake()
        {
            m_ButtonImage = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_ButtonImage.color = selectedBackground;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_ButtonImage.color = Color.clear;
        }

        public void OnClick()
        {
            craftingWindow.OnChooseItem(creatable);            
        }
    }
}