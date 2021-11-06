using Blox.ConfigurationNS;
using Blox.EnvironmentNS.GeneratorNS;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

namespace Blox.EnvironmentNS.JobsNS
{
    /// <summary>
    /// This job generates the chunk data information.
    /// </summary>
    public struct GenerateChunkJob : IChunkDataProvider
    {
        /// <summary>
        /// The chunk position.
        /// </summary>
        [ReadOnly] private ChunkPosition m_ChunkPosition;

        /// <summary>
        /// The size of the chunk.
        /// </summary>
        [ReadOnly] private ChunkSize m_ChunkSize;

        /// <summary>
        /// Generator parameters.
        /// </summary>
        [ReadOnly] private GeneratorParams m_GeneratorParams;

        /// <summary>
        /// The array containing the loaded information.
        /// </summary>
        private NativeArray<int> m_Blocks;

        /// <summary>
        /// Initializes this job.
        /// </summary>
        /// <param name="chunkSize">The size of the chunk</param>
        /// <param name="chunkPosition">The position of the chunk</param>
        /// <param name="generatorParams">The generator parameters</param>
        public void Initialize(ChunkSize chunkSize, ChunkPosition chunkPosition, GeneratorParams generatorParams)
        {
            m_ChunkSize = chunkSize;
            m_ChunkPosition = chunkPosition;
            m_GeneratorParams = generatorParams;
            m_Blocks = new NativeArray<int>(m_ChunkSize.Size, Allocator.Persistent);
        }

        /// <summary>
        /// Executes the generation job.
        /// </summary>
        public void Execute()
        {
            var config = Configuration.GetInstance();
            var random = new Random(m_GeneratorParams.randomSeed);
            var baseLevel = m_GeneratorParams.baseLine;
            var waterLevel = baseLevel + m_GeneratorParams.Water.waterOffset;

            for (var z = 0; z < m_ChunkSize.Width; z++)
            {
                for (var x = 0; x < m_ChunkSize.Width; x++)
                {
                    var terrainY = CalculateNoise(x, z, m_GeneratorParams.Terrain.Noise);
                    var waterY = CalculateNoise(x, z, m_GeneratorParams.Water.Noise);
                    var height = Mathf.FloorToInt(terrainY - waterY) + baseLevel;

                    var stoneY = CalculateNoise(x, z, m_GeneratorParams.Stone.Noise);
                    var stoneLevelLower = Mathf.Min(Mathf.FloorToInt(stoneY * baseLevel), height);
                    var stoneLevelUpper = baseLevel + m_GeneratorParams.Stone.RelativeLevel +
                                          random.Next(-m_GeneratorParams.Stone.Scattering,
                                              m_GeneratorParams.Stone.Scattering);
                    var snowLevel = baseLevel + m_GeneratorParams.Stone.RelativeLevel +
                                    m_GeneratorParams.Stone.SnowBorderLevel + random.Next(
                                        -m_GeneratorParams.Stone.SnowBorderScattering,
                                        m_GeneratorParams.Stone.SnowBorderScattering);

                    for (var y = 0; y < m_ChunkSize.Height; y++)
                    {
                        if (y <= stoneLevelLower)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Stone;
                        else if (y < height && y > stoneLevelLower && y < stoneLevelUpper)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Ground;
                        else if (y <= height && y >= stoneLevelUpper && y < snowLevel)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Stone;
                        else if (y < height - 1 && y >= snowLevel)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Stone;
                        else if (y == height - 1 && y >= snowLevel)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.SnowedStone;
                        else if (y == height && y >= snowLevel)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Snow;
                        else if (y == height)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Grass;
                        else if (y > height && y <= waterLevel)
                            m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Water;
                    }
                }
            }

            // Create trees
            for (var i = 0; i < m_GeneratorParams.Tree.MaximalTreeCount; i++)
            {
                var x = random.Next(4, m_ChunkSize.Width - 4);
                var z = random.Next(4, m_ChunkSize.Width - 4);
                var nv = CalculateNoise(x, z, m_GeneratorParams.Tree.Noise);
                if (nv > m_GeneratorParams.Tree.Threshold)
                {
                    var treeHeight = random.Next(m_GeneratorParams.Tree.MinimumHeight,
                        m_GeneratorParams.Tree.MaximumHeight);
                    var ty = GetSoilLevel(x, z, config) + 1;
                    var sy = Mathf.Min(ty + treeHeight, m_ChunkSize.Height);

                    for (var y = ty; y < sy; y++)
                        m_Blocks[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.IDs.Wood;

                    var leavesType = random.Next(0, 2);
                    var radius = random.Next(1, 4);
                    var center = new Vector3(x + 0.5f, sy, z + 0.5f);
                    for (var py = sy - radius; py <= sy + radius; py++)
                    {
                        for (var pz = z - radius; pz <= z + radius; pz++)
                        {
                            for (var px = x - radius; px <= x + radius; px++)
                            {
                                var index = m_ChunkSize.ToIndex(px, py, pz);
                                if (m_ChunkSize.IsValid(index) && (pz != z || px != x || py >= sy))
                                {
                                    var point = new Vector3(px + 0.5f, py, pz + 0.5f);
                                    var distance = (point - center).magnitude;
                                    if (distance <= radius)
                                    {
                                        if (m_Blocks[index] == (int)BlockType.IDs.Air)
                                            m_Blocks[index] = (int)BlockType.IDs.Leaves + leavesType;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Disposes the allocated memory used by this job.
        /// </summary>
        public void Dispose()
        {
            m_Blocks.Dispose();
        }

        public ChunkPosition GetChunkPosition()
        {
            return m_ChunkPosition;
        }

        public int[] GetResult()
        {
            return m_Blocks.ToArray();
        }

        /// <summary>
        /// Calculates a perlin noise value for the given parameters.
        /// </summary>
        /// <param name="x">The X coordinate of a block</param>
        /// <param name="z">The Z coordinate of a block</param>
        /// <param name="noiseParams">Noise parameters</param>
        /// <returns>The calculated value.</returns>
        private float CalculateNoise(int x, int z, NoiseParams noiseParams)
        {
            var scale = (float)noiseParams.scale;
            var pnX = x / scale + m_ChunkPosition.X * m_ChunkSize.Width / scale;
            var pnZ = z / scale + m_ChunkPosition.Z * m_ChunkSize.Width / scale;

            var result = 0f;
            var normal = 0f;
            for (var i = 0; i < noiseParams.octaves; i++)
            {
                var f = Mathf.Pow(2, i) * noiseParams.frequency;
                var n = 1f / f;
                result += n * Mathf.PerlinNoise(pnX * f + noiseParams.seed.x, pnZ * f + noiseParams.seed.y);
                normal += n;
            }

            result = Mathf.Pow(result / normal, noiseParams.redistribution);
            result = result * noiseParams.amplitude * (1 + (noiseParams.redistribution - 1) *
                noiseParams.redistributionScale);
            return result;
        }

        /// <summary>
        /// Returns the height of soil block for this given coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="config">The configuration instance</param>
        /// <returns></returns>
        private int GetSoilLevel(int x, int z, Configuration config)
        {
            var y = m_ChunkSize.Height - 1;
            do
            {
                var blockType = config.GetBlockType(m_Blocks[m_ChunkSize.ToIndex(x, y, z)]);
                if (blockType.IsSoil)
                    return y;
                if (!blockType.IsEmpty)
                    return -1;
                y--;
            } while (y >= 0);

            return -1;
        }
    }
}