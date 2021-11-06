using System;
using UnityEngine;

namespace Blox.EnvironmentNS.GeneratorNS
{
    /// <summary>
    /// This struct contains parameter used by a perlin noise function.
    /// </summary>
    [Serializable]
    public struct NoiseParams
    {
        /// <summary>
        /// A seed for the perlin noise calculation.
        /// </summary>
        public Vector2 seed;
        
        /// <summary>
        /// A scale number used to calculate the perlin noise coordinates.
        /// </summary>
        public int scale;

        /// <summary>
        /// The number of ocatves used for the perlin noise calculation.
        /// </summary>
        public int octaves;

        /// <summary>
        /// The base frequency used for the perlin noise calculation.
        /// </summary>
        public float frequency;

        /// <summary>
        /// The redistribution used for the perlin noise calculation.
        /// </summary>
        public float redistribution;

        /// <summary>
        /// The scale used to compensate the redistribution.
        /// </summary>
        [Range(0f, 1f)] public float redistributionScale;

        /// <summary>
        /// The amplitude used for the perlin noise calculation.
        /// </summary>
        public float amplitude;
    }
}