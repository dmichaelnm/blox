using System;
using UnityEngine;

namespace Blox.Environment
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

        public bool IsValid(Vector3Int position)
        {
            return IsValid(position.x, position.y, position.z);
        }

        public bool IsValid(int index)
        {
            return index >= 0 && index < size;
        }
        
        public int ToIndex(int x, int y, int z)
        {
            if (IsValid(x, y, z))
                return x + z * width + y * width * width;

            return -1;
        }

        public int ToIndex(Vector3Int position)
        {
            return ToIndex(position.x, position.y, position.z);
        }

        public Vector3Int ToPosition(int index)
        {
            if (IsValid(index))
            {
                var y = index / (width * width);
                var z = (index - y * width * width) / width;
                var x = index - z * width - y * width * width;

                return new Vector3Int(x, y, z);
            }

            Debug.LogError("The index " + index + " is not valid for chunk size " + this);
            return default;
        }

        public override string ToString()
        {
            return "ChunkSize[Width=" + width + "; Height=" + height + "]";
        }
    }
}