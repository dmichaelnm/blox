using UnityEngine;

namespace Blox.Utility
{
    public static class MathUtility
    {
        public static int Floor(float value, float epsilon = 0.0001f)
        {
            return Mathf.FloorToInt(value + epsilon);
        }
    }
}