using System;
using System.IO;
using System.IO.Compression;
using Blox.EnvironmentNS;
using Blox.EnvironmentNS.GeneratorNS;
using Blox.GameNS;
using Blox.PlayerNS;
using Blox.UtilitiesNS;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Blox.UINS
{
    public class LoadGame : FadeBehaviour, MainMenuButton.IHandler
    {
        [SerializeField] private GameObject m_MainMenu;
        [SerializeField] private GameObject m_ContentObject;
        [SerializeField] private GameObject m_ButtonPrefab;
        [SerializeField] private GameObject m_Inventory;
        [SerializeField] private LoadingScreen m_LoadingScreen;
        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private PlayerController m_PlayerController;

        public void OnClicked(MainMenuButton src)
        {
            if (src.Name.Equals("Back"))
                Back();
        }

        public void ShowSaveGames()
        {
            var files = Directory.GetFiles(Game.SaveGameDirectory);
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                if (filename.EndsWith(".zip"))
                {
                    var button = Instantiate(m_ButtonPrefab, m_ContentObject.transform).GetComponent<Button>();
                    var text = button.GetComponentInChildren<Text>();
                    text.text = filename.Substring(0, filename.Length - 4);
                    button.onClick.AddListener(() => LoadSavedGame(file));
                }
            }
        }

        private void Back()
        {
            FadeOut(state =>
            {
                m_MainMenu.SetActive(true);
                var comp = m_MainMenu.GetComponent<MainMenu>();
                comp.FadeIn();
                gameObject.SetActive(false);
                m_ContentObject.transform.IterateInverse(child => Destroy(child.gameObject));
            });
        }

        private void LoadSavedGame(string path)
        {
            var filename = Path.GetFileName(path);
            Game.CurrentName = filename.Substring(0, filename.Length - 4);
            var tempFolder = Game.SaveGameDirectory + "/.temp";
            Directory.CreateDirectory(tempFolder);
            ZipFile.ExtractToDirectory(path, tempFolder);
            var propertiesFile = tempFolder + "/properties.json";
            var content = File.ReadAllText(propertiesFile);
            var generatorParams = new GeneratorParams();
            var inventory = m_Inventory.GetComponent<Inventory>();
            using (var reader = new JsonTextReader(new StringReader(content)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("player");
                reader.NextTokenIsStartObject();
                reader.NextPropertyValue("positionX", out float positionX);
                reader.NextPropertyValue("positionY", out float positionY);
                reader.NextPropertyValue("positionZ", out float positionZ);
                m_PlayerController.InitialPosition = new Vector3(positionX, positionY, positionZ);
                reader.NextPropertyValue("rotationX", out float rotationX);
                reader.NextPropertyValue("rotationY", out float rotationY);
                m_PlayerController.InitialRotation = new Vector2(rotationX, rotationY);
                reader.NextTokenIsEndObject();
                reader.NextPropertyNameIs("inventory");
                reader.NextTokenIsStartObject();
                reader.NextPropertyValue("slotCount", out int slotCount);
                Assert.AreEqual(slotCount, inventory.SlotCount);
                for (var i = 0; i < inventory.SlotCount; i++)
                {
                    reader.NextPropertyNameIs("slot" + i);
                    reader.NextTokenIsStartObject();
                    reader.NextPropertyValue("blocktype", out int blockTypeId);
                    reader.NextPropertyValue("count", out int count);
                    reader.NextTokenIsEndObject();
                    inventory.SetBlock(i, blockTypeId, count);
                }

                reader.NextTokenIsEndObject();
                generatorParams.Read(reader);
                reader.NextTokenIsEndObject();
            }

            File.Delete(propertiesFile);
            var files = Directory.GetFiles(tempFolder);
            Array.ForEach(files, file =>
            {
                var fileName = Path.GetFileName(file);
                File.Move(file, Game.TemporaryDirectory + "/" + fileName);
            });
            Directory.Delete(tempFolder);

            FadeOut();
            m_LoadingScreen.Show(() =>
            {
                m_Inventory.SetActive(true);
                m_ChunkManager.StartNewGame(generatorParams);
                gameObject.SetActive(false);
            });
        }
    }
}