using System;
using UnityEngine;

namespace Blox.EnvironmentNS
{
    /// <summary>
    /// This struct defines the dimensions of a chunk.
    /// </summary>
    [Serializable]
    public struct ChunkSize
    {
        /// <summary>
        /// The width and length of a chunk.
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of a chunk.
        /// </summary>
        public int Height;

        /// <summary>
        /// The number of blocks contained by a chunk.
        /// </summary>
        public int Size => Width * Width * Height;

        /// <summary>
        /// Checks, if the local coordinates are valid for this chunk size.
        /// </summary>
        /// <param name="x">Local X coordinate</param>
        /// <param name="y">Local Y coordinate</param>
        /// <param name="z">Local Z coordinate</param>
        /// <returns></returns>
        public bool IsValid(int x, int y, int z)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height && z >= 0 && z < Width;
        }

        /// <summary>
        /// Checks, if the index is valid for this chunk size.
        /// </summary>
        /// <param name="index">An index value</param>
        /// <returns>True, if the index is valid, otherwise false</returns>
        public bool IsValid(int index)
        {
            return index >= 0 && index < Size;
        }
        
        /// <summary>
        /// Transforms the local coordinates of a block within a chunk into the index of the block within the chunks
        /// array.
        /// </summary>
        /// <param name="x">The X coordinate</param>
        /// <param name="y">The Y coordinate</param>
        /// <param name="z">The Z coordinate</param>
        /// <returns>The index of a block in the chunks array.</returns>
        public int ToIndex(int x, int y, int z)
        {
            return y * Width * Width + z * Width + x;
        }

        /// <summary>
        /// Transforms the local position of a block within a chunk into the index of the block within the chunks
        /// array.
        /// </summary>
        /// <param name="localPosition">A vector defining the location position</param>
        /// <returns>The index of a block in the chunks array.</returns>
        public int ToIndex(Vector3Int localPosition)
        {
            return ToIndex(localPosition.x, localPosition.y, localPosition.z);
        }

        /// <summary>
        /// Returns the local position of a block in a chunk defined by its index in the chunks array.
        /// </summary>
        /// <param name="index">The index of the block in the chunks array</param>
        /// <returns>A vector containing the local position of the block within a chunk.</returns>
        public Vector3Int ToPosition(int index)
        {
            var y = index / (Width * Width);
            var z = (index - y * Width * Width) / Width;
            var x = index - z * Width - y * Width * Width;
            return new Vector3Int(x, y, z);
        }
        
        /// <summary>
        /// Returns a string representation of the chunk size.
        /// </summary>
        /// <returns>A string containing chunk size information.</returns>
        public override string ToString()
        {
            return "ChunkSize[Width=" + Width + "; Height=" + Height + "]";
        }
    }
}