using Blox.GameNS;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blox.UINS
{
    public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public interface IHandler
        {
            public void OnClicked(MainMenuButton src);
        }
        
        public string Name;

        [SerializeField] private MonoBehaviour m_Handler;
        [SerializeField] private Sprite m_NormalSprite;
        [SerializeField] private Sprite m_SelectedSprite;

        private Image m_Image;

        private void Awake()
        {
            m_Image = GetComponent<Image>();
            m_Image.sprite = m_NormalSprite;
        }

        public void OnButtonClicked()
        {
            m_Image.sprite = m_NormalSprite;
            if (m_Handler is IHandler h)
                h.OnClicked(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Image.sprite = m_SelectedSprite;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Image.sprite = m_NormalSprite;
        }
    }
}