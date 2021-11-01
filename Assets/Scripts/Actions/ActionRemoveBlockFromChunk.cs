using Blox.Environment;
using Blox.Environment.Config;
using Common;
using UnityEngine;

namespace Blox.Actions
{
    public class ActionRemoveBlockFromChunk : Action<Position, int>
    {
        public ActionRemoveBlockFromChunk(Position input) : base(input)
        {
        }

        protected override int Execute(Position position)
        {
            var config = Configuration.GetInstance();
            var chunkManager = ChunkManager.GetInstance();
            var chunkData = chunkManager[position.chunk];
            var blockTypeId = chunkData[position.local].id;
            var blockType = config.GetBlockType(blockTypeId);

            var idTop = GetBlockTypeId(chunkData, position, BlockFace.Top);
            var idLeft = GetBlockTypeId(chunkData, position, BlockFace.Left);
            var idRight = GetBlockTypeId(chunkData, position, BlockFace.Right);
            var idFront = GetBlockTypeId(chunkData, position, BlockFace.Front);
            var idBack = GetBlockTypeId(chunkData, position, BlockFace.Back);
            var newBlockTypeId = Mathf.Max(idTop, idLeft, idRight, idFront, idBack);
            
            chunkData.SetBlockType(position.local, newBlockTypeId);
            chunkManager.RefreshChunkMesh(chunkData, false);

            var cx = position.chunk.x;
            var cz = position.chunk.z;

            if (position.local.x == 0)
                cx--;
            else if (position.local.x == chunkManager.chunkSize.width - 1)
                cx++;

            if (position.local.z == 0)
                cz--;
            else if (position.local.z == chunkManager.chunkSize.width - 1)
                cx++;

            var neighbour = chunkManager[cx, cz];
            chunkManager.RefreshChunkMesh(neighbour, false);
            
            return blockType.baseId;
        }

        private int GetBlockTypeId(ChunkData chunkData, Position position, BlockFace face)
        {
            var blockType = chunkData[position.local, face];
            if (blockType.isFluid)
                return blockType.id;
            return (int)BlockTypes.Air;
        }
    }
}