using Blox.GameNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS.MainMenuNS
{
    public class NewGame : FadingBehaviour
    {
        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private FadingBehaviour m_MainMenu;
        [SerializeField] private FadingBehaviour m_Title;
        [SerializeField] private InputField m_WorldName;
        [SerializeField] private InputField m_RandomSeed;
        [SerializeField] private InputField m_TerrainPreset;

        private string[] m_PresetNames;
        private int m_PresetIndex;

        protected override void _Awake()
        {
            var config = m_GameManager.configuration;
            m_PresetNames = config.GetTerrainPresetNames();
            m_TerrainPreset.text = m_PresetNames[0];
            m_RandomSeed.text = new System.Random().Next(int.MinValue, int.MaxValue).ToString();
        }

        public void OnPreviousPreset()
        {
            m_PresetIndex--;
            m_PresetIndex = m_PresetIndex < 0 ? m_PresetNames.Length - 1 : m_PresetIndex;
            m_TerrainPreset.text = m_PresetNames[m_PresetIndex];
        }

        public void OnNextPreset()
        {
            m_PresetIndex++;
            m_PresetIndex = m_PresetIndex >= m_PresetNames.Length ? 0 : m_PresetIndex;
            m_TerrainPreset.text = m_PresetNames[m_PresetIndex];
        }

        public void OnBackClick()
        {
            m_MainMenu.gameObject.SetActive(true);
            FadeOut(state =>
            {
                m_MainMenu.FadeIn();
                gameObject.SetActive(false);
            });
        }

        public void OnStartClick()
        {
            var newGameFader = GetComponent<FadingBehaviour>();
            newGameFader.FadeOut(state =>
            {
                var worldName = m_WorldName.text;
                var presetName = m_TerrainPreset.text;
                var randomSeed = int.Parse(m_RandomSeed.text);

                gameObject.SetActive(false);
                m_GameManager.StartNewGame(worldName, presetName, randomSeed);
            });
            m_Title.FadeOut();
        }
    }
}