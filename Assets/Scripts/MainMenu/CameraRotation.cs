using System;
using UnityEngine;

namespace Blox.MainMenu
{
    public class CameraRotation : MonoBehaviour
    {
        public Transform cameraTransform;
        public Vector3 center;
        public float radius;
        public Vector3 lookVector;
        public float rotationSpeed;

        private float m_Angle;

        private void Update()
        {
            m_Angle += Time.deltaTime * rotationSpeed;
            m_Angle = Mathf.Repeat(m_Angle, 360f);

            var x = center.x + radius * Mathf.Sin(m_Angle / 180f * Mathf.PI);
            var z = center.z + radius * Mathf.Cos(m_Angle / 180f * Mathf.PI);

            var position = new Vector3(x, center.y, z);
            var forward = position - lookVector;
            cameraTransform.position = position;
            cameraTransform.rotation = Quaternion.LookRotation(forward * -1f);
        }
    }
}