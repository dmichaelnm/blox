using Blox.EnvironmentNS;
using Blox.UtilitiesNS;
using UnityEngine;

namespace Blox.PlayerNS
{
    public struct PlayerPosition
    {
        public readonly ChunkSize ChunkSize;
        public Vector3 CurrentPosition;
        public Vector3 LastPosition;

        public Vector3Int CurrentGlobalBlockPosition
        {
            get
            {
                var x = MathUtilities.Floor(CurrentPosition.x);
                var y = MathUtilities.Floor(CurrentPosition.y);
                var z = MathUtilities.Floor(CurrentPosition.z);
                return new Vector3Int(x, y, z);
            }
        }

        public Vector3Int LastGlobalBlockPosition
        {
            get
            {
                var x = MathUtilities.Floor(LastPosition.x);
                var y = MathUtilities.Floor(LastPosition.y);
                var z = MathUtilities.Floor(LastPosition.z);
                return new Vector3Int(x, y, z);
            }
        }

        public ChunkPosition CurrentChunkPosition => ChunkPosition.FromGlobalPosition(ChunkSize, CurrentGlobalBlockPosition);
        public ChunkPosition LastChunkPosition => ChunkPosition.FromGlobalPosition(ChunkSize, LastGlobalBlockPosition);

        public Vector3 CurrentLocalPosition
        {
            get
            {
                var x = CurrentPosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = CurrentPosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3(x, CurrentPosition.y, z);
            }
        }
        
        public Vector3Int CurrentLocalBlockPosition
        {
            get
            {
                var x = CurrentGlobalBlockPosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = CurrentGlobalBlockPosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3Int(x, CurrentGlobalBlockPosition.y, z);
            }
        }

        public Vector3Int LastLocalBlockPosition
        {
            get
            {
                var x = LastGlobalBlockPosition.x - (ChunkSize.Width * LastChunkPosition.X);
                var z = LastGlobalBlockPosition.z - (ChunkSize.Width * LastChunkPosition.Z);
                return new Vector3Int(x, LastGlobalBlockPosition.y, z);
            }
        }

        public bool HasChunkChanged => !CurrentChunkPosition.Equals(LastChunkPosition);

        public Vector3 EyePosition
        {
            get
            {
                var cp = CurrentPosition;
                cp.y += m_Camera.localPosition.y;
                return cp;
            }    
        }
        
        public Vector3 LocalEyePosition
        {
            get
            {
                var x = EyePosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = EyePosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3(x, EyePosition.y, z);
            }
        }

        public Vector3 FeetPosition
        {
            get
            {
                var cp = CurrentPosition;
                cp.y += m_GroundCheck.localPosition.y;
                return cp;
            }    
        }
        
        public Vector3 LocalFeetPosition
        {
            get
            {
                var x = FeetPosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = FeetPosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3(x, FeetPosition.y, z);
            }
        }

        public Vector3 CameraForward => m_Camera.forward;
        
        private readonly Transform m_Camera;

        private readonly Transform m_GroundCheck;
        
        public PlayerPosition(ChunkSize chunkSize, Transform camera, Transform groundCheck)
        {
            ChunkSize = chunkSize;
            CurrentPosition = Vector3.zero;
            LastPosition = Vector3.zero;
            m_Camera = camera;
            m_GroundCheck = groundCheck;
        }
    }
}