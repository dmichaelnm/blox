using System;

namespace Blox.EnvironmentNS.GeneratorNS
{
    /// <summary>
    /// This struct contains all parameters for generating a chunk.
    /// </summary>
    [Serializable]
    public struct GeneratorParams
    {
        /// <summary>
        /// Generator parameter for the terrain heights.
        /// </summary>
        [Serializable]
        public struct TerrainParams
        {
            /// <summary>
            /// Noise parameter for generating the terrain heights.
            /// </summary>
            public NoiseParams Noise;
        }

        /// <summary>
        /// Generator parameter for the water depths.
        /// </summary>
        [Serializable]
        public struct WaterParams
        {
            /// <summary>
            /// Noise parameter for generating the water depths.
            /// </summary>
            public NoiseParams Noise;

            /// <summary>
            /// An offset for defining the water base relative to the baseline.
            /// </summary>
            public int waterOffset;
        }

        /// <summary>
        /// Generator parameter for the stone blocks.
        /// </summary>
        [Serializable]
        public struct StoneParams
        {
            /// <summary>
            /// Noise parameter for generating stone blocks.
            /// </summary>
            public NoiseParams Noise;

            /// <summary>
            /// The stone level relative to the base line.
            /// </summary>
            public int RelativeLevel;

            /// <summary>
            /// The scattering at the relative stone level.
            /// </summary>
            public int Scattering;

            /// <summary>
            /// The snow border level relative to the stone level.
            /// </summary>
            public int SnowBorderLevel;

            /// <summary>
            /// The scattering at the snow border level.
            /// </summary>
            public int SnowBorderScattering;
        }

        /// <summary>
        /// Generator parameter for trees.
        /// </summary>
        [Serializable]
        public struct TreeParams
        {
            /// <summary>
            /// The maximal number of trees in a chunk.
            /// </summary>
            public int MaximalTreeCount;

            /// <summary>
            /// Noise parameter for generating trees.
            /// </summary>
            public NoiseParams Noise;

            /// <summary>
            /// A threshold for generating a tree at a specific position regarding the result of a perlin noise function.
            /// </summary>
            public float Threshold;

            /// <summary>
            /// This minimum height of a tree.
            /// </summary>
            public int MinimumHeight;

            /// <summary>
            /// This maximum height of a tree.
            /// </summary>
            public int MaximumHeight;
        }
        
        /// <summary>
        /// Random seed.
        /// </summary>
        public int randomSeed;
        
        /// <summary>
        /// The baseline for the surface.
        /// </summary>
        public int baseLine;

        /// <summary>
        /// Generator parameter for the terrain heights.
        /// </summary>
        public TerrainParams Terrain;

        /// <summary>
        /// Generator parameter for the water depths.
        /// </summary>
        public WaterParams Water;

        /// <summary>
        /// Generator parameter for stone blocks.
        /// </summary>
        public StoneParams Stone;

        /// <summary>
        /// Generator parameter for trees.
        /// </summary>
        public TreeParams Tree;
    }
}