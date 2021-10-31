using System;
using System.Collections.Generic;
using Blox.Environment;
using Blox.Environment.Config;
using Blox.Environment.PostProcessing;
using Blox.Player;
using Blox.Utility;
using Common;
using UnityEngine;
using UnityEngine.UI;

namespace Blox2.UI
{
    public class GameInfo : MonoBehaviour
    {
        private static long MB = 1024 * 1024;

        public KeyCode activationShortcut = KeyCode.F1;
        public float refreshInterval = 1f;

        private bool m_GameInfoActive;
        private PlayerController m_PlayerController;
        private PlayerSelection m_PlayerSelection;
        private ChunkManager m_ChunkManager;
        private Text m_RawPosValue;
        private Text m_GlobalPosValue;
        private Text m_LocalPosValue;
        private Text m_ChunkPosValue;
        private Text m_MillisPerFrameValue;
        private Text m_MemoryUsage;
        private Underwater m_Underwater;
        private Text m_UnderwaterValue;
        private Text m_BlockSelectionValue;
        private float m_Time;
        private List<MonoBehaviour> m_Items;

        public void OnPlayerPositionChanged(Position position)
        {
            if (m_GameInfoActive)
            {
                m_RawPosValue.text = "X = " + Mathf.Round(position.raw.x * 100f) / 100f + " , Y = " +
                                     Mathf.Round(position.raw.y * 100f) / 100f +
                                     " , Z = " + Mathf.Round(position.raw.z * 100f) / 100f;
                m_GlobalPosValue.text = "X = " + position.global.x + " , Y = " + position.global.y +
                                        " , Z = " + position.global.z;
                m_LocalPosValue.text = "X = " + position.local.x + " , Y = " + position.local.y +
                                       " , Z = " + position.local.z;
                m_ChunkPosValue.text = "X = " + position.chunk.x + " , Z = " + position.chunk.z;
            }
        }

        public void OnPlayerIsUnderwater(bool underwater)
        {
            m_UnderwaterValue.text = underwater ? "yes" : "no";
        }
        
        private void Awake()
        {
            m_Items = new List<MonoBehaviour>();

            m_ChunkManager = GameObject.Find("Chunk Manager").GetComponent<ChunkManager>();
            
            var player = GameObject.Find("Player");
            m_PlayerController = player.GetComponent<PlayerController>();
            m_PlayerController.onPlayerPositionChanged += OnPlayerPositionChanged;

            var selection = GameObject.Find("Selection Block");
            m_PlayerSelection = selection.GetComponent<PlayerSelection>();
            m_PlayerSelection.onBlockSelection += OnBlockSelection;

            var underwater = GameObject.Find("Underwater");
            m_Underwater = underwater.GetComponent<Underwater>();
            m_Underwater.onIsPlayerUnderwater += OnPlayerIsUnderwater;

            m_RawPosValue = gameObject.GetChildObject("RawPosValue").GetComponent<Text>();
            m_GlobalPosValue = gameObject.GetChildObject("GlobalPosValue").GetComponent<Text>();
            m_LocalPosValue = gameObject.GetChildObject("LocalPosValue").GetComponent<Text>();
            m_ChunkPosValue = gameObject.GetChildObject("ChunkPosValue").GetComponent<Text>();
            m_MillisPerFrameValue = gameObject.GetChildObject("MillisPerFrameValue").GetComponent<Text>();
            m_MemoryUsage = gameObject.GetChildObject("MemoryUsageValue").GetComponent<Text>();
            m_UnderwaterValue = gameObject.GetChildObject("UnderwaterValue").GetComponent<Text>();
            m_BlockSelectionValue = gameObject.GetChildObject("BlockSelectionValue").GetComponent<Text>();
            
            m_Items.Add(gameObject.GetChildObject("RawPosLabel").GetComponent<Text>());
            m_Items.Add(m_RawPosValue);
            m_Items.Add(gameObject.GetChildObject("GlobalPosLabel").GetComponent<Text>());
            m_Items.Add(m_GlobalPosValue);
            m_Items.Add(gameObject.GetChildObject("LocalPosLabel").GetComponent<Text>());
            m_Items.Add(m_LocalPosValue);
            m_Items.Add(gameObject.GetChildObject("ChunkPosLabel").GetComponent<Text>());
            m_Items.Add(m_ChunkPosValue);
            m_Items.Add(gameObject.GetChildObject("MillisPerFrameLabel").GetComponent<Text>());
            m_Items.Add(m_MillisPerFrameValue);
            m_Items.Add(gameObject.GetChildObject("MemoryUsageLabel").GetComponent<Text>());
            m_Items.Add(m_MemoryUsage);
            m_Items.Add(gameObject.GetChildObject("UnderwaterLabel").GetComponent<Text>());
            m_Items.Add(m_UnderwaterValue);
            m_Items.Add(gameObject.GetChildObject("BlockSelectionLabel").GetComponent<Text>());
            m_Items.Add(m_BlockSelectionValue);
            m_Items.Add(GetComponent<Image>());
            
            ShowGameInfo(false);
        }

        private void OnBlockSelection(Position position, BlockFace face,
            PlayerSelection.MouseButtonState mouseButtonState)
        {
            if (m_GameInfoActive)
            {
                var chunkData = m_ChunkManager[position.chunk];
                var blockType = chunkData[position.local];
                m_BlockSelectionValue.text = blockType.name + " (" + face + ")";
            }
        }

        private void OnDestroy()
        {
            m_PlayerController.onPlayerPositionChanged -= OnPlayerPositionChanged;
            m_Underwater.onIsPlayerUnderwater -= OnPlayerIsUnderwater;
        }

        private void Update()
        {
            if (Input.GetKeyUp(activationShortcut))
            {
                m_GameInfoActive = !m_GameInfoActive;
                ShowGameInfo(m_GameInfoActive);
            }

            m_Time += Time.deltaTime;
            if (m_Time >= refreshInterval)
            {
                var fps = Mathf.FloorToInt(1f / Time.deltaTime);
                m_MillisPerFrameValue.text = Mathf.RoundToInt(Time.deltaTime * 1000f) + " ms ( " + fps + " fps )";
                var usg = GC.GetTotalMemory(false) / MB;
                m_MemoryUsage.text = usg + " MB";
                m_Time = 0f;
            }
        }

        private void ShowGameInfo(bool show)
        {
            //enabled = show;
            foreach (var item in m_Items)
                item.enabled = show;
        }
    }
}