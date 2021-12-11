using UnityEngine;

namespace Blox.CommonNS
{
    public static class Format
    {
        public static string ToTimeStr(float time)
        {
            var minutes = Mathf.FloorToInt(time / 60);
            var seconds = Mathf.FloorToInt(time - minutes * 60);
            return $"{minutes}:{seconds:00}";
        }
    }
}