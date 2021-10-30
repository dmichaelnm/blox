using Blox.Environment;
using Blox.Utility;
using UnityEngine;

namespace Common
{
    public class Position
    {
        public static Vector3Int ToLocal(ChunkSize size, ChunkPosition chunkPos, Vector3Int globalPos)
        {
            var lx = globalPos.x - chunkPos.x * size.width;
            var ly = globalPos.y;
            var lz = globalPos.z - chunkPos.z * size.width;
            return new Vector3Int(lx, ly, lz);
        }

        public static Vector3Int ToGlobal(ChunkSize size, ChunkPosition chunkPos, Vector3Int localPos)
        {
            var gx = chunkPos.x * size.width + localPos.x;
            var gy = localPos.y;
            var gz = chunkPos.z * size.width + localPos.z;
            return new Vector3Int(gx, gy, gz);
        }

        public static ChunkPosition ToChunk(ChunkSize size, Vector3Int globalPos)
        {
            var cx = globalPos.x >= 0
                ? globalPos.x / size.width
                : (globalPos.x - size.width + 1) / size.width;
            var cz = globalPos.z >= 0
                ? globalPos.z / size.width
                : (globalPos.z - size.width + 1) / size.width;
            return new ChunkPosition(cx, cz);
        }

        public readonly Vector3 raw;
        public readonly Vector3Int global;
        public readonly Vector3Int local;
        public readonly ChunkPosition chunk;

        public Vector3 rawLocal => new Vector3(local.x, local.y, local.z);

        public Position(ChunkSize size, ChunkPosition chunkPos, int localX, int localY, int localZ) : this(size,
            chunkPos, new Vector3Int(localX, localY, localZ))
        {
        }

        public Position(ChunkSize size, ChunkPosition chunkPos, Vector3Int localPos)
        {
            chunk = chunkPos;
            local = localPos;
            global = ToGlobal(size, chunkPos, localPos);
            raw = new Vector3(global.x + 0.5f, global.y + 0.5f, global.z + 0.5f);
        }

        public Position(ChunkSize size, Vector3 rawPos)
        {
            raw = rawPos;
            global = new Vector3Int(MathUtility.Floor(rawPos.x), MathUtility.Floor(rawPos.y),
                MathUtility.Floor(rawPos.z));
            chunk = ToChunk(size, global);
            local = ToLocal(size, chunk, global);
        }

        public override bool Equals(object obj)
        {
            if (obj is Position pos)
                return global.Equals(pos.global);

            return false;
        }

        public override int GetHashCode()
        {
            return global.x + global.z * 10000 + global.y * 100000000;
        }

        public override string ToString()
        {
            return "Position[Raw=" + raw + ", Gloabl=" + global + ", Local=" + local + ", Chunk=" + chunk + "]";
        }
    }
}