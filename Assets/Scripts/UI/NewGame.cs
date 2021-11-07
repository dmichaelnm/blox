using Blox.ConfigurationNS;
using Blox.EnvironmentNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component manages the new game UI.
    /// </summary>
    public class NewGame : MonoBehaviour
    {
        /// <summary>
        /// The chunk manager component.
        /// </summary>
        [SerializeField] private ChunkManager m_ChunkManager;

        /// <summary>
        /// The game object containing the "Main Menu".
        /// </summary>
        [SerializeField] private GameObject m_MainMenu;

        /// <summary>
        /// The world name input field.
        /// </summary>
        [SerializeField] private InputField m_WorldName;

        /// <summary>
        /// The landscape button.
        /// </summary>
        [SerializeField] private Button m_Landscape;

        /// <summary>
        /// The random seed input field
        /// </summary>
        [SerializeField] private InputField m_RandomSeed;
        
        /// <summary>
        /// The array with the landscape presets.
        /// </summary>
        private TerrainGeneratorPreset[] m_Presets;

        /// <summary>
        /// The current preset index.
        /// </summary>
        private int m_PresetIndex;
        
        /// <summary>
        /// This method is called when this component is created.
        /// </summary>
        private void Awake()
        {
            m_WorldName.text = "Neue Welt";
            
            var config = Configuration.GetInstance();
            m_Presets = config.GetTerrainGeneratorPresets();
            
            var landscapeText = m_Landscape.GetComponentInChildren<Text>();
            landscapeText.text = m_Presets[0].Name;

            m_RandomSeed.text = new System.Random().Next().ToString();
        }

        /// <summary>
        /// This method is called when the "back" button is clicked.
        /// </summary>
        public void Back()
        {
            m_MainMenu.SetActive(true);
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Changes the preset choice for the button to the next preset or to the first preset.
        /// </summary>
        public void SwitchPreset()
        {
            m_PresetIndex++;
            if (m_PresetIndex == m_Presets.Length)
                m_PresetIndex = 0;
            
            var landscapeText = m_Landscape.GetComponentInChildren<Text>();
            landscapeText.text = m_Presets[m_PresetIndex].Name;
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void StartGame()
        {
            int.TryParse(m_RandomSeed.text, out var randomSeed);

            var random = new System.Random(randomSeed);
            var generatorParams = m_Presets[m_PresetIndex].GeneratorParams;
            generatorParams.randomSeed = randomSeed;
            generatorParams.Terrain.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));
            generatorParams.Water.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));
            generatorParams.Stone.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));
            generatorParams.Tree.Noise.seed = new Vector2(random.Next(-65535, 65535), random.Next(-65535, 65535));

            m_ChunkManager.StartNew(generatorParams);
            gameObject.SetActive(false);
        }
    }
}