using System;
using Unity.Jobs;

namespace Blox.EnvironmentNS.JobsNS
{
    /// <summary>
    /// A job that provides the chunk data information.
    /// </summary>
    public interface IChunkDataProvider : IJob, IDisposable
    {
        /// <summary>
        /// Returns the position of the chunk data container.
        /// </summary>
        /// <returns></returns>
        public ChunkPosition GetChunkPosition();
        
        /// <summary>
        /// Returns the array with the loaded information.
        /// </summary>
        /// <returns>The result array.</returns>
        public int[] GetResult();
    }
}