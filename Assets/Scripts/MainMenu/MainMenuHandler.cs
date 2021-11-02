using System;
using System.Collections.Generic;
using System.Linq;
using Blox.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Blox.MainMenu
{
    public class MainMenuHandler : MonoBehaviour
    {
        [SerializeField] private GameObject m_MainMenu;
        [SerializeField] private GameObject m_NewGame;

        private Dropdown m_PresetSelection;
        private InputField m_RandomSeed;
        
        private void Awake()
        {
            Cursor.lockState = CursorLockMode.None;
            
            m_MainMenu.SetActive(true);
            m_NewGame.SetActive(false);
        }

        public void NewGame()
        {
            var presets = PresetManager.GetInstance().presets;
            m_MainMenu.SetActive(false);
            
            m_NewGame.SetActive(true);
            m_PresetSelection = m_NewGame.GetComponentInChildren<Dropdown>();
            m_PresetSelection.ClearOptions();
            var presetNames = new List<string>();
            presets.Iterate(preset => presetNames.Add(preset.name)); 
            m_PresetSelection.AddOptions(presetNames);

            m_RandomSeed = m_NewGame.GetComponentInChildren<InputField>();
            m_RandomSeed.text = Random.Range(int.MinValue, int.MaxValue).ToString();
            m_RandomSeed.characterValidation = InputField.CharacterValidation.Integer;
        }

        public void StartGame()
        {
            var presetManager = PresetManager.GetInstance();
            var presetName = m_PresetSelection.captionText.text;
            if (int.TryParse(m_RandomSeed.text, out var randomSeed))
            {
                presetManager.SelectPreset(presetName, randomSeed);
                SceneManager.LoadScene("GameScene");
            }
        }
        
        public void Quit()
        {
            Application.Quit();
        }
    }
}