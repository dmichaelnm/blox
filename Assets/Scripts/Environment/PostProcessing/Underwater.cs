using Blox.GameNS;
using Blox.PlayerNS;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Random = System.Random;

namespace Blox.EnvironmentNS.PostProcessing
{
    /// <summary>
    /// This component controls the post processing for the underwater effect.
    /// </summary>
    public class Underwater : MonoBehaviour
    {
        /// <summary>
        /// The distortion speed vector.
        /// </summary>
        public Vector2 distortionSpeed;
        
        /// <summary>
        /// The distortion strength.
        /// </summary>
        [Range(0f, 0.5f)] public float distortionStrength = 0.25f;

        /// <summary>
        /// When true, plays a splash sound when the player submerges.
        /// </summary>
        public bool PlayOnSubmerge = true;
        
        /// <summary>
        /// When true, plays a splash sound when the player emerges.
        /// </summary>
        public bool PlayOnEmerge;

        /// <summary>
        /// This event is triggered when the player submerges or emerges.
        /// </summary>
        public event Events.BooleanEvent OnPlayerUnderwater;

        /// <summary>
        /// This event is triggered when the player get wet feets.
        /// </summary>
        public event Events.BooleanEvent OnPlayerWetFeets;
        
        /// <summary>
        /// This event is triggered when this component is about to be destroyed.
        /// </summary>
        public event Events.ComponentEvent<Underwater> OnUnderwaterDestroyed;
        
        /// <summary>
        /// The chunk manager.
        /// </summary>
        [SerializeField] private ChunkManager m_ChunkManager;

        /// <summary>
        /// The player controller.
        /// </summary>
        [SerializeField] private PlayerController m_PlayerController;

        /// <summary>
        /// The volume component
        /// </summary>
        [SerializeField] private Volume m_Volume;

        /// <summary>
        /// The splash audio source
        /// </summary>
        [SerializeField] private AudioSource m_SplashAudio;
        
        /// <summary>
        /// The color adjustments post processing effect.
        /// </summary>
        private ColorAdjustments m_ColorAdjustments;

        /// <summary>
        /// The depth of field post processing effect.
        /// </summary>
        private DepthOfField m_DepthOfField;

        /// <summary>
        /// The lens distortion post processing effect.
        /// </summary>
        private LensDistortion m_LensDistortion;

        /// <summary>
        /// A vector containing the current distortion values.
        /// </summary>
        private Vector2 m_DistortionValues;

        /// <summary>
        /// A vector containing the direction of the distorion.
        /// </summary>
        private Vector2 m_DistortionDirection;
        
        /// <summary>
        /// A vector containing the bounds of the current distorion.
        /// </summary>
        private Vector2 m_DistortionBounds;

        /// <summary>
        /// A random number generator.
        /// </summary>
        private Random m_Random;

        /// <summary>
        /// A flag that indicates if the player has already wet feets or not.
        /// </summary>
        private bool m_WetFeets;
        
        /// <summary>
        /// This method is called when this component is created.
        /// </summary>
        private void Awake()
        {
            m_PlayerController.OnPlayerMoved += OnPlayerMoved;
            m_PlayerController.OnPlayerControllerDestroyed += OnPlayerControllerDestroyed;

            m_Volume.profile.TryGet(out m_ColorAdjustments);
            m_Volume.profile.TryGet(out m_DepthOfField);
            m_Volume.profile.TryGet(out m_LensDistortion);

            m_Random = new Random();
            m_DistortionDirection = new Vector2(1f, -1f);
            m_DistortionValues = new Vector2(0.5f, 0.5f);
            m_DistortionBounds = new Vector2(
                0.5f + distortionStrength + (float)m_Random.NextDouble() * (0.5f - distortionStrength),
                0.5f - distortionStrength - (float)m_Random.NextDouble() * (0.5f - distortionStrength));
        }

        /// <summary>
        /// This method is called before this component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnUnderwaterDestroyed?.Invoke(this);
        }

        /// <summary>
        /// This method is called every frame
        /// </summary>
        private void Update()
        {
            if (m_LensDistortion.intensity.overrideState)
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
            if (m_ChunkManager.Initialized)
            {
                // Checks if the underwater post processing should be activated
                var blockType = m_ChunkManager[position.CurrentChunkPosition][position.LocalEyePosition];
                if (blockType.IsFluid && !m_ColorAdjustments.colorFilter.overrideState)
                {
                    m_ColorAdjustments.colorFilter.overrideState = true;
                    m_DepthOfField.mode.value = DepthOfFieldMode.Bokeh;
                    m_LensDistortion.intensity.overrideState = true;
                    OnPlayerUnderwater?.Invoke(true);
                } else if (blockType.IsEmpty && m_ColorAdjustments.colorFilter.overrideState)
                {
                    m_ColorAdjustments.colorFilter.overrideState = false;
                    m_DepthOfField.mode.value = DepthOfFieldMode.Off;
                    m_LensDistortion.intensity.overrideState = false;
                    OnPlayerUnderwater?.Invoke(false);
                }

                // Checks if a water splash sound should be played or not
                blockType = m_ChunkManager[position.CurrentChunkPosition][position.LocalFeetPosition];
                if (blockType.IsFluid && !m_WetFeets)
                {
                    if (PlayOnSubmerge)
                        m_SplashAudio.Play();
                    m_WetFeets = true;
                    OnPlayerWetFeets?.Invoke(true);
                } else if (blockType.IsEmpty && m_WetFeets)
                {
                    if (PlayOnEmerge)
                        m_SplashAudio.Play();
                    m_WetFeets = false;
                    OnPlayerWetFeets?.Invoke(false);
                }
            }
        }
    }
}