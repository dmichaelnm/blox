using Blox.PlayerNS;
using UnityEngine;

namespace Blox.EnvironmentNS
{
    /// <summary>
    /// This component manages the minimap.
    /// </summary>
    public class MiniMapCamera : MonoBehaviour
    {
        /// <summary>
        /// The shortcut to toggle the minimap between static and rotating.
        /// </summary>
        public KeyCode shortcut = KeyCode.F2; 
        
        /// <summary>
        /// The minimap camera component.
        /// </summary>
        [SerializeField] private Camera m_Camera;

        /// <summary>
        /// The player controller component.
        /// </summary>
        [SerializeField] private PlayerController m_PlayerController;

        /// <summary>
        /// The camera transform object.
        /// </summary>
        private Transform m_CameraTransform;

        /// <summary>
        /// Flag for toggling between static and rotating minimap.
        /// </summary>
        private bool m_StaticMinimap = true;

        /// <summary>
        /// This method is called when this component is created. 
        /// </summary>
        private void Awake()
        {
            m_CameraTransform = m_Camera.transform;
            m_PlayerController.OnPlayerMoved += OnPlayerMoved;
            m_Camera.enabled = false;
        }

        /// <summary>
        /// This method is called when the position of the player has changed.
        /// </summary>
        /// <param name="position">The players position</param>
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

        /// <summary>
        /// This method is called every frame.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyUp(shortcut))
                m_StaticMinimap = !m_StaticMinimap;
        }
    }
}