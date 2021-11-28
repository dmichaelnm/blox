using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS
{
    public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Sprite normal;
        [SerializeField] private Sprite hovered;

        private Image m_Image;

        private void Awake()
        {
            m_Image = GetComponent<Image>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Image.sprite = hovered;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            m_Image.sprite = normal;
        }
    }
}