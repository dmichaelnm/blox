using Blox.ConfigurationNS;
using Blox.EnvironmentNS;
using Blox.PlayerNS;
using Blox.UINS;
using UnityEngine;

namespace Blox.ActionsNS
{
    /// <summary>
    /// This component controls the removing of a block from the world and putting it into the inventory.
    /// </summary>
    public class RemoveBlockFromWorldAction : ProgressAction
    {
        /// <summary>
        /// The chunk manager component
        /// </summary>
        [SerializeField] private ChunkManager m_ChunkManager;

        /// <summary>
        /// The inventory component.
        /// </summary>
        [SerializeField] private Inventory m_Inventory;

        protected override bool StartCondidition(ActionManager.State currentState, ActionManager.State lastState)
        {
            return currentState.MouseButtonState == PlayerSelection.MouseButtonState.LeftButtonDown &&
                   currentState.SelectedBlock;
        }

        protected override bool ProgressCondition(ActionManager.State currentState, ActionManager.State lastState)
        {
            return currentState.MouseButtonState == PlayerSelection.MouseButtonState.None &&
                   currentState.SelectedBlock &&
                   currentState.SelectedBlockPosition.Equals(lastState.SelectedBlockPosition);
        }

        protected override bool CancelCondition(ActionManager.State currentState, ActionManager.State lastState)
        {
            return currentState.MouseButtonState == PlayerSelection.MouseButtonState.LeftButtonUp ||
                   !currentState.SelectedBlock ||
                   !currentState.SelectedBlockPosition.Equals(lastState.SelectedBlockPosition);
        }

        protected override void OnActionProgressStarted()
        {
        }

        protected override void OnActionProgressFinished(ActionManager.State state)
        {
            // Remove the block from the chunk data container
            var position = state.SelectedBlockPosition;
            var chunkPosition = ChunkPosition.FromGlobalPosition(m_ChunkManager.ChunkSize, position);
            var chunkData = m_ChunkManager[chunkPosition];
            var localPosition = chunkPosition.ToLocalPosition(m_ChunkManager.ChunkSize, position);
            
            // TODO check the surrounding blocks for fluid
            chunkData.SetBlock(localPosition, (int)BlockType.IDs.Air);

            // Refresh the chunk
            m_ChunkManager.RecreateChunk(chunkData);

            // If the selected block is on a chunk border the neighbour chunk must also be created
            if (localPosition.x == 0)
                m_ChunkManager.RecreateChunk(chunkData.Left);
            else if (localPosition.x == m_ChunkManager.ChunkSize.Width - 1)
                m_ChunkManager.RecreateChunk(chunkData.Right);
            if (localPosition.z == 0)
                m_ChunkManager.RecreateChunk(chunkData.Front);
            else if (localPosition.z == m_ChunkManager.ChunkSize.Width - 1)
                m_ChunkManager.RecreateChunk(chunkData.Back);

            // Add the block to the inventory
            m_Inventory.AddBlock(state.SelectedBlockType.BaseID);

            m_ChunkManager.StartFillingFluidBlocks(position);
        }

        protected override void OnActionProgressCancelled()
        {
        }
    }
}