using System;
using Blox.ConfigurationNS;
using Blox.EnvironmentNS.PostProcessing;
using Blox.PlayerNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component shows detailed game information on the canvas.
    /// </summary>
    public class GameInfo : MonoBehaviour
    {
        /// <summary>
        /// The shortcut key that shows or hides the game information panel.
        /// </summary>
        public KeyCode Shortcut = KeyCode.F1;

        /// <summary>
        /// Refresh interval of some informations in seconds.
        /// </summary>
        public float RefreshInterval = 1f;

        /// <summary>
        /// The player controller component.
        /// </summary>
        [SerializeField] private PlayerController m_PlayerController;

        /// <summary>
        /// The player selection component.
        /// </summary>
        [SerializeField] private PlayerSelection m_PlayerSelection;

        /// <summary>
        /// The underwater component.
        /// </summary>
        [SerializeField] private Underwater m_Underwater;

        /// <summary>
        /// An array with all text components of the game information panel.
        /// </summary>
        private Text[] m_Components;

        /// <summary>
        /// A flag with the shown or hide state of the game information panel.
        /// </summary>
        private bool m_Enabled;

        /// <summary>
        /// Timestamp of the last frame started.
        /// </summary>
        private float m_LastFrameTime;

        /// <summary>
        /// Timer until the next refresh.
        /// </summary>
        private float m_RefreshTimer;

        /// <summary>
        /// This method is called when the object is created. 
        /// </summary>
        private void Awake()
        {
            m_Components = GetComponentsInChildren<Text>();
            m_Enabled = false;
            SetEnabled(m_Enabled);

            m_PlayerController.OnPlayerMoved += OnPlayerMoved;
            m_PlayerController.OnPlayerControllerDestroyed += OnPlayerControllerDestroyed;

            m_PlayerSelection.OnBlockSelected += OnBlockSelected;
            m_PlayerSelection.OnNothingSelected += OnNothingSelected;
            m_PlayerSelection.OnPlayerSelectionDestroyed += OnPlayerSelectionDestroyed;

            m_Underwater.OnPlayerUnderwater += OnPlayerUnderwater;
            m_Underwater.OnUnderwaterDestroyed += OnUnderwaterDestroyed;
        }

        /// <summary>
        /// This method is called when nothing is selected.
        /// </summary>
        private void OnNothingSelected()
        {
            m_Components[15].text = "-";
        }

        /// <summary>
        /// This method is called when the player selection component is about to be destroyed.
        /// </summary>
        /// <param name="component">The player selection component</param>
        private void OnPlayerSelectionDestroyed(PlayerSelection component)
        {
            m_PlayerSelection.OnBlockSelected -= OnBlockSelected;
            m_PlayerSelection.OnNothingSelected -= OnNothingSelected;
            m_PlayerSelection.OnPlayerSelectionDestroyed -= OnPlayerSelectionDestroyed;
        }

        /// <summary>
        /// This method is called when a block is selected.
        /// </summary>
        /// <param name="position">The global block position</param>
        /// <param name="blockType">The type of the selected block</param>
        /// <param name="face">The block face</param>
        /// <param name="mousebuttonstate">The state of the mouse buttons</param>
        private void OnBlockSelected(Vector3Int position, BlockType blockType, BlockFace face,
            PlayerSelection.MouseButtonState mousebuttonstate)
        {
            m_Components[15].text = $"{blockType.Name} ({face})";
        }

        /// <summary>
        /// This method is called when the underwater component is about to be destroyed.
        /// </summary>
        /// <param name="underwater">The underwater component</param>
        private void OnUnderwaterDestroyed(Underwater underwater)
        {
            underwater.OnPlayerUnderwater -= OnPlayerUnderwater;
            underwater.OnUnderwaterDestroyed -= OnUnderwaterDestroyed;
        }

        /// <summary>
        /// This method is called when the player is submerged or emerged.
        /// </summary>
        /// <param name="underwater"></param>
        private void OnPlayerUnderwater(bool underwater)
        {
            m_Components[13].text = underwater ? "yes" : "no";
        }

        /// <summary>
        /// This method is called when the player controller is about to be destroyed.
        /// </summary>
        /// <param name="source"></param>
        private void OnPlayerControllerDestroyed(PlayerController source)
        {
            source.OnPlayerMoved -= OnPlayerMoved;
            source.OnPlayerControllerDestroyed -= OnPlayerControllerDestroyed;
        }

        /// <summary>
        /// This method receives events from the player controller about the player position.
        /// </summary>
        /// <param name="position">The player position</param>
        private void OnPlayerMoved(PlayerPosition position)
        {
            if (m_Enabled)
            {
                var rp = position.CurrentPosition;
                m_Components[1].text = $"X={rp.x:F2} Y={rp.y:F2} Z={rp.z:F2}";
                var gp = position.CurrentGlobalBlockPosition;
                m_Components[3].text = $"X={gp.x} Y={gp.y} Z={gp.z}";
                var lp = position.CurrentLocalBlockPosition;
                m_Components[5].text = $"X={lp.x} Y={lp.y} Z={lp.z}";
                var cp = position.CurrentChunkPosition;
                m_Components[7].text = $"X={cp.X} Z={cp.Z}";
            }
        }

        /// <summary>
        /// This method is called every frame.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyUp(Shortcut))
            {
                m_Enabled = !m_Enabled;
                SetEnabled(m_Enabled);
            }

            m_RefreshTimer += Time.deltaTime;
            if (m_Enabled && m_RefreshTimer > RefreshInterval)
            {
                var duration = (Time.realtimeSinceStartup - m_LastFrameTime) * 1000f;
                var fps = 1000f / duration;
                m_Components[9].text = $"{duration:F0} ms ({fps:F0} fps)";
                m_RefreshTimer = 0f;
                var memoryUsage = GC.GetTotalMemory(true) / 1024f / 1024f;
                m_Components[11].text = $"{memoryUsage:F0} MB";
            }

            m_LastFrameTime = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Shows or hides the game information panel.
        /// </summary>
        /// <param name="enabled">True to show the panel or false to hide the panel.</param>
        private void SetEnabled(bool enabled)
        {
            foreach (var component in m_Components)
                component.enabled = enabled;
        }
    }
}