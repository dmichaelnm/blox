using Blox.EnvironmentNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UINS
{
    /// <summary>
    /// This component controls the loading screen.
    /// </summary>
    public class LoadingScreen : FadeBehaviour
    {
        /// <summary>
        /// The chunk manager component.
        /// </summary>
        [SerializeField] private ChunkManager m_ChunkManager;

        /// <summary>
        /// The loading screen image.
        /// </summary>
        private Image m_LoadingScreenImage;
        
        /// <summary>
        /// This method is called when the component is created.
        /// </summary>
        private void Awake()
        {
            m_ChunkManager.OnChunkManagerInitialized += OnChunkManagerInitialized;
            m_ChunkManager.OnChunkManagerDestroyed += OnChunkManagerDestroyed;

            m_LoadingScreenImage = GetComponent<Image>();
            m_LoadingScreenImage.enabled = true;
        }

        /// <summary>
        /// This method is called when the chunk manager component is about to be destroyed.
        /// </summary>
        /// <param name="component">The chunk manager component</param>
        private void OnChunkManagerDestroyed(ChunkManager component)
        {
            m_ChunkManager.OnChunkManagerInitialized -= OnChunkManagerInitialized;
            m_ChunkManager.OnChunkManagerDestroyed -= OnChunkManagerDestroyed;
        }

        private void OnChunkManagerInitialized(ChunkManager component)
        {
            FadeOut();
        }

        protected override void StartFade(State state)
        {
            m_LoadingScreenImage.enabled = state == State.FadeOut;
        }

        protected override void Fading(State state, float value)
        {
            m_LoadingScreenImage.color = new Color(1f, 1f, 1f, value);
        }

        protected override void EndFade(State state)
        {
            m_LoadingScreenImage.enabled = state == State.FadeIn;
        }
    }
}