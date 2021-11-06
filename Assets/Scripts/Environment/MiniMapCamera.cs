using Blox.PlayerNS;
using UnityEngine;

namespace Blox.EnvironmentNS
{
    public class MiniMapCamera : MonoBehaviour
    {
        public KeyCode shortcut = KeyCode.F2; 
        
        [SerializeField] private Camera m_Camera;

        [SerializeField] private PlayerController m_PlayerController;

        private Transform m_CameraTransform;

        private bool m_StaticMinimap = true;

        private void Awake()
        {
            m_CameraTransform = m_Camera.transform;
            m_PlayerController.OnPlayerMoved += OnPlayerMoved;
            m_Camera.enabled = false;
        }

        private void OnPlayerMoved(PlayerPosition position)
        {
            var camPos = new Vector3(position.CurrentPosition.x, 100, position.CurrentPosition.z);
            m_CameraTransform.position = camPos;
            m_Camera.enabled = true;

            if (m_StaticMinimap)
            {
                m_CameraTransform.rotation = Quaternion.Euler(90f, 0f, 0f);
            }
            else
            {
                var forward = position.CameraForward;
                forward.y = 0;
                forward = forward.normalized;
                var angle = Vector3.SignedAngle(Vector3.forward, forward, Vector3.up) * -1f;
                //Debug.Log($"Forward: {forward}, Angle: {angle}");
                m_CameraTransform.rotation = Quaternion.Euler(90f, 0f, angle);
            }
        }

        private void Update()
        {
            if (Input.GetKeyUp(shortcut))
                m_StaticMinimap = !m_StaticMinimap;
        }
    }
}