using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blox.MainMenu
{
    public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Sprite normalSprite;
        public Sprite hoverSprite;

        private Button m_Button;

        private void Awake()
        {
            m_Button = GetComponent<Button>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Button.image.sprite = hoverSprite;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Button.image.sprite = normalSprite;
        }
    }
}