using UnityEngine;

namespace Blox.TerrainNS
{
    public readonly struct ChunkPosition
    {
        public static ChunkPosition Zero = new ChunkPosition(0, 0);

        public static ChunkPosition From(ChunkSize chunkSize, Vector3Int position) =>
            From(chunkSize, position.x, position.z);

        public static ChunkPosition From(ChunkSize chunkSize, int globalX, int globalZ)
        {
            var cx = globalX >= 0 ? globalX / chunkSize.width : (globalX - chunkSize.width + 1) / chunkSize.width;
            var cz = globalZ >= 0 ? globalZ / chunkSize.width : (globalZ - chunkSize.width + 1) / chunkSize.width;
            return new ChunkPosition(cx, cz);
        }

        public readonly int x;
        public readonly int z;

        public string cacheFilename => $"_{x}_{z}_.chunkdata";
        public string chunkName => $"Chunk {x}:{z}";
        public ChunkPosition left => new ChunkPosition(x - 1, z);
        public ChunkPosition right => new ChunkPosition(x + 1, z);
        public ChunkPosition front => new ChunkPosition(x - 1, z - 1);
        public ChunkPosition back => new ChunkPosition(x - 1, z + 1);

        public ChunkPosition(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public override bool Equals(object obj)
        {
            if (obj is ChunkPosition chunkPosition)
                return x == chunkPosition.x && z == chunkPosition.z;

            return false;
        }

        public override int GetHashCode()
        {
            return x + 65535 * z;
        }

        public override string ToString()
        {
            return $"ChunkPosition(X={x}; Z={z})";
        }

        public Vector3Int ToLocalPosition(ChunkSize chunkSize, Vector3Int position)
        {
            return ToLocalPosition(chunkSize, position.x, position.y, position.z);
        }

        public Vector3Int ToLocalPosition(ChunkSize chunkSize, int globalX, int globalY, int globalZ)
        {
            var xx = globalX - this.x * chunkSize.width;
            var zz = globalZ - this.z * chunkSize.width;
            return new Vector3Int(xx, globalY, zz);
        }

        public Vector3 ToWorldPosition(ChunkSize chunkSize)
        {
            return new Vector3(x * chunkSize.width, 0f, z * chunkSize.width);
        }
    }
}