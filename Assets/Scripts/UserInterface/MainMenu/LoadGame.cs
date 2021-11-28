using System.Collections.Generic;
using System.IO;
using Blox.CommonNS;
using Blox.GameNS;
using Blox.UserInterfaceNS;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.MainMenu
{
    public class LoadGame : FadingBehaviour
    {
        public GameObject loadButtonPrefab;
        
        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private FadingBehaviour m_MainMenu;
        [SerializeField] private GameObject m_Content;

        private List<LoadButton> m_LoadButtons;

        public void Load(string saveGameFile)
        {
            m_GameManager.Load(saveGameFile);            
        }
        
        protected override void _Awake()
        {
            m_LoadButtons = new List<LoadButton>();
        }

        private void OnEnable()
        {
            m_Content.RemoveChildren();
            m_LoadButtons.Clear();
            
            var path = GameManager.SaveGameDirectory;
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var gameName = Path.GetFileName(file);
                gameName = gameName.Substring(0, gameName.Length - 4);
                var buttonObj = Instantiate(loadButtonPrefab, m_Content.transform);
                var loadButton = buttonObj.GetComponentInChildren<LoadButton>();
                loadButton.loadGame = this;
                loadButton.saveGameFile = file;
                var text = buttonObj.GetComponentInChildren<Text>();
                text.text = gameName;
                m_LoadButtons.Add(loadButton);
            }
        }

        public void FadeInLoadButtons()
        {
            foreach (var loadButton in m_LoadButtons)
                loadButton.FadeIn();            
        }
        
        public void OnBackClick()
        {
            m_MainMenu.gameObject.SetActive(true);
            FadeOut(state =>
            {
                m_MainMenu.FadeIn();
                gameObject.SetActive(false);
            });
            foreach (var loadButton in m_LoadButtons)
                loadButton.FadeOut();
        }
    }
}