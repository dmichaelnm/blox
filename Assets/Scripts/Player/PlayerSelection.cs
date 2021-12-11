using System;
using Blox.CommonNS;
using Blox.ConfigurationNS;
using Blox.TerrainNS;
using UnityEngine;
using UnityEngine.EventSystems;
using Math = Blox.CommonNS.Math;

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

        public struct SelectionState
        {
            public MouseButtonState mouseButtonState;
            public Vector3 position;
            public BlockFace face;
            public EntityType entityType;

            public override bool Equals(object obj)
            {
                if (obj is SelectionState state)
                    return mouseButtonState == state.mouseButtonState && face == state.face &&
                           position == state.position;

                return false;
            }

            public override int GetHashCode()
            {
                return (int)mouseButtonState * (int)face * position.GetHashCode();
            }
        }

        private const float Epsilon = 0.001f;

        public float maxSelectionDistance = 5;

        public event Events.ComponentArgsEvent<PlayerSelection, SelectionState> OnBlockSelected;
        public event Events.ComponentEvent<PlayerSelection> OnNoBlockSelected;

        [SerializeField] private Transform m_CameraTransform;
        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private EventSystem m_EventSystem;

        private MeshRenderer m_MeshRenderer;
        private SelectionState m_CurrentSelectionState;
        private SelectionState m_LastSelectionState;
        private Model m_SelectedModel;

        private void Awake()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
        }

        private void Update()
        {
            // Get the current state of the mouse buttons
            m_CurrentSelectionState.mouseButtonState =
                (Input.GetMouseButtonDown(0) ? MouseButtonState.LeftButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonDown(1) ? MouseButtonState.RightButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonDown(2) ? MouseButtonState.MiddleButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonUp(0) ? MouseButtonState.LeftButtonUp : MouseButtonState.None) |
                (Input.GetMouseButtonUp(1) ? MouseButtonState.RightButtonUp : MouseButtonState.None) |
                (Input.GetMouseButtonUp(2) ? MouseButtonState.MiddleButtonUp : MouseButtonState.None);

            // If the mouse hits a component in UI canvas then ignore it here
            if (m_EventSystem.IsPointerOverGameObject())
                m_CurrentSelectionState.mouseButtonState = MouseButtonState.None;

            var show = false;

            // Calculates which block face is selected
            var ray = new Ray(m_CameraTransform.position, m_CameraTransform.forward);
            if (Physics.Raycast(ray, out var hit, maxSelectionDistance))
            {
                Log.Debug(this, $"normal = {hit.normal}");

                var model = hit.transform.gameObject.GetComponent<Model>();
                if (model == null)
                {
                    if (m_SelectedModel != null)
                        m_SelectedModel.Highlight(false);
                    m_SelectedModel = null;

                    show = true;

                    var x = hit.point.x + Epsilon;
                    var y = hit.point.y + Epsilon;
                    var z = hit.point.z + Epsilon;

                    var bx = Math.FloorToInt(x) + Epsilon;
                    var by = Math.FloorToInt(y) + Epsilon;
                    var bz = Math.FloorToInt(z) + Epsilon;

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
                            m_CurrentSelectionState.position = new Vector3(Math.FloorToInt(x) + 0.5f,
                                Math.FloorToInt(y) - 0.5f,
                                Math.FloorToInt(z) + 0.5f);
                            m_CurrentSelectionState.face = BlockFace.Top;
                        }
                        else if (ray.direction.y > 0f && dy < Epsilon)
                        {
                            // Bottom
                            m_CurrentSelectionState.position = new Vector3(Math.FloorToInt(x) + 0.5f,
                                Math.FloorToInt(y) + 0.5f,
                                Math.FloorToInt(z) + 0.5f);
                            m_CurrentSelectionState.face = BlockFace.Bottom;
                        }
                        else if (ray.direction.z > 0f && dz < Epsilon)
                        {
                            // Front
                            m_CurrentSelectionState.position = new Vector3(Math.FloorToInt(x) + 0.5f,
                                Math.FloorToInt(y) + 0.5f,
                                Math.FloorToInt(z) + 0.5f);
                            m_CurrentSelectionState.face = BlockFace.Front;
                        }
                        else if (ray.direction.z < 0f && dz < Epsilon)
                        {
                            // Back
                            m_CurrentSelectionState.position = new Vector3(Math.FloorToInt(x) + 0.5f,
                                Math.FloorToInt(y) + 0.5f,
                                Math.FloorToInt(z) - 0.5f);
                            m_CurrentSelectionState.face = BlockFace.Back;
                        }
                        else if (ray.direction.x > 0f && dx < Epsilon)
                        {
                            // Left
                            m_CurrentSelectionState.position = new Vector3(Math.FloorToInt(x) + 0.5f,
                                Math.FloorToInt(y) + 0.5f,
                                Math.FloorToInt(z) + 0.5f);
                            m_CurrentSelectionState.face = BlockFace.Left;
                        }
                        else if (ray.direction.x < 0f && dx < Epsilon)
                        {
                            // Right
                            m_CurrentSelectionState.position = new Vector3(Math.FloorToInt(x) - 0.5f,
                                Math.FloorToInt(y) + 0.5f,
                                Math.FloorToInt(z) + 0.5f);
                            m_CurrentSelectionState.face = BlockFace.Right;
                        }
                    }
                    else
                        show = false;
                }
                else
                {
                    Log.Debug(this, $"Hitting {model}");
                    if (!model.Equals(m_SelectedModel))
                    {
                        if (m_SelectedModel != null)
                            m_SelectedModel.Highlight(false);
                        
                        m_SelectedModel = model;
                        m_SelectedModel.Highlight(true);
                    }

                    show = true;
                    m_CurrentSelectionState.position = model.transform.position;
                }
            }

            if (show)
            {
                // show the selection block
                transform.position = m_CurrentSelectionState.position;

                // check, if new event must be send
                if (!m_CurrentSelectionState.Equals(m_LastSelectionState))
                {
                    var position = m_CurrentSelectionState.position.ToVector3Int();
                    m_CurrentSelectionState.entityType = m_ChunkManager.GetEntity<EntityType>(position);
                    var rotation = m_ChunkManager.GetRotation(position);
                    Log.Debug(this, $"Rotation: {rotation}");

                    if (m_CurrentSelectionState.entityType is IBlockType)
                    {
                        m_MeshRenderer.sharedMaterial.SetFloat("_Face", (int)m_CurrentSelectionState.face);
                        m_MeshRenderer.enabled = true;
                    }
                    else
                    {
                        m_MeshRenderer.enabled = false;
                    }

                    OnBlockSelected?.Invoke(this, m_CurrentSelectionState);
                    m_LastSelectionState = m_CurrentSelectionState;
                }
            }
            else
            {
                // hide the selection block
                m_MeshRenderer.enabled = false;
                OnNoBlockSelected?.Invoke(this);
            }
        }
    }
}