using Blox.Environment;
using Blox.Environment.Config;
using Common;

namespace Blox.Actions
{
    public class ActionPutBlockToChunk : Action<ActionPutBlockToChunk.Input, bool>
    {
        public struct Input
        {
            public Position position;
            public int blockTypeId;
            public BlockFace face;
        }


        public ActionPutBlockToChunk(Input input) : base(input)
        {
        }

        protected override bool Execute(Input input)
        {
            if (input.blockTypeId > 0)
            {
                var chunkManager = ChunkManager.GetInstance();
                var newPosition = input.position[input.face];
                var chunk = chunkManager[newPosition.chunk];
                chunk.SetBlockType(newPosition.local, input.blockTypeId);
                chunkManager.RefreshChunkMesh(chunk, false);
                return true;
            }
            return false;
        }
    }
}