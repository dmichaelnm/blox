using System;
using System.Diagnostics.CodeAnalysis;
using Blox.ConfigurationNS;
using Blox.EnvironmentNS;
using Blox.GameNS;
using Blox.UtilitiesNS;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Blox.PlayerNS
{
    /// <summary>
    /// This component controls the selection of blocks by the player.
    /// </summary>
    public class PlayerSelection : MonoBehaviour
    {
        /// <summary>
        /// This enumeration contains all supported mouse button states.
        /// </summary>
        [Flags]
        public enum MouseButtonState
        {
            /// <summary>
            /// No mouse button event.
            /// </summary>
            None = 0,

            /// <summary>
            /// The left mouse button is pressed down.
            /// </summary>
            LeftButtonDown = 1,

            /// <summary>
            /// The right mouse button is pressed down.
            /// </summary>
            RightButtonDown = 2,

            /// <summary>
            /// The middle mouse button is pressed down.
            /// </summary>
            MiddleButtonDown = 4,

            /// <summary>
            /// The left mouse button is released.
            /// </summary>
            LeftButtonUp = 8,

            /// <summary>
            /// The right mouse button is released.
            /// </summary>
            RightButtonUp = 16,

            /// <summary>
            /// The midlle mouse button is released.
            /// </summary>
            MiddleButtonUp = 32
        }

        /// <summary>
        /// The state of this component.
        /// </summary>
        internal struct State
        {
            /// <summary>
            /// The current mouse button state.
            /// </summary>
            public MouseButtonState MouseButtonState;

            /// <summary>
            /// The selected block face.
            /// </summary>
            public BlockFace Face;

            /// <summary>
            /// The position of the selection block
            /// </summary>
            public Vector3 Position;

            /// <summary>
            /// Compares this state to an other object. If the other object is also a state then all three properties
            /// must be the same to be considered as equal.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public override bool Equals(object obj)
            {
                if (obj is State state)
                    return MouseButtonState == state.MouseButtonState && Face == state.Face &&
                           Position == state.Position;

                return false;
            }

            /// <summary>
            /// Returns a hash code for this struct.
            /// </summary>
            /// <returns>Hash code</returns>
            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode()
            {
                return (int)MouseButtonState * (int)Face * Position.GetHashCode();
            }
        }

        /// <summary>
        /// A constant for an error margin value used when detecting the selected block face.
        /// </summary>
        private const float Epsilon = 0.001f;

        /// <summary>
        /// The maximum distance the player can select blocks from his position.
        /// </summary>
        public float MaxSelectionDistance = 5;

        /// <summary>
        /// This event is triggered when a block is selected.
        /// </summary>
        public event Events.PlayerSelectionEvent OnBlockSelected;

        /// <summary>
        /// This event is triggered when nothing is selected.
        /// </summary>
        public event Events.EmptyEvent OnNothingSelected;
        
        /// <summary>
        /// This event is triggered when the player selection is about to be destroyed.
        /// </summary>
        public event Events.ComponentEvent<PlayerSelection> OnPlayerSelectionDestroyed;

        /// <summary>
        /// The camera object.
        /// </summary>
        [SerializeField] private Transform m_CameraTransform;

        /// <summary>
        /// The chunk manager component.
        /// </summary>
        [SerializeField] private ChunkManager m_ChunkManager;

        /// <summary>
        /// The mesh renderer of the selection block.
        /// </summary>
        private MeshRenderer m_MeshRenderer;

        /// <summary>
        /// The current state.
        /// </summary>
        private State m_CurrentState;

        /// <summary>
        /// The last state.
        /// </summary>
        private State m_LastState;

        /// <summary>
        /// This method is called when this component is created.
        /// </summary>
        private void Awake()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }

        /// <summary>
        /// This method is called before the player selection is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            OnPlayerSelectionDestroyed?.Invoke(this);
        }

        /// <summary>
        /// This method is called every frame
        /// </summary>
        private void Update()
        {
            // Get the current state of the mouse buttons
            m_CurrentState.MouseButtonState =
                (Input.GetMouseButtonDown(0) ? MouseButtonState.LeftButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonDown(1) ? MouseButtonState.RightButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonDown(2) ? MouseButtonState.MiddleButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonUp(0) ? MouseButtonState.LeftButtonUp : MouseButtonState.None) |
                (Input.GetMouseButtonUp(1) ? MouseButtonState.RightButtonUp : MouseButtonState.None) |
                (Input.GetMouseButtonUp(2) ? MouseButtonState.MiddleButtonUp : MouseButtonState.None);

            // If the mouse hits a component in UI canvas then ignore it here
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                m_CurrentState.MouseButtonState = MouseButtonState.None;

            var show = false;

            // Calculates which block face is selected
            var ray = new Ray(m_CameraTransform.position, m_CameraTransform.forward);
            if (Physics.Raycast(ray, out var hit, MaxSelectionDistance))
            {
                show = true;

                var x = hit.point.x + Epsilon;
                var y = hit.point.y + Epsilon;
                var z = hit.point.z + Epsilon;

                var bx = MathUtilities.Floor(x) + Epsilon;
                var by = MathUtilities.Floor(y) + Epsilon;
                var bz = MathUtilities.Floor(z) + Epsilon;

                var dx = Mathf.Abs(x - bx);
                var dy = Mathf.Abs(y - by);
                var dz = Mathf.Abs(z - bz);

                var cnt = 0;
                if (dx < Epsilon) cnt++;
                if (dy < Epsilon) cnt++;
                if (dz < Epsilon) cnt++;

                if (cnt <= 1)
                {
                    if (ray.direction.y < 0f && dy < Epsilon)
                    {
                        // Top
                        m_CurrentState.Position = new Vector3(MathUtilities.Floor(x) + 0.5f,
                            MathUtilities.Floor(y) - 0.5f,
                            MathUtilities.Floor(z) + 0.5f);
                        m_CurrentState.Face = BlockFace.Top;
                    }
                    else if (ray.direction.y > 0f && dy < Epsilon)
                    {
                        // Bottom
                        m_CurrentState.Position = new Vector3(MathUtilities.Floor(x) + 0.5f,
                            MathUtilities.Floor(y) + 0.5f,
                            MathUtilities.Floor(z) + 0.5f);
                        m_CurrentState.Face = BlockFace.Bottom;
                    }
                    else if (ray.direction.z > 0f && dz < Epsilon)
                    {
                        // Front
                        m_CurrentState.Position = new Vector3(MathUtilities.Floor(x) + 0.5f,
                            MathUtilities.Floor(y) + 0.5f,
                            MathUtilities.Floor(z) + 0.5f);
                        m_CurrentState.Face = BlockFace.Front;
                    }
                    else if (ray.direction.z < 0f && dz < Epsilon)
                    {
                        // Back
                        m_CurrentState.Position = new Vector3(MathUtilities.Floor(x) + 0.5f,
                            MathUtilities.Floor(y) + 0.5f,
                            MathUtilities.Floor(z) - 0.5f);
                        m_CurrentState.Face = BlockFace.Back;
                    }
                    else if (ray.direction.x > 0f && dx < Epsilon)
                    {
                        // Left
                        m_CurrentState.Position = new Vector3(MathUtilities.Floor(x) + 0.5f,
                            MathUtilities.Floor(y) + 0.5f,
                            MathUtilities.Floor(z) + 0.5f);
                        m_CurrentState.Face = BlockFace.Left;
                    }
                    else if (ray.direction.x < 0f && dx < Epsilon)
                    {
                        // Right
                        m_CurrentState.Position = new Vector3(MathUtilities.Floor(x) - 0.5f,
                            MathUtilities.Floor(y) + 0.5f,
                            MathUtilities.Floor(z) + 0.5f);
                        m_CurrentState.Face = BlockFace.Right;
                    }
                }
                else
                    show = false;
            }

            if (show)
            {
                // show the selection block
                transform.position = m_CurrentState.Position;
                m_MeshRenderer.sharedMaterial.SetFloat("_Face", (int)m_CurrentState.Face);
                m_MeshRenderer.enabled = true;

                // check, if new event must be send
                if (!m_CurrentState.Equals(m_LastState))
                {
                    var position = new Vector3Int(
                        (int)m_CurrentState.Position.x,
                        (int)m_CurrentState.Position.y,
                        (int)m_CurrentState.Position.z);
                    var chunkPosition = ChunkPosition.FromGlobalPosition(m_ChunkManager.ChunkSize, position);
                    var chunkData = m_ChunkManager[chunkPosition];
                    var localPosition = chunkPosition.ToLocalPosition(m_ChunkManager.ChunkSize, position);
                    var blockType = chunkData[localPosition];

                    OnBlockSelected?.Invoke(position, blockType, m_CurrentState.Face, m_CurrentState.MouseButtonState);
                    m_LastState = m_CurrentState;
                }
            }
            else
            {
                // hide the selection block
                m_MeshRenderer.enabled = false;
                OnNothingSelected?.Invoke();
            }
        }
    }
}