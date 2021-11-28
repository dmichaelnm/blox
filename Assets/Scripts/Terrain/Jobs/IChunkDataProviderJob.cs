using System;
using Unity.Jobs;

namespace Blox.TerrainNS.JobsNS
{
    public interface IChunkDataProviderJob : IJob, IDisposable
    {
        public ChunkPosition GetChunkPosition();

        public int[] GetBlockTypeIds();
    }
}