using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component handles a main menu button.
    /// </summary>
    public class MainMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IColorProvider, IEnableable
    {
        /// <summary>
        /// The name of the button.
        /// </summary>
        public string Name;
        
        /// <summary>
        /// The sprite of the not hovered button.
        /// </summary>
        [SerializeField] private Sprite m_NormalSprite;

        /// <summary>
        /// The sprite of the hovered button.
        /// </summary>
        [SerializeField] private Sprite m_SelectedSprite;

        /// <summary>
        /// The image of the button.
        /// </summary>
        private Image m_Image;

        /// <summary>
        /// The button component.
        /// </summary>
        private Button m_Button;

        /// <summary>
        /// The main menu component.
        /// </summary>
        private MainMenu m_MainMenu;

        /// <summary>
        /// This method is called when this component is created.
        /// </summary>
        private void Awake()
        {
            m_Image = GetComponent<Image>();
            m_Image.sprite = m_NormalSprite;
            m_Button = GetComponent<Button>();
            m_MainMenu = GetComponentInParent<MainMenu>();
        }

        public void OnButtonClicked()
        {
            m_MainMenu.PerformButtonClick(Name);
        }
        
        /// <summary>
        /// Shows or hide this button.
        /// </summary>
        /// <param name="enabled">True to show the button or false to hide the button</param>
        public void SetEnabled(bool enabled)
        {
            m_Image.enabled = enabled;
            m_Button.enabled = enabled;
        }

        /// <summary>
        /// Show the hover sprite when the mouse is over the button.
        /// </summary>
        /// <param name="eventData">Event data</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            m_Image.sprite = m_SelectedSprite;
        }

        /// <summary>
        /// Show the normal sprite when the mouse is not over the button. 
        /// </summary>
        /// <param name="eventData">Event data</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            m_Image.sprite = m_NormalSprite;
        }

        public Color GetColor()
        {
            return m_Image.color;
        }

        public void SetColor(Color color)
        {
            m_Image.color = color;
        }
    }
}