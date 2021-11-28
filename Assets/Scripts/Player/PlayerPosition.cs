using Blox.CommonNS;
using Blox.TerrainNS;
using UnityEngine;

namespace Blox.PlayerNS
{
    public readonly struct PlayerPosition
    {
        public readonly Vector3 currentPosition;

        public readonly Vector3 cameraForward;

        public Vector3Int currentGlobalBlockPosition
        {
            get
            {
                var x = Math.FloorToInt(currentPosition.x);
                var y = Math.FloorToInt(currentPosition.y);
                var z = Math.FloorToInt(currentPosition.z);
                return new Vector3Int(x, y, z);
            }
        }

        public ChunkPosition currentChunkPosition
        {
            get
            {
                var cbp = currentGlobalBlockPosition;
                return ChunkPosition.From(m_ChunkSize, cbp.x, cbp.z);
            }
        }

        public Vector3Int currentLocalBlockPosition
        {
            get
            {
                var cp = currentChunkPosition;
                return cp.ToLocalPosition(m_ChunkSize, currentGlobalBlockPosition);
            }
        }

        public Vector3 currentEyePosition => currentPosition + m_LocalEyePosition;

        public Vector3 currentFeetPosition => currentPosition + m_LocalFootPosition;
        
        private readonly ChunkSize m_ChunkSize;
        private readonly Vector3 m_LocalEyePosition;
        private readonly Vector3 m_LocalFootPosition;

        public PlayerPosition(Vector3 currentPosition, ChunkSize chunkSize, Transform cameraTransform,
            Transform groundCheck)
        {
            m_ChunkSize = chunkSize;
            m_LocalEyePosition = cameraTransform.localPosition;
            m_LocalFootPosition = groundCheck.localPosition;

            this.currentPosition = currentPosition;
            cameraForward = cameraTransform.forward;
        }
    }
}