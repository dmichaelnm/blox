using Blox.ConfigurationNS;
using Blox.PlayerNS;
using UnityEngine;

namespace Blox.GameNS
{
    /// <summary>
    /// This static class defines delegates for events.
    /// </summary>
    public static class Events
    {
        /// <summary>
        /// This delegate can be used for an empty event.
        /// </summary>
        public delegate void EmptyEvent();
        
        /// <summary>
        /// This delegate can be used for a component related event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public delegate void ComponentEvent<in T>(T component) where T : Component;

        /// <summary>
        /// This delegate can be used for events that are send when the player is submerged or emerged.
        /// </summary>
        public delegate void BooleanEvent(bool underwater);

        /// <summary>
        /// This delegate can be used for events that submits a player position.
        /// </summary>
        public delegate void PlayerPositionEvent(PlayerPosition position);

        /// <summary>
        /// This delegate can be used for events of the selection of a block.
        /// </summary>
        public delegate void PlayerSelectionEvent(Vector3Int position, BlockType blockType, BlockFace face,
            PlayerSelection.MouseButtonState mouseButtonState);
    }
}