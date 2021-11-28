using System;
using UnityEngine;

namespace Blox.TerrainNS
{
    [Serializable]
    public struct ChunkSize
    {
        public int width;
        public int height;

        public int size => width * width * height;

        public bool IsValid(int x, int y, int z)
        {
            return x >= 0 && x < width && y >= 0 && y < height && z >= 0 && z < width;
        }

        public int ToIndex(int x, int y, int z)
        {
            return x + z * width + y * width * width;
        }

        public Vector3Int ToPosition(int index)
        {
            var y = index / width / width;
            var z = (index - y * width * width) / width;
            var x = index - y * width * width - z * width;
            return new Vector3Int(x, y, z);
        }

        public override string ToString()
        {
            return $"ChunkSize(Width={width}; Height={height}; Size={size})";
        }
    }
}