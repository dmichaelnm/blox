using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.PlayerNS;
using UnityEngine;

namespace Blox.GameNS.ActionNS
{
    public class ActionAddBlockToChunk : Action
    {
        private Vector3Int m_SelectedPosition;

        public override bool IsActionStarted(bool firstCall)
        {
            if (firstCall)
            {
                var inventory = m_GameManager.inventory;
                var slot = inventory.selectedSlot;
                if (slot != null && slot.count > 0)
                {
                    if (m_GameManager.mouseButtonState != null && m_GameManager.selectedPosition != null &&
                        m_GameManager.selectedBlockFace != null &&
                        (m_GameManager.mouseButtonState & PlayerSelection.MouseButtonState.RightButtonDown) != 0)
                    {
                        m_SelectedPosition = ((Vector3)m_GameManager.selectedPosition).ToVector3Int();
                        m_SelectedPosition = m_SelectedPosition.Neighbour((BlockFace)m_GameManager.selectedBlockFace);
                        return true;
                    }
                }

                return false;
            }

            if (m_GameManager.mouseButtonState == null || m_GameManager.selectedPosition == null ||
                m_GameManager.selectedBlockFace == null ||
                m_GameManager.mouseButtonState == PlayerSelection.MouseButtonState.RightButtonUp)
                return false;

            var selectedFace = (BlockFace)m_GameManager.selectedBlockFace;
            var selectedPosition = ((Vector3)m_GameManager.selectedPosition).ToVector3Int();
            selectedPosition = selectedPosition.Neighbour(selectedFace);
            if (!selectedPosition.Equals(m_SelectedPosition))
                return false;

            return true;
        }

        public override void OnFinished()
        {
            var inventory = m_GameManager.inventory;
            if (inventory.RemoveItem(out var entity))
            {
                var chunkManager = m_GameManager.chunkManager;
                chunkManager.SetEntity(m_SelectedPosition, entity, true, false);
            }
        }
    }
}