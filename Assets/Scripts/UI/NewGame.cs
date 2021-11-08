using Blox.ConfigurationNS;
using Blox.EnvironmentNS;
using Blox.GameNS;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

namespace Blox.UINS
{
    public class NewGame : FadeBehaviour, MainMenuButton.IHandler
    {
        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private GameObject m_MainMenu;
        [SerializeField] private InputField m_WorldName;
        [SerializeField] private Button m_Landscape;
        [SerializeField] private InputField m_RandomSeed;
        [SerializeField] private LoadingScreen m_LoadingScreen;
        [SerializeField] private GameObject m_Inventory;

        private TerrainGeneratorPreset[] m_Presets;
        private int m_PresetIndex;

        protected override void OnAwake()
        {
            m_WorldName.text = "Neue Welt";

            var config = Configuration.GetInstance();
            m_Presets = config.GetTerrainGeneratorPresets();

            var landscapeText = m_Landscape.GetComponentInChildren<Text>();
            landscapeText.text = m_Presets[0].Name;

            m_RandomSeed.text = new Random().Next().ToString();
        }

        public void OnClicked(MainMenuButton src)
        {
            if (src.Name.Equals("StartGame"))
                StartGame();
            else if (src.Name.Equals("Back"))
                Back();
        }

        public void SwitchPreset()
        {
            m_PresetIndex++;
            if (m_PresetIndex == m_Presets.Length)
                m_PresetIndex = 0;

            var landscapeText = m_Landscape.GetComponentInChildren<Text>();
            landscapeText.text = m_Presets[m_PresetIndex].Name;
        }

        private void StartGame()
        {
            int.TryParse(m_RandomSeed.text, out var randomSeed);

            var random = new Random(randomSeed);
            var generatorParams = m_Presets[m_PresetIndex].GeneratorParams;
            generatorParams.randomSeed = randomSeed;
            generatorParams.Terrain.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));
            generatorParams.Water.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));
            generatorParams.Stone.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));
            generatorParams.Tree.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));

            Game.CurrentName = m_WorldName.text;
            
            FadeOut();
            m_LoadingScreen.Show(() =>
            {
                m_Inventory.SetActive(true);
                m_ChunkManager.StartNewGame(generatorParams);
                gameObject.SetActive(false);
            });
        }
         
        private void Back()
        {
            m_MainMenu.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}