using System.IO;
using Unity.Collections;
using UnityEngine.Assertions;

namespace Blox.EnvironmentNS.JobsNS
{
    /// <summary>
    /// This job loads the chunk data from the temporary directory for a given chunk position.
    /// </summary>
    public struct LoadChunkJob : IChunkDataProvider
    {
        /// <summary>
        /// The native array containing the path to the chunk data container file.
        /// </summary>
        [ReadOnly] private NativeArray<char> m_Path;
        
        /// <summary>
        /// The chunk position.
        /// </summary>
        [ReadOnly] private ChunkPosition m_ChunkPosition;
        
        /// <summary>
        /// The array containing the loaded information.
        /// </summary>
        private NativeArray<int> m_Blocks;
        
        /// <summary>
        /// Initializes this job.
        /// </summary>
        /// <param name="chunkSize">The size of the chunk</param>
        /// <param name="chunkPosition">The position of the chunk</param>
        /// <param name="path">The path to the file.</param>
        public void Initialize(ChunkSize chunkSize, ChunkPosition chunkPosition, string path)
        {
            m_ChunkPosition = chunkPosition;
            
            m_Path = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
            m_Blocks = new NativeArray<int>(chunkSize.Size, Allocator.Persistent);
        }
        
        /// <summary>
        /// Executes the loading job.
        /// </summary>
        public void Execute()
        {
            var path = new string(m_Path.ToArray());
            var bytes = File.ReadAllBytes(path);
            
            // Check if the size of the byte array is valid
            Assert.IsTrue(bytes.Length == m_Blocks.Length * 4);
            
            var index = 0;
            for (var i = 0; i < m_Blocks.Length; i++)
            {
                var b3 = (bytes[index++] & 0xFF) << 3;
                var b2 = (bytes[index++] & 0xFF) << 2;
                var b1 = (bytes[index++] & 0xFF) << 1;
                var b0 = (bytes[index++] & 0xFF) << 0;
                m_Blocks[i] = b3 + b2 + b1 + b0;
            }
        }

        /// <summary>
        /// Disposes the allocated memory used by this job.
        /// </summary>
        public void Dispose()
        {
            m_Path.Dispose();
            m_Blocks.Dispose();
        }

        public ChunkPosition GetChunkPosition()
        {
            return m_ChunkPosition;
        }

        public int[] GetResult()
        {
            return m_Blocks.ToArray();
        }
    }
}