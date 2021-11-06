using Blox.EnvironmentNS;
using Blox.UtilitiesNS;
using UnityEngine;

namespace Blox.PlayerNS
{
    /// <summary>
    /// This struct contains detailed information about the players position.
    /// </summary>
    public struct PlayerPosition
    {
        /// <summary>
        /// The size of a chunk.
        /// </summary>
        public readonly ChunkSize ChunkSize;

        /// <summary>
        /// The current global position of the player in the world.
        /// </summary>
        public Vector3 CurrentPosition;

        /// <summary>
        /// The last global position of the player in the world.
        /// </summary>
        public Vector3 LastPosition;

        /// <summary>
        /// Returns the current global block position of the player
        /// </summary>
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

        /// <summary>
        /// Returns the last global block position of the player
        /// </summary>
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

        /// <summary>
        /// Returns the current chunk position of the player.
        /// </summary>
        public ChunkPosition CurrentChunkPosition => ChunkPosition.FromGlobalPosition(ChunkSize, CurrentGlobalBlockPosition);

        /// <summary>
        /// Returns the last chunk position of the player.
        /// </summary>
        public ChunkPosition LastChunkPosition => ChunkPosition.FromGlobalPosition(ChunkSize, LastGlobalBlockPosition);

        /// <summary>
        /// Returns the current local position of the player.
        /// </summary>
        public Vector3 CurrentLocalPosition
        {
            get
            {
                var x = CurrentPosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = CurrentPosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3(x, CurrentPosition.y, z);
            }
        }
        
        /// <summary>
        /// Returns the current local block position of the player.
        /// </summary>
        public Vector3Int CurrentLocalBlockPosition
        {
            get
            {
                var x = CurrentGlobalBlockPosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = CurrentGlobalBlockPosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3Int(x, CurrentGlobalBlockPosition.y, z);
            }
        }

        /// <summary>
        /// Returns the last local block position of the player
        /// </summary>
        public Vector3Int LastLocalBlockPosition
        {
            get
            {
                var x = LastGlobalBlockPosition.x - (ChunkSize.Width * LastChunkPosition.X);
                var z = LastGlobalBlockPosition.z - (ChunkSize.Width * LastChunkPosition.Z);
                return new Vector3Int(x, LastGlobalBlockPosition.y, z);
            }
        }

        /// <summary>
        /// A flag that indicates that the player has entered a new chunk or not.
        /// </summary>
        public bool HasChunkChanged => !CurrentChunkPosition.Equals(LastChunkPosition);

        /// <summary>
        /// The position of the camera eye.
        /// </summary>
        public Vector3 EyePosition
        {
            get
            {
                var cp = CurrentPosition;
                cp.y += m_Camera.localPosition.y;
                return cp;
            }    
        }
        
        /// <summary>
        /// Returns the local position of the camera eye.
        /// </summary>
        public Vector3 LocalEyePosition
        {
            get
            {
                var x = EyePosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = EyePosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3(x, EyePosition.y, z);
            }
        }

        /// <summary>
        /// The position of the players feet.
        /// </summary>
        public Vector3 FeetPosition
        {
            get
            {
                var cp = CurrentPosition;
                cp.y += m_GroundCheck.localPosition.y;
                return cp;
            }    
        }
        
        /// <summary>
        /// Returns the local position of the players feet.
        /// </summary>
        public Vector3 LocalFeetPosition
        {
            get
            {
                var x = FeetPosition.x - (ChunkSize.Width * CurrentChunkPosition.X);
                var z = FeetPosition.z - (ChunkSize.Width * CurrentChunkPosition.Z);
                return new Vector3(x, FeetPosition.y, z);
            }
        }

        /// <summary>
        /// The forward vector of the main camera.
        /// </summary>
        public Vector3 CameraForward => m_Camera.forward;
        
        /// <summary>
        /// The tranform object of the main camera.
        /// </summary>
        private readonly Transform m_Camera;

        /// <summary>
        /// The transform object of the ground check object.
        /// </summary>
        private readonly Transform m_GroundCheck;
        
        /// <summary>
        /// Constructor with a chunk size.
        /// </summary>
        /// <param name="chunkSize">The size of a chunk</param>
        /// <param name="camera">The tranform object of the main camera</param>
        /// <param name="groundCheck">The transform object of the ground check object</param>
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