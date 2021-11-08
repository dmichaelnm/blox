using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blox.UINS
{
    public class LoadGame : FadeBehaviour, MainMenuButton.IHandler
    {
        [SerializeField] private GameObject m_MainMenu;
        [SerializeField] private ScrollbarEditor m_ScrollView;
        
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