using Blox.ConfigurationNS;
using Blox.PlayerNS;
using Blox.UINS;
using UnityEngine;

namespace Blox.ActionsNS
{
    /// <summary>
    /// This component manages the actions of the game.
    /// </summary>
    public class ActionManager : MonoBehaviour
    {
        /// <summary>
        /// This struct contains several states from the game and is the base for actions to decide what to do.
        /// </summary>
        public struct State
        {
            /// <summary>
            /// A flag indicating a block is selected or not.
            /// </summary>
            public bool SelectedBlock;

            /// <summary>
            /// The position of the selected block.
            /// </summary>
            public Vector3Int SelectedBlockPosition;

            /// <summary>
            /// The type of the selected block.
            /// </summary>
            public BlockType SelectedBlockType;

            /// <summary>
            /// The face of the selected block.
            /// </summary>
            public BlockFace SelectedBlockFace;

            /// <summary>
            /// The current mouse button state.
            /// </summary>
            public PlayerSelection.MouseButtonState MouseButtonState;
        }

        /// <summary>
        /// The player selection component.
        /// </summary>
        [SerializeField] private PlayerSelection m_PlayerSelection;

        /// <summary>
        /// The progress bar component.
        /// </summary>
        [SerializeField] private ProgressBar m_ProgressBar;

        /// <summary>
        /// All registered actions.
        /// </summary>
        private Action[] m_Actions;

        /// <summary>
        /// The current state.
        /// </summary>
        private State m_State;

        /// <summary>
        /// The last state.
        /// </summary>
        private State m_LastState;
        
        /// <summary>
        /// The currently active action.
        /// </summary>
        private Action m_CurrentAction;

        /// <summary>
        /// The progress timer of the current active action.
        /// </summary>
        private float m_ProgressTimer;

        /// <summary>
        /// This method is called when the component is created.
        /// </summary>
        private void Awake()
        {
            m_PlayerSelection.OnBlockSelected += OnBlockSelected;
            m_PlayerSelection.OnNothingSelected += OnNothingSelected;
            m_Actions = GetComponentsInChildren<Action>();
        }

        /// <summary>
        /// This method is called when no block is selected.
        /// </summary>
        private void OnNothingSelected()
        {
            // Save the last state
            m_LastState = m_State;

            // Set the new state
            m_State.SelectedBlock = false;
        }

        /// <summary>
        /// This method is called when a block is selected.
        /// </summary>
        /// <param name="position">The global block position</param>
        /// <param name="blocktype">The type of the selected block</param>
        /// <param name="face">The block face</param>
        /// <param name="mousebuttonstate">The state of the mouse buttons</param>
        private void OnBlockSelected(Vector3Int position, BlockType blocktype, BlockFace face,
            PlayerSelection.MouseButtonState mousebuttonstate)
        {
            // Save the last state
            m_LastState = m_State;

            // Set the new state
            m_State.SelectedBlock = true;
            m_State.MouseButtonState = mousebuttonstate;
            m_State.SelectedBlockFace = face;
            m_State.SelectedBlockPosition = position;
            m_State.SelectedBlockType = blocktype;
        }

        /// <summary>
        /// This method is called every frame.
        /// </summary>
        private void Update()
        {
            // If there is an active action then the action must be finished before a new action can be performed.
            if (m_CurrentAction != null)
            {
                if (!m_CurrentAction.IsActive(m_State, m_LastState))
                {
                    m_CurrentAction = null;
                    m_ProgressTimer = 0f;
                    m_ProgressBar.SetProgressBarEnabled(false);
                }
                else
                {
                    if (m_CurrentAction is ProgressAction pgAction)
                    {
                        // Update the progress
                        m_ProgressTimer += Time.deltaTime;
                        m_ProgressBar.SetProgressValue(m_ProgressTimer, pgAction.NeededProgressTime);
                        if (m_ProgressTimer >= pgAction.NeededProgressTime)
                        {
                            // The progress is complete
                            m_ProgressBar.SetProgressBarEnabled(false);
                            pgAction.ProgressFinished(m_State);
                        }
                    }
                }
            }

            // Check for a new active action
            if (m_CurrentAction == null)
            {
                foreach (var action in m_Actions)
                {
                    if (action.IsActive(m_State, m_LastState))
                    {
                        if (action is ProgressAction)
                        {
                            m_ProgressBar.SetProgressBarEnabled(true);
                            m_ProgressTimer = 0f;
                        }
                        m_CurrentAction = action;
                        break;
                    }
                }
            }
        }
    }
}