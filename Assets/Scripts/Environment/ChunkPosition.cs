using UnityEngine;

namespace Blox.Environment
{
    public class ChunkPosition
    {
        public static readonly ChunkPosition Zero = new ChunkPosition(0, 0);

        public static ChunkPosition FromGlobalPosition(ChunkSize size, int x, int z)
        {
            var cx = x > 0 ? x / size.width : x - size.width - 1 / size.width;
            var cz = z > 0 ? z / size.width : z - size.width - 1 / size.width;
            return new ChunkPosition(cx, cz);
        }

        public static ChunkPosition FromGlobalPosition(ChunkSize size, Vector3Int position)
        {
            return FromGlobalPosition(size, position.x, position.z);
        }

        public static string GetCacheKey(int x, int z)
        {
            return "[" + x + ":" + z + "]";
        }
        
        public readonly int x;
        public readonly int z;

        public string cacheKey => GetCacheKey(x, z);
        public string cacheFilename => "chunk_" + x + "_" + z + ".dat";
        
        public ChunkPosition(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj is ChunkPosition position)
                return x == position.x && z == position.z;

            return false;
        }

        public override int GetHashCode()
        {
            return x + z * 65536;
        }

        public override string ToString()
        {
            return "ChunkPosition[X=" + x + "; Z=" + z + "]";
        }
    }
}