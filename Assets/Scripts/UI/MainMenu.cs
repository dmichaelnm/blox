using System;
using System.IO;
using System.IO.Compression;
using Blox.EnvironmentNS;
using Blox.GameNS;
using Blox.PlayerNS;
using Blox.UtilitiesNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.UINS
{
    public class MainMenu : FadeBehaviour, MainMenuButton.IHandler
    {
        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private GameObject m_Inventory;
        [SerializeField] private GameObject m_NewGame;
        [SerializeField] private GameObject m_ProceedButton;
        [SerializeField] private GameObject m_RotationCamera;
        [SerializeField] private GameObject m_PlayerController;
        [SerializeField] private GameObject m_LoadScreen;

        public void SwitchToMainMenu()
        {
            m_Inventory.SetActive(false);
            m_ProceedButton.SetActive(true);
            m_PlayerController.SetActive(false);
            m_RotationCamera.SetActive(true);
            FadeIn();
        }

        public void SwitchToGame()
        {
            FadeOut(state =>
            {
                m_ChunkManager.Unlock();
                m_Inventory.SetActive(true);
                m_PlayerController.SetActive(true);
                m_RotationCamera.SetActive(false);
                gameObject.SetActive(false);
            });
        }
        
        public void OnClicked(MainMenuButton src)
        {
            if (src.Name.Equals("QuitGame"))
                Application.Quit();
            else if (src.Name.Equals("Proceed"))
                SwitchToGame();
            else if (src.Name.Equals("NewGame"))
                NewGame();
            else if (src.Name.Equals("SaveGame"))
                SaveGame();
            else if (src.Name.Equals("LoadGame"))
                SwitchToLoadGameScreen();
        }
        
        private void NewGame()
        {
            m_NewGame.SetActive(true);
            gameObject.SetActive(false);
        }

        public void SaveGame()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var srcFolder = folder + "/My Games/Blox/" + Game.CurrentName;
            var dstFile = srcFolder + ".zip";
            if (!Directory.Exists(srcFolder))
                Directory.CreateDirectory(srcFolder);
            
            // Copy all cache files
            var files = Directory.GetFiles(Game.TemporaryDirectory);
            foreach (var file in files)
            {
                var target = srcFolder + "/" + Path.GetFileName(file);
                File.Copy(file, target);
            }
            
            // Save all chunks in cache
            m_ChunkManager.SaveCacheTo(srcFolder);

            using (var w = new JsonTextWriter(new StreamWriter(srcFolder + "/properties.json")))
            {
                var playerController = m_PlayerController.GetComponent<PlayerController>();
                var inventory = m_Inventory.GetComponent<Inventory>();
                
                w.WriteStartObject();
                w.WritePropertyName("player");
                w.WriteStartObject();
                w.WriteProperty("positionX", playerController.PlayerPosition.CurrentPosition.x);
                w.WriteProperty("positionY", playerController.PlayerPosition.CurrentPosition.y);
                w.WriteProperty("positionZ", playerController.PlayerPosition.CurrentPosition.z);
                w.WriteProperty("forwardX", playerController.PlayerPosition.CameraForward.x);
                w.WriteProperty("forwardY", playerController.PlayerPosition.CameraForward.y);
                w.WriteProperty("forwardZ", playerController.PlayerPosition.CameraForward.z);
                w.WriteEndObject();
                w.WritePropertyName("inventory");
                w.WriteStartObject();
                w.WriteProperty("slotCount", inventory.SlotCount);
                for (var i = 0; i < inventory.SlotCount; i++)
                {
                    var slot = inventory[i];
                    w.WritePropertyName("slot" + i);
                    w.WriteStartObject();
                    w.WriteProperty("blocktype", slot.BlockTypeId);
                    w.WriteProperty("count", slot.Count);
                    w.WriteEndObject();
                }
                w.WriteEndObject();
                w.WriteEndObject();
            }
            
            File.Delete(dstFile);
            ZipFile.CreateFromDirectory(srcFolder, dstFile);
            Array.ForEach(Directory.GetFiles(srcFolder), File.Delete);
            Directory.Delete(srcFolder);
            
            SwitchToGame();
        }

        public void SwitchToLoadGameScreen()
        {
            FadeOut(state =>
            {
                m_LoadScreen.SetActive(true);
                var comp = m_LoadScreen.GetComponent<LoadGame>();
                comp.ShowSaveGames();
                comp.FadeIn();
                gameObject.SetActive(false);
            });           
        }
        
        protected override void OnAwake()
        {
            m_ProceedButton.SetActive(false);
        }
   }
}