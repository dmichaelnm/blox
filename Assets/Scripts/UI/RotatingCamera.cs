using Blox.EnvironmentNS;
using Blox.PlayerNS;
using UnityEngine;

namespace Blox.UINS
{
    public class RotatingCamera : MonoBehaviour
    {
        public float cameraHeight = 60f;
        public float lookHeight = 20f;
        public float radius = 32f;
        public float rotationSpeed = 1f;

        [SerializeField] private ChunkManager m_ChunkManager;
        [SerializeField] private PlayerController m_PlayerController;

        private float m_Angle;
        private Vector3 m_CameraCenter;
        private Vector3 m_LookCenter;

        private void Awake()
        {
            var center = m_ChunkManager.ChunkSize.Width / 2f;
            m_CameraCenter = new Vector3(center, cameraHeight, center);
            m_LookCenter = new Vector3(center, lookHeight, center);

            m_PlayerController.OnPlayerMoved += playerPosition =>
            {
                var position = playerPosition.CurrentPosition;
                m_CameraCenter = new Vector3(position.x, cameraHeight, position.z);
                m_LookCenter = new Vector3(position.x, lookHeight, position.z);
            };
        }

        private void Update()
        {
            m_Angle += Time.deltaTime * rotationSpeed;
            m_Angle = Mathf.Repeat(m_Angle, 360f);

            var x = m_CameraCenter.x + radius * Mathf.Sin(m_Angle / 180f * Mathf.PI);
            var z = m_CameraCenter.z + radius * Mathf.Cos(m_Angle / 180f * Mathf.PI);
            var position = new Vector3(x, cameraHeight, z);
            transform.position = position;

            var forward = (position - m_LookCenter) * -1f;
            transform.rotation = Quaternion.LookRotation(forward);
        }
    }
}