using System.Collections.Generic;
using Blox.Environment;
using Blox.Environment.Config;
using Blox.Player;
using Blox.Actions;
using Blox.UI;
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
            // left mouse button click
            if ((mousebuttonstate & PlayerSelection.MouseButtonState.LeftButtonUp) != 0)
            {
                // remove the block from the world
                var blockTypeId = new ActionRemoveBlockFromChunk(position).Invoke();
                // add block to inventory
                new ActionPutBlockToInventory(blockTypeId).Invoke();
            }
            // right mouse button click
            else if ((mousebuttonstate & PlayerSelection.MouseButtonState.RightButtonUp) != 0)
            {
                var inventory = Inventory.GetInstance();
                var blockTypeId = inventory.selectedBlockTypeId;
                // remove the block from inventory
                if (new ActionRemoveBlockFromInventory(blockTypeId).Invoke())
                {
                    // put the block in the chunk
                    var input = new ActionPutBlockToChunk.Input();
                    input.position = position;
                    input.blockTypeId = blockTypeId;
                    input.face = face;
                    new ActionPutBlockToChunk(input).Invoke();
                }
            }
        }
    }
}