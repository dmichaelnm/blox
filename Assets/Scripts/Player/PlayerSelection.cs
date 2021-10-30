using System;
using Blox.Environment;
using Blox.Environment.Config;
using Blox.Utility;
using Common;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Blox.Player
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

        private const float epsilon = 0.001f;

        public Camera cam;
        public float maxDistance = 5;
        public bool mouseSelection;

        public delegate void BlockSelectionEvent(Position position, BlockFace face,
            MouseButtonState mouseButtonState);
        public event BlockSelectionEvent onBlockSelection;

        private ChunkManager m_ChunkManager;
        private MeshRenderer m_MeshRenderer;
        private MouseButtonState m_MouseButtonState;
        private MouseButtonState m_LastMouseButtonState;
        private bool m_MouseButtonStateChanged;
        private BlockFace m_LastFace;
        private Vector3 m_LastPosition;
        private Transform m_CameraTransform;

        private void Awake()
        {
            m_ChunkManager = GameObject.Find("Chunk Manager").GetComponent<ChunkManager>();
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_CameraTransform = cam.transform;
        }

        private void Update()
        {
            m_MouseButtonState =
                (Input.GetMouseButtonDown(0) ? MouseButtonState.LeftButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonDown(1) ? MouseButtonState.RightButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonDown(2) ? MouseButtonState.MiddleButtonDown : MouseButtonState.None) |
                (Input.GetMouseButtonUp(0) ? MouseButtonState.LeftButtonUp : MouseButtonState.None) |
                (Input.GetMouseButtonUp(1) ? MouseButtonState.RightButtonUp : MouseButtonState.None) |
                (Input.GetMouseButtonUp(2) ? MouseButtonState.MiddleButtonUp : MouseButtonState.None);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                m_MouseButtonState = MouseButtonState.None;

            if (m_MouseButtonState != m_LastMouseButtonState)
            {
                m_MouseButtonStateChanged = true;
                m_LastMouseButtonState = m_MouseButtonState;
            }

            var show = false;
            var face = BlockFace.Top;
            var position = Vector3.zero;

            var ray = mouseSelection
                ? cam.ScreenPointToRay(Input.mousePosition)
                : new Ray(m_CameraTransform.position, m_CameraTransform.forward);

            if (Physics.Raycast(ray, out var hit, maxDistance))
            {
                show = true;

                var x = hit.point.x;
                var y = hit.point.y;
                var z = hit.point.z;

                var bx = MathUtility.Floor(x) + epsilon;
                var by = MathUtility.Floor(y) + epsilon;
                var bz = MathUtility.Floor(z) + epsilon;

                var cnt = 0;
                if (x < bx) cnt++;
                if (y < by) cnt++;
                if (z < bz) cnt++;

                if (cnt <= 1)
                {
                    if (ray.direction.y < 0f && y < by)
                    {
                        // Top
                        position = new Vector3(MathUtility.Floor(x) + 0.5f, MathUtility.Floor(y) - 0.5f,
                            MathUtility.Floor(z) + 0.5f);
                        face = BlockFace.Top;
                    }
                    else if (ray.direction.y > 0f && y < by)
                    {
                        // Bottom
                        position = new Vector3(MathUtility.Floor(x) + 0.5f, MathUtility.Floor(y) + 0.5f,
                            MathUtility.Floor(z) + 0.5f);
                        face = BlockFace.Bottom;
                    }
                    else if (ray.direction.z > 0f && z < bz)
                    {
                        // Front
                        position = new Vector3(MathUtility.Floor(x) + 0.5f, MathUtility.Floor(y) + 0.5f,
                            MathUtility.Floor(z) + 0.5f);
                        face = BlockFace.Front;
                    }
                    else if (ray.direction.z < 0f && z < bz)
                    {
                        // Back
                        position = new Vector3(MathUtility.Floor(x) + 0.5f, MathUtility.Floor(y) + 0.5f,
                            MathUtility.Floor(z) - 0.5f);
                        face = BlockFace.Back;
                    }
                    else if (ray.direction.x > 0f && x < bx)
                    {
                        // Left
                        position = new Vector3(MathUtility.Floor(x) + 0.5f, MathUtility.Floor(y) + 0.5f,
                            MathUtility.Floor(z) + 0.5f);
                        face = BlockFace.Left;
                    }
                    else if (ray.direction.x < 0f && x < bx)
                    {
                        // Right
                        position = new Vector3(MathUtility.Floor(x) - 0.5f, MathUtility.Floor(y) + 0.5f,
                            MathUtility.Floor(z) + 0.5f);
                        face = BlockFace.Right;
                    }
                }
                else
                    show = false;
            }

            if (show)
            {
                // show the selection block
                transform.position = position;
                m_MeshRenderer.sharedMaterial.SetFloat("_Face", (int)face);
                m_MeshRenderer.enabled = true;

                // check, if new event must be send
                if (position != m_LastPosition || face != m_LastFace || m_MouseButtonStateChanged)
                {
                    var blockPosition = new Position(m_ChunkManager.chunkSize, position);
                    onBlockSelection?.Invoke(blockPosition, face, m_MouseButtonState);

                    m_LastPosition = position;
                    m_LastFace = face;
                    m_MouseButtonStateChanged = false;
                }
            }
            else
            {
                // hide the selection block
                m_MeshRenderer.enabled = false;
            }
        }
    }
}