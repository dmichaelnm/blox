using UnityEngine;

namespace Blox.CommonNS
{
    /// <summary>
    /// Contains information about a performance measurement.
    /// </summary>
    public struct PerfomanceInfo
    {
        /// <summary>
        /// The start time of a performance measure.
        /// </summary>
        public float startTime;

        /// <summary>
        /// The frame count when a performance measure starts.
        /// </summary>
        public int frameCount;

        /// <summary>
        /// Returns the duration in milliseconds for this performance measurement.
        /// </summary>
        public float duration => (Time.realtimeSinceStartup - startTime) * 1000f;

        /// <summary>
        /// Returns the number of frames for this performance measurement.
        /// </summary>
        public int frames => Time.frameCount - frameCount;

        /// <summary>
        /// Starts the measurement.
        /// </summary>
        public void StartMeasure()
        {
            startTime = Time.realtimeSinceStartup;
            frameCount = Time.frameCount;
        }

        /// <summary>
        /// Returns a string representation of this struct.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Duration={duration}ms, Frames={frames}";
        }
    }
}