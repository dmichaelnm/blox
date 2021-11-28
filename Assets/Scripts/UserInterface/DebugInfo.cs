using Blox.CommonNS;
using Blox.GameNS;
using Blox.PlayerNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UserInterfaceNS
{
    public class DebugInfo : MonoBehaviour
    {
        public float refreshInterval;

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private Text m_GlobalPositionValue;
        [SerializeField] private Text m_ChunkPositionValue;
        [SerializeField] private Text m_LocalPositionValue;
        [SerializeField] private Text m_MemoryUsageValue;
        [SerializeField] private Text m_FrameTimeValue;
        [SerializeField] private Text m_SelectionValue;

        private bool m_ShowDebugInfo;
        private Behaviour[] m_Components;
        private float m_RefreshTimer;
        private int m_FrameCount;

        private void Awake()
        {
            m_Components = GetComponentsInChildren<Behaviour>();
            ShowOrHideComponents();

            m_GameManager.onPlayerPosition += OnPlayerPosition;
            m_GameManager.onBlockSelected += OnBlockSelected;
        }

        private void Update()
        {
            m_RefreshTimer += Time.deltaTime;
            m_FrameCount++;
            if (m_RefreshTimer >= refreshInterval)
            {
                var memoryUsage = System.GC.GetTotalMemory(true) / 1024 / 1024;
                m_MemoryUsageValue.text = $"{memoryUsage:F1} MB";
                var frameTime = m_RefreshTimer / m_FrameCount;
                var fps = 1f / frameTime;
                var frameTimeMS = frameTime * 1000f;
                m_FrameTimeValue.text = $"{frameTimeMS:F1}ms / {fps:F0} fps";
                m_RefreshTimer = 0f;
                m_FrameCount = 0;
            }

            if (Input.GetKeyDown(m_GameManager.showDebugInfo))
            {
                m_ShowDebugInfo = !m_ShowDebugInfo;
                ShowOrHideComponents();
                Log.Debug(this, $"Toggle debug info window to [{m_ShowDebugInfo}].");
            }
        }

        private void OnPlayerPosition(PlayerControl component, PlayerPosition eventargs)
        {
            if (m_ShowDebugInfo)
            {
                var gp = eventargs.currentGlobalBlockPosition;
                m_GlobalPositionValue.text = $"{gp.x} : {gp.y} : {gp.z}";
                var cp = eventargs.currentChunkPosition;
                m_ChunkPositionValue.text = $"{cp.x} : {cp.z}";
                var lp = eventargs.currentLocalBlockPosition;
                m_LocalPositionValue.text = $"{lp.x} : {lp.y} : {lp.z}";
            }
        }

        private void OnBlockSelected(PlayerSelection component, PlayerSelection.SelectionState eventargs)
        {
            m_SelectionValue.text = $"{eventargs.blockType.name} ({eventargs.face})";
        }

        private void ShowOrHideComponents()
        {
            foreach (var component in m_Components)
            {
                if (component != this)
                    component.enabled = m_ShowDebugInfo;
            }
        }
    }
}