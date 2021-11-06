using System;

namespace Blox.ActionsNS
{
    /// <summary>
    /// This is the abstract component for action that have a progress timer.
    /// </summary>
    public abstract class ProgressAction : Action
    {
        /// <summary>
        /// The needed time of the progress for this action.
        /// </summary>
        public float NeededProgressTime = 1f;
        
        /// <summary>
        /// A flag that holds the info about the start of this action.
        /// </summary>
        private bool m_Started;

        /// <summary>
        /// A flag that indicates that the progress has finished.
        /// </summary>
        private bool m_Finished;

        public override bool IsActive(ActionManager.State currentState, ActionManager.State lastState)
        {
            // Chec if action has finished
            if (m_Finished)
            {
                m_Finished = false;
                m_Started = false;
                return false;
            }

            // Check if this action has started
            if (!m_Started && StartCondidition(currentState, lastState))
            {
                OnActionProgressStarted();
                m_Started = true;
                return true;
            }

            // Check if this action remains active.
            if (m_Started && ProgressCondition(currentState, lastState))
            {
                return true;
            }

            // Check if this action has been cancelled
            if (!m_Started || CancelCondition(currentState, lastState))
            {
                OnActionProgressCancelled();
                m_Started = false;
                return false;
            }

            throw new Exception("Invalid action state: " + ToString());
        }

        /// <summary>
        /// Marks the progress as finished.
        /// </summary>
        /// <param name="state">The action managers current state</param>
        public void ProgressFinished(ActionManager.State state)
        {
            m_Finished = true;
            OnActionProgressFinished(state);
        }

        /// <summary>
        /// Returns the start condition of this action.
        /// </summary>
        /// <param name="currentState">The action managers current state</param>
        /// <param name="lastState">The action managers last state</param>
        /// <returns>True if the action has started, otherwise false</returns>
        protected abstract bool StartCondidition(ActionManager.State currentState, ActionManager.State lastState);

        /// <summary>
        /// Returns the condition for remaining in progress.
        /// </summary>
        /// <param name="currentState">The action managers current state</param>
        /// <param name="lastState">The action managers last state</param>
        /// <returns>True if the action remains in progress, otherwise false</returns>
        protected abstract bool ProgressCondition(ActionManager.State currentState, ActionManager.State lastState);

        /// <summary>
        /// Returns the condition for cancel the action.
        /// </summary>
        /// <param name="currentState">The action managers current state</param>
        /// <param name="lastState">The action managers last state</param>
        /// <returns>True if the action has canceled, otherwise false</returns>
        protected abstract bool CancelCondition(ActionManager.State currentState, ActionManager.State lastState);
        
        /// <summary>
        /// This method is called when the progress of this action has started.
        /// </summary>
        protected abstract void OnActionProgressStarted();

        /// <summary>
        /// This method is called when the progress of this action has finished.
        /// </summary>
        /// <param name="state">The action managers current state</param>
        protected abstract void OnActionProgressFinished(ActionManager.State state);

        /// <summary>
        /// This method is called when the progress of this action has cancelled.
        /// </summary>
        protected abstract void OnActionProgressCancelled();
    }
}