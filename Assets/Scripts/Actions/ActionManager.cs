using System.Collections.Generic;
using Blox.Environment;
using Blox.Environment.Config;
using Blox.Player;
using Blox.Actions;
using Common;
using UnityEngine;

namespace Blox.Actions
{
    public class ActionManager : MonoBehaviour
    {
        private ChunkManager m_ChunkManager;
        
        private void Awake()
        {
            m_ChunkManager = ChunkManager.GetInstance();
            
            var blockSelection = GameObject.Find("Selection Block").GetComponent<PlayerSelection>();
            blockSelection.onBlockSelection += OnBlockSelection;
        }

        private void OnBlockSelection(Position position, BlockFace face,
            PlayerSelection.MouseButtonState mousebuttonstate)
        {
            if ((mousebuttonstate & PlayerSelection.MouseButtonState.LeftButtonUp) != 0)
            {
                // remove the block from the world
                var blockTypeId = new ActionRemoveBlockFromChunk(position).Invoke();
                // add block to inventory
                new ActionPutBlockToInventory(blockTypeId).Invoke();
            }
        }
    }
}