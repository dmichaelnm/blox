using Blox.GameNS;
using Blox.UserInterfaceNS;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface.MainMenu
{
    public class LoadButton : FadingBehaviour
    {
        public LoadGame loadGame;
        public string saveGameFile;
        
        public void OnLoadClick()
        {
            loadGame.Load(saveGameFile);
        }
    }
}