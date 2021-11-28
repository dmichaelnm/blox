using Blox.PlayerNS;
using UnityEngine;

namespace Blox.UserInterfaceNS
{
    public class RotatingCamera : MonoBehaviour
    {
        public Vector3 center;
        public float radius;
        public float speed;
        public float lookHeight;

        private float m_Angle;

        public void SetCenter(PlayerPosition position)
        {
            center = new Vector3(position.currentPosition.x, center.y, position.currentPosition.z);
        }
        
        private void Update()
        {
            m_Angle += Time.deltaTime * speed;
            m_Angle = Mathf.Repeat(m_Angle, 360f);

            var x = center.x + radius * Mathf.Sin(m_Angle / 180f * Mathf.PI);
            var z = center.z + radius * Mathf.Cos(m_Angle / 180f * Mathf.PI);
            var cameraPos = new Vector3(x, center.y, z);
            transform.position = cameraPos;

            var lookPos = new Vector3(center.x, lookHeight, center.z);
            var lookDirection = lookPos - cameraPos;
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}