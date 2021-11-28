using Blox.ConfigurationNS;
using UnityEngine;

namespace Blox.CommonNS
{
    public static class Math
    {
        public static int FloorToInt(float value, float epsilon = 0.0001f)
        {
            return Mathf.FloorToInt(value + epsilon);
        }

        public static Vector3Int ToVector3Int(this Vector3 value, float epsilon = 0.0001f)
        {
            var x = FloorToInt(value.x, epsilon);
            var y = FloorToInt(value.y, epsilon);
            var z = FloorToInt(value.z, epsilon);
            return new Vector3Int(x, y, z);
        }

        public static Vector3Int Neighbour(this Vector3Int vector, BlockFace face)
        {
            return face switch
            {
                BlockFace.Top => new Vector3Int(vector.x, vector.y + 1, vector.z),
                BlockFace.Bottom => new Vector3Int(vector.x, vector.y - 1, vector.z),
                BlockFace.Front => new Vector3Int(vector.x, vector.y, vector.z - 1),
                BlockFace.Back => new Vector3Int(vector.x, vector.y, vector.z + 1),
                BlockFace.Left => new Vector3Int(vector.x - 1, vector.y, vector.z),
                BlockFace.Right => new Vector3Int(vector.x + 1, vector.y, vector.z),
                _ => vector
            };
        }
    }
}