using System;
using System.IO;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Jobs;

namespace Blox.TerrainNS.JobsNS
{
    public struct SaveChunkDataJob : IJob, IDisposable
    {
        [ReadOnly] private NativeArray<char> m_CacheFilename;
        [ReadOnly] private NativeArray<int> m_BlockTypeIds;

        public void Initialize([NotNull] ChunkData chunkData, [NotNull] string cacheFile)
        {
            m_CacheFilename = new NativeArray<char>(cacheFile.ToCharArray(), Allocator.Persistent);
            m_BlockTypeIds = new NativeArray<int>(chunkData.ToArray(), Allocator.Persistent);
        }

        public void Execute()
        {
            var path = new string(m_CacheFilename.ToArray());
            var bytes = new byte[m_BlockTypeIds.Length * 4];
            var j = 0;
            foreach (var id in m_BlockTypeIds)
            {
                bytes[j++] = (byte)(id & 0x000000FF);
                bytes[j++] = (byte)((id & 0x0000FF00) >> 8);
                bytes[j++] = (byte)((id & 0x00FF0000) >> 16);
                bytes[j++] = (byte)((id & 0xFF000000) >> 24);
            }

            File.WriteAllBytes(path, bytes);
        }

        public void Dispose()
        {
            m_CacheFilename.Dispose();
            m_BlockTypeIds.Dispose();
        }
    }
}