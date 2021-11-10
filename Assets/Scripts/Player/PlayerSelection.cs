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
    public class PlayerSelection : MonoBehaviour
    {
        [Flags]
        public enum MouseButtonState
        {
            None = 0,
            LeftButtonDown = 1,
            RightButtonDown = 2,
            MiddleButtonDown = 4,
            LeftButtonUp = 8,
            RightButtonUp = 16,
            MiddleButtonUp = 32
        }

        internal struct State
        {
            public MouseButtonState MouseButtonState;
            public BlockFace Face;
            public Vector3 Position;

            public override bool Equals(object obj)
            {
                if (obj is State state)
                    return MouseButtonState == state.MouseButtonState && Face == state.Face &&
                           Position == state.Position;

                return false;
            }

            [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
            public override int GetHashCode()
            {
                return (int)MouseButtonState * (int)Face * Position.GetHashCode();
            }
        }

        private const float Epsilon = 0.001f;

        public float MaxSelectionDistance = 5;

        public event Events.PlayerSelectionEvent OnBlockSelected;
        public event Events.EmptyEvent OnNothingSelected;
        public event Events.ComponentEvent<PlayerSelection> OnPlayerSelectionDestroyed;

        [SerializeField] private Transform m_CameraTransform;
        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private EventSystem m_EventSystem;

        private MeshRenderer m_MeshRenderer;
        private State m_CurrentState;
        private State m_LastState;

        private void Awake()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnDestroy()
        {
            OnPlayerSelectionDestroyed?.Invoke(this);
        }

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
            if (m_EventSystem.IsPointerOverGameObject())
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
                        MathUtilities.Floor(m_CurrentState.Position.x),
                        MathUtilities.Floor(m_CurrentState.Position.y),
                        MathUtilities.Floor(m_CurrentState.Position.z));
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