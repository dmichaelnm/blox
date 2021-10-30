using System.IO;
using Unity.Collections;

namespace Blox.Environment.Jobs
{
    public struct ChunkLoadingJob : IChunkDataProvider
    {
        [ReadOnly] private NativeArray<char> m_Path;
        private NativeArray<int> m_BlockTypeIds;
        private int m_ChunkPositionX;
        private int m_ChunkPositionZ;

        public void Initialize(ChunkSize size, ChunkPosition position, string path)
        {
            m_ChunkPositionX = position.x;
            m_ChunkPositionZ = position.z;

            m_Path = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
            
            var length = size.width * size.width * size.height;
            m_BlockTypeIds = new NativeArray<int>(length, Allocator.Persistent);
        }

        public void Execute()
        {
            var path = new string(m_Path.ToArray());
            var bytes = File.ReadAllBytes(path);
            var index = 0;
            for (var i = 0; i < m_BlockTypeIds.Length; i++)
            {
                var b3 = (bytes[index++] & 0xFF) << 3;
                var b2 = (bytes[index++] & 0xFF) << 2;
                var b1 = (bytes[index++] & 0xFF) << 1;
                var b0 = (bytes[index++] & 0xFF) << 0;
                var id = b3 + b2 + b1 + b0;
                m_BlockTypeIds[i] = id;
            }
        }

        public void Dispose()
        {
            m_BlockTypeIds.Dispose();
            m_Path.Dispose();
        }

        public int[] GetBlockTypeIdArray() => m_BlockTypeIds.ToArray();

        public ChunkPosition GetChunkPosition() => new ChunkPosition(m_ChunkPositionX, m_ChunkPositionZ);
    }
}