namespace Blox.Environment.Jobs
{
    public interface IChunkDataProvider : IJob
    {
        public int[] GetBlockTypeIdArray();

        public ChunkPosition GetChunkPosition();
    }
}