using UnityEngine;

namespace Blox.ActionsNS
{
    /// <summary>
    /// This is the abstract base component of an action.
    /// </summary>
    public abstract class Action : MonoBehaviour
    {
        /// <summary>
        /// The name of the action.
        /// </summary>
        public string ActionName;

        /// <summary>
        /// Checks, if the action is active or not dependend on the action managers state.
        /// </summary>
        /// <param name="currentState">The action managers current state</param>
        /// <param name="lastState">The action managers last state</param>
        /// <returns>True, if the action is active, otherwise false</returns>
        public abstract bool IsActive(ActionManager.State currentState, ActionManager.State lastState);

        /// <summary>
        /// Returns a string representation of this action.
        /// </summary>
        /// <returns>A string representation</returns>
        public override string ToString()
        {
            return $"{ActionName}";
        }
    }
}