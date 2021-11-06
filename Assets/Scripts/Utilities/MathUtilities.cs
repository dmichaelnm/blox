using Blox.ConfigurationNS;
using UnityEngine;

namespace Blox.UtilitiesNS
{
    /// <summary>
    /// This utility class contains several methods for mathematical calculations.
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// Returns the integer value for the given float value.
        /// </summary>
        /// <param name="value">A float value</param>
        /// <param name="epsilon">An epsilon value to avoid invalid values because of incorrect float values</param>
        /// <returns>The integer value</returns>
        public static int Floor(float value, float epsilon = 0.0001f)
        {
            return Mathf.FloorToInt(value + epsilon);
        }

        /// <summary>
        /// Returns a float value with a defined number of decimal places.
        /// </summary>
        /// <param name="value">A float value</param>
        /// <param name="decimals">Number of decimal places</param>
        /// <param name="epsilon">An epsilon value to avoid invalid values because of incorrect float values</param>
        /// <returns>The float value with the number of decimal places</returns>
        public static float Floor(float value, int decimals, float epsilon = 0.0001f)
        {
            var factor = Mathf.Pow(10, decimals);
            return Floor(value * factor, epsilon) / factor;
        }

        /// <summary>
        /// Returns the neighbour position relative to the given global position and the given block face.
        /// </summary>
        /// <param name="globalPosition">A global position vector</param>
        /// <param name="face">A block face</param>
        /// <returns>The neighbour global position vector</returns>
        public static Vector3Int Neighbour(Vector3Int globalPosition, BlockFace face)
        {
            if (face == BlockFace.Top)
                globalPosition.y++;
            else if (face == BlockFace.Bottom)
                globalPosition.y--;
            else if (face == BlockFace.Front)
                globalPosition.z--;
            else if (face == BlockFace.Back)
                globalPosition.z++;
            else if (face == BlockFace.Left)
                globalPosition.x--;
            else if (face == BlockFace.Right)
                globalPosition.x++;

            return globalPosition;
        }
    }
}