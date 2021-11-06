using Blox.EnvironmentNS;
using Blox.PlayerNS;
using Blox.UINS;
using Blox.UtilitiesNS;
using UnityEngine;

namespace Blox.ActionsNS
{
    /// <summary>
    /// This component controls the adding of a block to the world.
    /// </summary>
    public class AddBlockToWorldAction : ProgressAction
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
            return currentState.MouseButtonState == PlayerSelection.MouseButtonState.RightButtonDown &&
                   currentState.SelectedBlock && m_Inventory.GetSelectedBlockTypeID() > 0;
        }

        protected override bool ProgressCondition(ActionManager.State currentState, ActionManager.State lastState)
        {
            return currentState.MouseButtonState == PlayerSelection.MouseButtonState.None &&
                   currentState.SelectedBlock && m_Inventory.GetSelectedBlockTypeID() > 0;
        }

        protected override bool CancelCondition(ActionManager.State currentState, ActionManager.State lastState)
        {
            return currentState.MouseButtonState == PlayerSelection.MouseButtonState.RightButtonUp ||
                   !currentState.SelectedBlock ||
                   !currentState.SelectedBlockPosition.Equals(lastState.SelectedBlockPosition) ||
                   m_Inventory.GetSelectedBlockTypeID() == 0;
        }

        protected override void OnActionProgressStarted()
        {
        }

        protected override void OnActionProgressFinished(ActionManager.State state)
        {
            // Add the block to the world
            var blockTypeId = m_Inventory.GetSelectedBlockTypeID();
            var position = MathUtilities.Neighbour(state.SelectedBlockPosition, state.SelectedBlockFace);
            var chunkPosition = ChunkPosition.FromGlobalPosition(m_ChunkManager.ChunkSize, position);
            var chunkData = m_ChunkManager[chunkPosition];
            var localPosition = chunkPosition.ToLocalPosition(m_ChunkManager.ChunkSize, position);
            chunkData.SetBlock(localPosition, blockTypeId);

            // Refresh the chunk
            m_ChunkManager.RecreateChunk(chunkData);
            
            // Remove the block from the inventory
            m_Inventory.RemoveBlock();
        }

        protected override void OnActionProgressCancelled()
        {
        }
    }
}