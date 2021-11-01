using UnityEngine;
using UnityEngine.SceneManagement;

namespace Blox.MainMenu
{
    public class MainMenuHandler : MonoBehaviour
    {
        public void NewGame()
        {
            SceneManager.LoadScene("GameScene");
        }
        
        public void Quit()
        {
            Application.Quit();
        }
    }
}