using Blox.Environment.Config;
using Blox.Player;
using Common;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

namespace Blox.Environment.PostProcessing
{
    public class Underwater : MonoBehaviour
    {
        public static Underwater GetInstance()
        {
            return GameObject.Find("Underwater").GetComponent<Underwater>();
        }
        
        public float offset = 0.3f;

        public delegate void IsPlayerUnderwater(bool underwater);
        public delegate void IsPlayerHitsWater(bool hitted);
        public event IsPlayerUnderwater onIsPlayerUnderwater;
        public event IsPlayerHitsWater onIsPlayerHitsWater;
        
        public Vector2 distortionSpeed;
        [Range(0f, 0.5f)] public float distortionStrength = 0.25f;

        private PlayerController m_PlayerController;
        private ChunkManager m_ChunkManager;
        private bool m_IsUnderwater;
        private ColorAdjustments m_ColorAdjustments;
        private DepthOfField m_DepthOfField;
        private LensDistortion m_LensDistortion;
        private Vector2 m_DistortionValues;
        private Vector2 m_DistortionFactor;
        private Vector2 m_DistortionBounds;
        private Random m_Random;
        private bool m_WaterHitted;

        public void OnPlayerPositionChanged(Position position)
        {
            if (m_ChunkManager.initialized)
            {
                var chunkData = m_ChunkManager[position.chunk];
                var belowBlockType = chunkData[position[BlockFace.Bottom].local];
                Debug.Log(belowBlockType);
                if (belowBlockType.isFluid && !m_WaterHitted)
                {
                    m_WaterHitted = true;
                    onIsPlayerHitsWater?.Invoke(m_WaterHitted);
                } 
                else if (belowBlockType.isEmpty && m_WaterHitted)
                {
                    m_WaterHitted = false;
                    onIsPlayerHitsWater?.Invoke(m_WaterHitted);
                }
                
                var blockType = chunkData[position.local];
                if (blockType != null)
                {
                    var delta = position.raw.y - position.local.y;
                    if (blockType.isFluid && delta < offset && !m_IsUnderwater)
                    {
                        m_IsUnderwater = true;
                        onIsPlayerUnderwater?.Invoke(m_IsUnderwater);
                        EnableUnderwaterPostProcessing();
                    }
                    else if (chunkData[position.local, BlockFace.Top].id != 3 && delta > offset && m_IsUnderwater)
                    {
                        m_IsUnderwater = false;
                        onIsPlayerUnderwater?.Invoke(m_IsUnderwater);
                        DisableUnderwaterPostProcessing();
                    }
                }
            }
        }

        private void Awake()
        {
            var chm = GameObject.Find("Chunk Manager");
            m_ChunkManager = chm.GetComponent<ChunkManager>();

            var player = GameObject.Find("Player");
            m_PlayerController = player.GetComponent<PlayerController>();
            m_PlayerController.onPlayerPositionChanged += OnPlayerPositionChanged;

            var volume = GetComponent<Volume>();
            volume.profile.TryGet(out m_ColorAdjustments);
            volume.profile.TryGet(out m_DepthOfField);
            volume.profile.TryGet(out m_LensDistortion);
            m_Random = new Random();
            m_DistortionFactor = new Vector2(1f, -1f);
            m_DistortionValues = new Vector2(0.5f, 0.5f);
            m_DistortionBounds = new Vector2(
                0.5f + distortionStrength + (float)m_Random.NextDouble() * (0.5f - distortionStrength),
                0.5f - distortionStrength - (float)m_Random.NextDouble() * (0.5f - distortionStrength));
        }

        private void OnDestroy()
        {
            m_PlayerController.onPlayerPositionChanged -= OnPlayerPositionChanged;
        }

        private void Update()
        {
            if (m_IsUnderwater)
            {
                m_DistortionValues.x += distortionSpeed.x * Time.deltaTime * m_DistortionFactor.x;
                m_DistortionValues.y += distortionSpeed.y * Time.deltaTime * m_DistortionFactor.y;

                if (m_DistortionFactor.x > 0f && m_DistortionValues.x > m_DistortionBounds.x)
                {
                    m_DistortionValues.x = m_DistortionBounds.x;
                    m_DistortionFactor.x = -1f;
                    m_DistortionBounds.x = 0.5f - distortionStrength -
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                if (m_DistortionFactor.x < 0f && m_DistortionValues.x < m_DistortionBounds.x)
                {
                    m_DistortionValues.x = m_DistortionBounds.x;
                    m_DistortionFactor.x = 1f;
                    m_DistortionBounds.x = 0.5f + distortionStrength +
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                if (m_DistortionFactor.y > 0f && m_DistortionValues.y > m_DistortionBounds.y)
                {
                    m_DistortionValues.y = m_DistortionBounds.y;
                    m_DistortionFactor.y = -1f;
                    m_DistortionBounds.y = 0.5f - distortionStrength -
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                if (m_DistortionFactor.y < 0f && m_DistortionValues.y < m_DistortionBounds.y)
                {
                    m_DistortionValues.y = m_DistortionBounds.y;
                    m_DistortionFactor.y = 1f;
                    m_DistortionBounds.y = 0.5f + distortionStrength +
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                m_LensDistortion.xMultiplier.value = m_DistortionValues.x;
                m_LensDistortion.yMultiplier.value = m_DistortionValues.y;
            }
        }

        private void EnableUnderwaterPostProcessing()
        {
            m_ColorAdjustments.colorFilter.overrideState = true;
            m_DepthOfField.mode.value = DepthOfFieldMode.Bokeh;
            m_LensDistortion.intensity.overrideState = true;
        }

        private void DisableUnderwaterPostProcessing()
        {
            m_ColorAdjustments.colorFilter.overrideState = false;
            m_DepthOfField.mode.value = DepthOfFieldMode.Off;
        }
    }
}