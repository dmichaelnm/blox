using System;
using Blox.ConfigurationNS;
using Blox.UtilitiesNS;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blox.EnvironmentNS
{
    /// <summary>
    /// A container for storing the data of a chunk.
    /// </summary>
    public class ChunkData
    {
        /// <summary>
        /// Returns the size of the chunk as defined in the chunk manager.
        /// </summary>
        public ChunkSize ChunkSize => m_ChunkManager.ChunkSize;

        /// <summary>
        /// The position of the chunk that uses this data.
        /// </summary>
        public readonly ChunkPosition ChunkPosition;

        /// <summary>
        /// The chunk data container left to this one.
        /// </summary>
        public ChunkData Left => m_ChunkManager[ChunkPosition.Left];

        /// <summary>
        /// The chunk data container right to this one.
        /// </summary>
        public ChunkData Right => m_ChunkManager[ChunkPosition.Right];

        /// <summary>
        /// The chunk data container in front to this one.
        /// </summary>
        public ChunkData Front => m_ChunkManager[ChunkPosition.Front];

        /// <summary>
        /// The chunk data container back to this one.
        /// </summary>
        public ChunkData Back => m_ChunkManager[ChunkPosition.Back];

        /// <summary>
        /// The timestamp when the chunk data was last active. 
        /// </summary>
        public float LastActive { get; private set; }
        
        /// <summary>
        /// Returns the block type of a neighbour to the given local coordinates.
        /// </summary>
        /// <param name="x">The local X coordinate of the block</param>
        /// <param name="y">The local Y coordinate of the block</param>
        /// <param name="z">The local Z coordinate of the block</param>
        /// <param name="face">The face defined which neighbour block type is returned</param>
        public BlockType this[int x, int y, int z, BlockFace face]
        {
            get
            {
                return face switch
                {
                    BlockFace.Top => this[x, y + 1, z],
                    BlockFace.Bottom => this[x, y - 1, z],
                    BlockFace.Front => this[x, y, z - 1],
                    BlockFace.Back => this[x, y, z + 1],
                    BlockFace.Left => this[x - 1, y, z],
                    BlockFace.Right => this[x + 1, y, z],
                    _ => null
                };
            }
        }

        /// <summary>
        /// Returns the block type for the given local raw position.
        /// </summary>
        /// <param name="position">A local raw position</param>
        public BlockType this[Vector3 position]
        {
            get
            {
                var x = MathUtilities.Floor(position.x);
                var y = MathUtilities.Floor(position.y);
                var z = MathUtilities.Floor(position.z);
                return this[x, y, z];
            }
        }

        /// <summary>
        /// Returns the block type for the given local position vector.
        /// </summary>
        /// <param name="localPosition">A local position vector</param>
        public BlockType this[Vector3Int localPosition] => this[localPosition.x, localPosition.y, localPosition.z];
        
        /// <summary>
        /// Returns the block type for the given local coordinates.
        /// </summary>
        /// <param name="x">The local X coordinate of the block</param>
        /// <param name="y">The local Y coordinate of the block</param>
        /// <param name="z">The local Z coordinate of the block</param>
        public BlockType this[int x, int y, int z]
        {
            get
            {
                if (ChunkSize.IsValid(x, y, z))
                {
                    var config = Configuration.GetInstance();
                    return config.GetBlockType(m_Blocks[ChunkSize.ToIndex(x, y, z)]);
                }

                if (y < 0 || y >= ChunkSize.Height)
                    return null;

                if (x == -1)
                    return Left[ChunkSize.Width - 1, y, z];
                if (x == ChunkSize.Width)
                    return Right[0, y, z];
                if (z == -1)
                    return Front[x, y, ChunkSize.Width - 1];
                if (z == ChunkSize.Width)
                    return Back[x, y, 0];

                throw new Exception("Invalid application state.");
            }
        }

        /// <summary>
        /// Internal reference to the chunk manager
        /// </summary>
        private readonly ChunkManager m_ChunkManager;

        /// <summary>
        /// Internal array containg the block informations of this chunks data container.
        /// </summary>
        private readonly int[] m_Blocks;

        /// <summary>
        /// Creates a new chunk data instance for the given position and initializes it with the given block array.
        /// </summary>
        /// <param name="chunkPosition">The position of the chunk that uses this data</param>
        /// <param name="blocks">An array containing the block information for this new chunk data container</param>
        public ChunkData(ChunkPosition chunkPosition, [NotNull] int[] blocks)
        {
            m_ChunkManager = ChunkManager.GetInstance();

            // Check, if the block array has the proper size
            Assert.IsTrue(blocks.Length == ChunkSize.Size);

            ChunkPosition = chunkPosition;
            m_Blocks = blocks;
            LastActive = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Returns the height of the surface. The surface is the first solid block counted from the top of the chunk.
        /// </summary>
        /// <param name="x">Local X coordinate</param>
        /// <param name="z">Local Z coordinate</param>
        /// <param name="y">The resulting Y coordinate</param>
        /// <returns>True, if a solid block was found, otherwise false</returns>
        public bool GetSurfaceHeight(int x, int z, out int y)
        {
            var _y = ChunkSize.Height - 1;
            while (_y >= 0)
            {
                var blockType = this[x, _y, z];
                if (blockType.IsSolid)
                {
                    y = _y;
                    return true;
                }
                _y--;
            }

            y = default;
            return false;
        }
        
        /// <summary>
        /// Sets the block type of the block located at the given local coordinates.
        /// </summary>
        /// <param name="x">The local X coordinate</param>
        /// <param name="y">The local Y coordinate</param>
        /// <param name="z">The local Z coordinate</param>
        /// <param name="blockTypeId">The ID of the block type</param>
        public void SetBlock(int x, int y, int z, int blockTypeId)
        {
            Assert.IsTrue(ChunkSize.IsValid(x, y, z));
            m_Blocks[ChunkSize.ToIndex(x, y, z)] = blockTypeId;
        }

        /// <summary>
        /// Sets the block type of the block located at the given local position vector.
        /// </summary>
        /// <param name="localPosition">The local position vector</param>
        /// <param name="blockTypeId">The ID of the block type</param>
        public void SetBlock(Vector3Int localPosition, int blockTypeId)
        {
            SetBlock(localPosition.x, localPosition.y, localPosition.z, blockTypeId);    
        }
        
        /// <summary>
        /// Marks this chunk data container as active.
        /// </summary>
        public void MarkAsActive()
        {
            LastActive = Time.realtimeSinceStartup;
        }

        /// <summary>
        /// Returns the array with the block types.
        /// </summary>
        /// <returns>block type array</returns>
        public int[] ToArray()
        {
            return m_Blocks;
        }

        /// <summary>
        /// Compare this chunk data container with another object. If the other object is also a chunk data container
        /// and has the same chunk position, both objects are considered as equal.
        /// </summary>
        /// <param name="obj">Another object</param>
        /// <returns>True if both objects are equal, otherwise false</returns>
        public override bool Equals(object obj)
        {
            if (obj is ChunkData cd)
                return cd.ChunkPosition.Equals(ChunkPosition);

            return false;
        }

        /// <summary>
        /// Returns the chunk positions hash code.
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            return ChunkPosition.GetHashCode();
        }
    }
}