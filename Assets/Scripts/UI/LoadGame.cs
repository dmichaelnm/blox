using UnityEngine;

namespace Blox.UINS
{
    public class LoadGame : FadeBehaviour, MainMenuButton.IHandler
    {
        [SerializeField] private GameObject m_MainMenu;
        
        public void OnClicked(MainMenuButton src)
        {
            if (src.Name.Equals("Back"))
                Back();
        }

        public void ShowSaveGames()
        {
            
        }
        
        private void Back()
        {
            FadeOut(state =>
            {
                m_MainMenu.SetActive(true);
                var comp = m_MainMenu.GetComponent<MainMenu>();
                comp.FadeIn();
                gameObject.SetActive(false);
            });
        }
    }
}