using Blox.CommonNS;
using Blox.GameNS;
using UnityEngine;
using UnityEngine.UI;
using UserInterface.MainMenu;

namespace Blox.UserInterfaceNS.MainMenuNS
{
    public class MainMenu : FadingBehaviour
    {
        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private FadingBehaviour m_NewGame;
        [SerializeField] private Button m_ButtonNewGame;
        [SerializeField] private Button m_ButtonLoadGame;
        [SerializeField] private Button m_ButtonContinue;
        [SerializeField] private Button m_ButtonSaveGame;
        [SerializeField] private LoadGame m_LoadGame;

        private void OnEnable()
        {
            if (m_GameManager.state == GameManager.State.Paused)
            {
                m_ButtonLoadGame.gameObject.SetActive(false);
                m_ButtonContinue.gameObject.SetActive(true);
                m_ButtonSaveGame.gameObject.SetActive(true);
            }
            else
            {
                m_ButtonLoadGame.gameObject.SetActive(true);
                m_ButtonContinue.gameObject.SetActive(false);
                m_ButtonSaveGame.gameObject.SetActive(false);
            }
        }

        public void OnNewGameClick()
        {
            m_NewGame.gameObject.SetActive(true);
            FadeOut(state =>
            {
                m_NewGame.FadeIn();
                gameObject.SetActive(false);
            });
        }

        public void OnContinueClick()
        {
            m_GameManager.ResumeGame();
        }

        public void OnLoadClick()
        {
            m_LoadGame.gameObject.SetActive(true);
            FadeOut(state =>
            {
                m_LoadGame.FadeIn();
                m_LoadGame.FadeInLoadButtons();
                gameObject.SetActive(false);
            });
        }

        public void OnSaveClick()
        {
            m_GameManager.Save();
        }

        public void OnQuitClick()
        {
            Log.Info(this, "Application quit.");
            Application.Quit();
        }
    }
}