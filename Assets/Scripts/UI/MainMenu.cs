using System;
using Blox.EnvironmentNS;
using Blox.UtilitiesNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component manages the main menu.
    /// </summary>
    public class MainMenu : FadeBehaviour, IColorProvider, IEnableable
    {
        /// <summary>
        /// The chunk manager component.
        /// </summary>
        [SerializeField] private ChunkManager m_ChunkManager;

        /// <summary>
        /// The inventory component.
        /// </summary>
        [SerializeField] private Inventory m_Inventory;

        /// <summary>
        /// The "New Game" game object.
        /// </summary>
        [SerializeField] private GameObject m_NewGame;
        
        /// <summary>
        /// The background image of the main menu.
        /// </summary>
        private Image m_BackgroundImage;

        /// <summary>
        /// The title image of the main menu.
        /// </summary>
        private Image m_TitleImage;

        /// <summary>
        /// An array with all buttons of the main menu
        /// </summary>
        private MainMenuButton[] m_Buttons;

        /// <summary>
        /// This method is called when this component is created.
        /// </summary>
        private void Awake()
        {
            m_BackgroundImage = GetComponent<Image>();
            m_TitleImage = gameObject.GetChild("Title").GetComponent<Image>();
            m_Buttons = GetComponentsInChildren<MainMenuButton>();
        }

        /// <summary>
        /// This method is called when a main menu button was clicked.
        /// </summary>
        /// <param name="name">The name of the button</param>
        public void PerformButtonClick(string name)
        {
            if (name.Equals("QuitGame"))
                Application.Quit();
            else if (name.Equals("Proceed"))
            {
                FadeOut();
                m_ChunkManager.Unlock();
            }
            else if (name.Equals("NewGame"))
                NewGame();
        }

        /// <summary>
        /// Shows the main menu immediatly.
        /// </summary>
        public void ShowInitialMainMenu()
        {
            m_BackgroundImage.enabled = true;
            Iterate(comp =>
            {
                if (comp is IEnableable en)
                    en.SetEnabled(true);
            });
            m_Buttons[0].SetEnabled(false);
        }

        /// <summary>
        /// Hides the main menu immediatly
        /// </summary>
        public void HideImmediatly()
        {
            m_BackgroundImage.enabled = false;
            Iterate(comp =>
            {
                if (comp is IEnableable en)
                    en.SetEnabled(false);
            });
        }
        
        protected override void StartFade(State state)
        {
            m_BackgroundImage.enabled = false;
            m_Buttons[0].SetEnabled(true);
            m_Inventory.SetEnabled(state != State.FadeIn);

            Iterate(comp =>
            {
                if (comp is IColorProvider cp)
                    cp.SetColor(state == State.FadeIn ? Color.clear : Color.white);
                if (comp is IEnableable en)
                    en.SetEnabled(true);
            });
        }

        protected override void Fading(State state, float value)
        {
            Iterate(comp =>
            {
                if (comp is IColorProvider cp)
                {
                    cp.SetColor(new Color(1f, 1f, 1f, value));
                }
            });
        }

        protected override void EndFade(State state)
        {
            Iterate(comp =>
            {
                if (comp is IColorProvider cp)
                    cp.SetColor(state == State.FadeIn ? Color.white : Color.clear);
                if (comp is IEnableable en)
                    en.SetEnabled(state == State.FadeIn);
            });
        }

        /// <summary>
        /// Internal method to iterate over all elements of the main menu.
        /// </summary>
        /// <param name="comp"></param>
        private void Iterate(Action<object> comp)
        {
            comp.Invoke(this);
            foreach (var button in m_Buttons)
                comp.Invoke(button);
        }

        public Color GetColor()
        {
            return m_TitleImage.color;
        }

        public void SetColor(Color color)
        {
            m_TitleImage.color = color;
        }

        public void SetEnabled(bool enabled)
        {
            m_TitleImage.enabled = enabled;
        }

        /// <summary>
        /// Show the new game screen.
        /// </summary>
        private void NewGame()
        {
            m_NewGame.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}