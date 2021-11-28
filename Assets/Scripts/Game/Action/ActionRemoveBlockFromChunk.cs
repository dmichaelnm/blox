using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.PlayerNS;
using UnityEngine;

namespace Blox.GameNS.ActionNS
{
    public class ActionRemoveBlockFromChunk : Action
    {
        private Vector3Int m_SelectedPosition;

        public override bool IsActionStarted(bool firstCall)
        {
            if (firstCall)
            {
                if (m_GameManager.mouseButtonState != null && m_GameManager.selectedPosition != null &&
                    (m_GameManager.mouseButtonState & PlayerSelection.MouseButtonState.LeftButtonDown) != 0)
                {
                    m_SelectedPosition = ((Vector3)m_GameManager.selectedPosition).ToVector3Int();
                    return true;
                }

                return false;
            }

            if (m_GameManager.mouseButtonState == null || m_GameManager.selectedPosition == null ||
                m_GameManager.mouseButtonState == PlayerSelection.MouseButtonState.LeftButtonUp)
                return false;

            var selectedPosition = ((Vector3)m_GameManager.selectedPosition).ToVector3Int();
            if (!selectedPosition.Equals(m_SelectedPosition))
                return false;

            return true;
        }

        public override void OnFinished()
        {
            var config = m_GameManager.configuration;
            var chunkManager = m_GameManager.chunkManager;
            var blockType = chunkManager.GetEntity<BlockType>(m_SelectedPosition);

            if (m_GameManager.inventory.AddItem(blockType.ItemBlockType))
            {
                chunkManager.SetEntity(m_SelectedPosition, config.GetEntityType<BlockType>(BlockType.ID.Air));
            }
        }
    }
}