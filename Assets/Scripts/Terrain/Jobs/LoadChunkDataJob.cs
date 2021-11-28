using System.IO;
using JetBrains.Annotations;
using Unity.Collections;
using UnityEngine;

namespace Blox.TerrainNS.JobsNS
{
    public struct LoadChunkDataJob : IChunkDataProviderJob
    {
        [ReadOnly] private ChunkPosition m_ChunkPosition;
        [ReadOnly] private NativeArray<char> m_CacheFilename;

        private NativeArray<int> m_BlockTypeIds;

        public void Initialize(ChunkSize chunkSize, ChunkPosition chunkPosition, [NotNull] string cacheFilename)
        {
            m_ChunkPosition = chunkPosition;
            m_CacheFilename = new NativeArray<char>(cacheFilename.ToCharArray(), Allocator.Persistent);
            m_BlockTypeIds = new NativeArray<int>(chunkSize.size, Allocator.Persistent);
        }

        public void Execute()
        {
            var path = new string(m_CacheFilename.ToArray());
            var bytes = File.ReadAllBytes(path);
            Debug.Assert(m_BlockTypeIds.Length * 4 == bytes.Length, $"\"{path}\": sizes does not match.");
            var j = 0;
            for (var i = 0; i < m_BlockTypeIds.Length; i++)
            {
                var b0 = bytes[j++] << 0;
                var b1 = bytes[j++] << 8;
                var b2 = bytes[j++] << 16;
                var b3 = bytes[j++] << 24;
                m_BlockTypeIds[i] = b0 | b1 | b2 | b3;
            }
        }

        public void Dispose()
        {
            m_CacheFilename.Dispose();
            m_BlockTypeIds.Dispose();
        }

        public ChunkPosition GetChunkPosition()
        {
            return m_ChunkPosition;
        }

        public int[] GetBlockTypeIds()
        {
            return m_BlockTypeIds.ToArray();
        }
    }
}