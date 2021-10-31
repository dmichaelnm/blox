using Blox.Environment;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.UI
{
    public class DisableStartScreen : MonoBehaviour
    {
        private Image m_StartImage;
        
        private void OnChunkManagerInitialized()
        {
            m_StartImage.enabled = false;
        }
        
        private void Awake()
        {
            var chunkManager = GameObject.Find("Chunk Manager").GetComponent<ChunkManager>();
            chunkManager.onInitialized += OnChunkManagerInitialized;

            m_StartImage = GetComponent<Image>();
        }
    }
}