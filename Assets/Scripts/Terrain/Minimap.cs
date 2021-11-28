using Blox.GameNS;
using UnityEngine;
using UnityEngine.UI;

namespace Blox.TerrainNS
{
    public class Minimap : MonoBehaviour
    {
        public enum Anchor
        {
            TopLeft,
            Top,
            TopRight,
            Left,
            Center,
            Right,
            BottomLeft,
            Bottom,
            BottomRight
        }

        internal enum State
        {
            None,
            Static,
            Rotation
        }

        public Anchor anchorPoint;
        public Vector2Int anchorOffset;
        public Vector2Int size;

        [SerializeField] private GameManager m_GameManager;
        [SerializeField] private Camera m_Camera;
        [SerializeField] private Image m_FrameImage;

        private State m_State;

        private void Awake()
        {
            m_Camera.enabled = false;
            m_FrameImage.enabled = false;
            m_State = State.None;
        }

        private void Update()
        {
            if (Input.GetKeyDown(m_GameManager.toggleMinimap))
            {
                var state = (int)m_State;
                state = (state + 1) % 3;
                m_State = (State)state;
            }

            if (m_GameManager.initialized && m_State != State.None)
            {
                m_Camera.rect = CalculateViewRect();
                m_Camera.enabled = true;
                m_FrameImage.enabled = true;

                var playerPosition = m_GameManager.playerPosition;

                // camera position
                var x = playerPosition.currentPosition.x;
                var y = m_GameManager.chunkSize.height;
                var z = playerPosition.currentPosition.z;
                transform.position = new Vector3(x, y, z);

                if (m_State == State.Static)
                {
                    // static camera
                    transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                }
                else if (m_State == State.Rotation)
                {
                    // camera rotating
                    var forward = playerPosition.cameraForward;
                    forward.y = 0;
                    var angle = Vector3.SignedAngle(Vector3.forward, forward.normalized, Vector3.up) * -1f;
                    transform.rotation = Quaternion.Euler(90f, 0f, angle);
                }
            }
            else
            {
                m_Camera.enabled = false;
                m_FrameImage.enabled = false;
            }
        }

        private Rect CalculateViewRect()
        {
            var ix = 0;
            var iy = 0;

            switch (anchorPoint)
            {
                case Anchor.TopLeft:
                case Anchor.Left:
                case Anchor.BottomLeft:
                    ix = anchorOffset.x;
                    break;
                case Anchor.Top:
                case Anchor.Center:
                case Anchor.Bottom:
                    ix = Screen.width / 2 - size.x / 2 + anchorOffset.x;
                    break;
                case Anchor.TopRight:
                case Anchor.Right:
                case Anchor.BottomRight:
                    ix = Screen.width + anchorOffset.x;
                    break;
            }

            switch (anchorPoint)
            {
                case Anchor.BottomLeft:
                case Anchor.Bottom:
                case Anchor.BottomRight:
                    iy = anchorOffset.y;
                    break;
                case Anchor.Left:
                case Anchor.Center:
                case Anchor.Right:
                    iy = Screen.height / 2 - size.y / 2 + anchorOffset.y;
                    break;
                case Anchor.TopLeft:
                case Anchor.Top:
                case Anchor.TopRight:
                    iy = Screen.height + anchorOffset.y;
                    break;
            }

            var x = ix / (float)Screen.width;
            var y = iy / (float)Screen.height;
            var w = size.x / (float)Screen.width;
            var h = size.y / (float)Screen.height;

            return new Rect(x, y, w, h);
        }
    }
}