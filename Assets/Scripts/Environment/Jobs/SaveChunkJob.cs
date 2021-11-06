using System;
using System.IO;
using Unity.Collections;
using Unity.Jobs;

namespace Blox.EnvironmentNS.JobsNS
{
    /// <summary>
    /// This job saves the content of a chunk data container to the filesystem.
    /// </summary>
    public struct SaveChunkJob : IJob, IDisposable
    {
        /// <summary>
        /// The native array contains the char array for the path.
        /// </summary>
        [ReadOnly] private NativeArray<char> m_PathArray;

        /// <summary>
        /// The native array with the content of the chunk data container
        /// </summary>
        [ReadOnly] private NativeArray<int> m_Content;
        
        /// <summary>
        /// Initializes this job.
        /// </summary>
        /// <param name="chunkData">The chunk data container</param>
        /// <param name="path">The path to the file</param>
        public void Initialize(ChunkData chunkData, string path)
        {
            m_Content = new NativeArray<int>(chunkData.ToArray(), Allocator.Persistent);
            m_PathArray = new NativeArray<char>(path.ToCharArray(), Allocator.Persistent);
        }
 
        /// <summary>
        /// Executes this job.
        /// </summary>
        public void Execute()
        {
            var path = new string(m_PathArray.ToArray());
            var bytes = new byte[m_Content.Length * 4];
            var index = 0;
            foreach (var id in m_Content)
            {
                bytes[index++] = (byte)((id & 0xFF000000) >> 3);
                bytes[index++] = (byte)((id & 0xFF0000) >> 2);
                bytes[index++] = (byte)((id & 0xFF00) >> 1);
                bytes[index++] = (byte)((id & 0xFF) >> 0);
            }
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// Disposes all resources of this job.
        /// </summary>
        public void Dispose()
        {
            m_Content.Dispose();
            m_PathArray.Dispose();
        }
    }
}