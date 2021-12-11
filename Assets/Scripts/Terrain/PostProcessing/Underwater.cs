using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.GameNS;
using Blox.PlayerNS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

namespace Blox.TerrainNS.PostProcessingNS
{
    public class Underwater : MonoBehaviour
    {
        public Vector2 distortionSpeed;
        public float distortionStrength;

        public bool headUnderwater { get; private set; }
        public bool feetUnderwater { get; private set; }
        
        public event Events.ComponentBoolEvent<Underwater> onHeadUnderwater;
        public event Events.ComponentBoolEvent<Underwater> onFeetUnderwater; 

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private Volume m_Volume;
        [SerializeField] private AudioSource m_WaterSplash;

        private ColorAdjustments m_ColorAdjustments;
        private DepthOfField m_DepthOfField;
        private LensDistortion m_LensDistortion;
        private Random m_Random;
        private Vector2 m_DistortionValues;
        private Vector2 m_DistortionDirection;
        private Vector2 m_DistortionBounds;

        private void Awake()
        {
            m_GameManager.onPlayerPosition += OnPlayerPosition;

            m_Volume.profile.TryGet(out m_ColorAdjustments);
            m_Volume.profile.TryGet(out m_DepthOfField);
            m_Volume.profile.TryGet(out m_LensDistortion);

            m_Random = new Random(m_GameManager.randomSeed);
            m_DistortionDirection = new Vector2(1f, -1f);
            m_DistortionValues = new Vector2(0.5f, 0.5f);
            m_DistortionBounds = new Vector2(
                0.5f + distortionStrength + (float)m_Random.NextDouble() * (0.5f - distortionStrength),
                0.5f - distortionStrength - (float)m_Random.NextDouble() * (0.5f - distortionStrength));
        }

        private void Update()
        {
            if (headUnderwater)
            {
                m_DistortionValues.x += distortionSpeed.x * Time.deltaTime * m_DistortionDirection.x;
                m_DistortionValues.y += distortionSpeed.y * Time.deltaTime * m_DistortionDirection.y;

                if (m_DistortionDirection.x > 0f && m_DistortionValues.x > m_DistortionBounds.x)
                {
                    m_DistortionValues.x = m_DistortionBounds.x;
                    m_DistortionDirection.x = -1f;
                    m_DistortionBounds.x = 0.5f - distortionStrength -
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                if (m_DistortionDirection.x < 0f && m_DistortionValues.x < m_DistortionBounds.x)
                {
                    m_DistortionValues.x = m_DistortionBounds.x;
                    m_DistortionDirection.x = 1f;
                    m_DistortionBounds.x = 0.5f + distortionStrength +
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                if (m_DistortionDirection.y > 0f && m_DistortionValues.y > m_DistortionBounds.y)
                {
                    m_DistortionValues.y = m_DistortionBounds.y;
                    m_DistortionDirection.y = -1f;
                    m_DistortionBounds.y = 0.5f - distortionStrength -
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                if (m_DistortionDirection.y < 0f && m_DistortionValues.y < m_DistortionBounds.y)
                {
                    m_DistortionValues.y = m_DistortionBounds.y;
                    m_DistortionDirection.y = 1f;
                    m_DistortionBounds.y = 0.5f + distortionStrength +
                                           (float)m_Random.NextDouble() * (0.5f - distortionStrength);
                }

                m_LensDistortion.xMultiplier.value = m_DistortionValues.x;
                m_LensDistortion.yMultiplier.value = m_DistortionValues.y;
            }
        }

        private void OnPlayerPosition(PlayerControl component, PlayerPosition eventargs)
        {
            var chunkManager = m_GameManager.chunkManager;

            // check if feet are underwater
            var feetPosition = eventargs.currentFeetPosition.ToVector3Int();
            var feetBlockType = chunkManager.GetEntity<BlockType>(feetPosition);

            if (feetBlockType != null)
            {
                if (feetBlockType.isFluid && !feetUnderwater)
                {
                    Log.Debug(this, "Players feets are underwater.");
                    feetUnderwater = true;
                    m_WaterSplash.Play();
                    onFeetUnderwater?.Invoke(this, true);
                }
                else if (feetBlockType.isEmpty && feetUnderwater)
                {
                    Log.Debug(this, "Players feets are dry.");
                    feetUnderwater = false;
                    onFeetUnderwater?.Invoke(this, false);
                }
            }

            // check if head is underwater
            var eyePosition = eventargs.currentEyePosition.ToVector3Int();
            var eyeBlockType = chunkManager.GetEntity<BlockType>(eyePosition);

            if (eyeBlockType != null)
            {
                if (eyeBlockType.isFluid && !headUnderwater)
                {
                    Log.Debug(this, "Players head is underwater.");
                    headUnderwater = true;
                    m_ColorAdjustments.colorFilter.overrideState = true;
                    m_DepthOfField.mode.overrideState = true;
                    m_LensDistortion.intensity.overrideState = true;
                    onHeadUnderwater?.Invoke(this, true);
                }
                else if (eyeBlockType.isEmpty && headUnderwater)
                {
                    Log.Debug(this, "Players head is breathing air.");
                    headUnderwater = false;
                    m_ColorAdjustments.colorFilter.overrideState = false;
                    m_DepthOfField.mode.overrideState = false;
                    m_LensDistortion.intensity.overrideState = false;
                    onHeadUnderwater?.Invoke(this, false);
                }
            }
        }
    }
}