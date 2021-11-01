using Blox.Environment.Config;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

namespace Blox.Environment.Jobs
{
    public struct ChunkGenerationJob : IChunkDataProvider
    {
        // common generator properties
        private ChunkSize m_ChunkSize;
        private int m_ChunkPositionX;
        private int m_ChunkPositionZ;
        private int m_BaseLine;
        private int m_RandomSeed;

        // terrain generator properties
        private NoiseParameter m_TerrainNoiseParameter;
        private float m_TerrainAmplitude;
        private NoiseParameter m_WaterNoiseParameter;
        private float m_WaterAmplitude;
        private int m_WaterOffset;
        private NoiseParameter m_StoneNoiseParameter;
        private float m_StoneThreshold;
        private int m_StoneLevelRelative;
        private int m_StoneScattering;
        private int m_SnowLevelRelative;
        private int m_SnowScattering;

        // tree generator properties
        private int m_MaxTreeCount;
        private NoiseParameter m_TreeNoiseParameter;
        private float m_TreeThreshold;
        private int m_MinTreeHeight;
        private int m_MaxTreeHeight;

        // probabilities
        private float m_CoalProbability;

        // ----------------------------------
        private NativeArray<int> m_BlockTypeIds;

        public void Initialize(ChunkSize size, ChunkPosition position, int baseLine, int randomSeed,
            NoiseParameter terrainNoiseParameter, float terrainAmplitude, NoiseParameter waterNoiseParameter,
            float waterAmplitude, int waterOffset, NoiseParameter stoneNoiseParameter, float stoneThreshold,
            int stoneLevelRelative, int stoneScattering, int snowLevelRelative, int snowScattering, int maxTreeCount,
            NoiseParameter treeNoiseParameter, float treeThreshold, int minTreeHeight, int maxTreeHeight,
            float coalProbability)
        {
            m_ChunkSize = size;
            m_ChunkPositionX = position.x;
            m_ChunkPositionZ = position.z;
            m_BaseLine = baseLine;
            m_RandomSeed = randomSeed;
            m_TerrainNoiseParameter = terrainNoiseParameter;
            m_TerrainAmplitude = terrainAmplitude;
            m_WaterNoiseParameter = waterNoiseParameter;
            m_WaterAmplitude = waterAmplitude;
            m_WaterOffset = waterOffset;
            m_StoneNoiseParameter = stoneNoiseParameter;
            m_StoneThreshold = stoneThreshold;
            m_StoneLevelRelative = stoneLevelRelative;
            m_StoneScattering = stoneScattering;
            m_SnowLevelRelative = snowLevelRelative;
            m_SnowScattering = snowScattering;
            m_MaxTreeCount = maxTreeCount;
            m_TreeNoiseParameter = treeNoiseParameter;
            m_TreeThreshold = treeThreshold;
            m_MinTreeHeight = minTreeHeight;
            m_MaxTreeHeight = maxTreeHeight;
            m_CoalProbability = coalProbability;

            m_BlockTypeIds = new NativeArray<int>(size.size, Allocator.Persistent);
        }

        public void Execute()
        {
            var config = Configuration.GetInstance();
            var random = new Random(m_RandomSeed);
            var position = new ChunkPosition(m_ChunkPositionX, m_ChunkPositionZ);
            var waterLevel = m_BaseLine + m_WaterOffset;

            for (var z = 0; z < m_ChunkSize.width; z++)
            {
                for (var x = 0; x < m_ChunkSize.width; x++)
                {
                    var terrainCoords =
                        CalcNoiseInputCoordinates(x, z, m_TerrainNoiseParameter.noiseScale,
                            position,
                            m_ChunkSize);
                    var nv = CalcNoiseValue(terrainCoords, m_TerrainNoiseParameter.seed,
                        m_TerrainNoiseParameter.octaves,
                        m_TerrainNoiseParameter.frequency, m_TerrainAmplitude,
                        m_TerrainNoiseParameter.redistribution, m_TerrainNoiseParameter.redistributionScale);

                    var waterCoords =
                        CalcNoiseInputCoordinates(x, z, m_WaterNoiseParameter.noiseScale, position,
                            m_ChunkSize);
                    nv -= CalcNoiseValue(waterCoords, m_WaterNoiseParameter.seed,
                        m_WaterNoiseParameter.octaves,
                        m_WaterNoiseParameter.frequency, m_WaterAmplitude,
                        m_WaterNoiseParameter.redistribution, m_WaterNoiseParameter.redistributionScale);

                    var stoneCoords =
                        CalcNoiseInputCoordinates(x, z, m_StoneNoiseParameter.noiseScale, position,
                            m_ChunkSize);
                    var sv = CalcNoiseValue(stoneCoords, m_StoneNoiseParameter.seed,
                        m_StoneNoiseParameter.octaves,
                        m_StoneNoiseParameter.frequency, 1f, m_StoneNoiseParameter.redistribution,
                        m_StoneNoiseParameter.redistributionScale) + m_StoneThreshold;
                    var lowerStoneLevel = Mathf.FloorToInt(m_BaseLine * sv);

                    var height = Mathf.FloorToInt(nv) + m_BaseLine;
                    var upperStoneLevel =
                        m_BaseLine + m_StoneLevelRelative + random.Next(-m_StoneScattering, m_StoneScattering);
                    var snowLevel = m_BaseLine + m_StoneLevelRelative + m_SnowLevelRelative +
                                    random.Next(-m_SnowScattering, m_SnowScattering);

                    for (var y = 0; y < m_ChunkSize.height; y++)
                    {
                        var index = m_ChunkSize.ToIndex(x, y, z);
                        if (y <= height && y >= snowLevel)
                            m_BlockTypeIds[index] = (int)BlockTypes.Snow;
                        else if (y <= lowerStoneLevel && y <= height || y >= upperStoneLevel && y <= height)
                        {
                            var coalThreshold = (float)random.NextDouble();
                            if (coalThreshold < m_CoalProbability)
                                m_BlockTypeIds[index] = (int)BlockTypes.Coal;
                            else
                                m_BlockTypeIds[index] = (int)BlockTypes.Stone;
                        }
                        else if (y < height)
                            m_BlockTypeIds[index] = (int)BlockTypes.Ground;
                        else if (y == height)
                            m_BlockTypeIds[index] = (int)BlockTypes.Grass;
                        else if (y > height && y <= waterLevel)
                            m_BlockTypeIds[index] = (int)BlockTypes.Water;
                        else
                            m_BlockTypeIds[index] = (int)BlockTypes.Air;
                    }
                }
            }

            for (var z = 0; z < m_ChunkSize.width; z++)
            {
                for (var x = 0; x < m_ChunkSize.width; x++)
                {
                    var index = m_ChunkSize.ToIndex(x, waterLevel, z);
                    if (m_BlockTypeIds[index] == (int)BlockTypes.Water)
                    {
                        SetSandBlock(x > 0 ? index - 1 : -1); // left
                        SetSandBlock(x > 0 && z > 0 ? m_ChunkSize.ToIndex(x - 1, waterLevel, z - 1) : -1); // left front
                        SetSandBlock(x > 0 && z < m_ChunkSize.width - 1
                            ? m_ChunkSize.ToIndex(x - 1, waterLevel, z + 1)
                            : -1); // left back
                        SetSandBlock(x < m_ChunkSize.width - 1 ? index + 1 : -1); // right
                        SetSandBlock(z > 0 ? m_ChunkSize.ToIndex(x, waterLevel, z - 1) : -1); // front
                        SetSandBlock(x < m_ChunkSize.width - 1 && z > 0
                            ? m_ChunkSize.ToIndex(x + 1, waterLevel, z - 1)
                            : -1); // right front
                        SetSandBlock(x < m_ChunkSize.width - 1 && z < m_ChunkSize.width - 1
                            ? m_ChunkSize.ToIndex(x + 1, waterLevel, z + 1)
                            : -1); // right back
                    }
                }
            }

            for (var i = 0; i < m_MaxTreeCount; i++)
            {
                var x = random.Next(4, m_ChunkSize.width - 4);
                var z = random.Next(4, m_ChunkSize.width - 4);
                var coords =
                    CalcNoiseInputCoordinates(x, z, m_TreeNoiseParameter.noiseScale, position,
                        m_ChunkSize);
                var nv = CalcNoiseValue(coords, m_TreeNoiseParameter.seed, m_TreeNoiseParameter.octaves,
                    m_TreeNoiseParameter.frequency, 1f, m_TreeNoiseParameter.redistribution,
                    m_TreeNoiseParameter.redistributionScale);
                if (nv > m_TreeThreshold)
                {
                    var treeHeight = random.Next(m_MinTreeHeight, m_MaxTreeHeight);
                    var ty = GetSoilLevel(m_ChunkSize, x, z, config) + 1;
                    var sy = Mathf.Min(ty + treeHeight, m_ChunkSize.height);

                    for (var y = ty; y < sy; y++)
                        m_BlockTypeIds[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockTypes.Wood;

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
                                        if (m_BlockTypeIds[index] == (int)BlockTypes.Air)
                                            m_BlockTypeIds[index] = (int)BlockTypes.Leaves + leavesType;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            m_BlockTypeIds.Dispose();
        }

        public int[] GetBlockTypeIdArray() => m_BlockTypeIds.ToArray();

        public ChunkPosition GetChunkPosition() => new ChunkPosition(m_ChunkPositionX, m_ChunkPositionZ);

        private int GetSoilLevel(ChunkSize size, int x, int z, Configuration config)
        {
            var y = size.height - 1;
            do
            {
                var blockType = config.GetBlockType(m_BlockTypeIds[size.ToIndex(x, y, z)]);
                if (blockType.isSoil)
                    return y;
                if (!blockType.isEmpty)
                    return -1;
                y--;
            } while (y >= 0);

            return -1;
        }

        private Vector2 CalcNoiseInputCoordinates(int x, int z, int scale, ChunkPosition position, ChunkSize size)
        {
            var nx = x / (float)scale + position.x * size.width / (float)scale;
            var nz = z / (float)scale + position.z * size.width / (float)scale;
            return new Vector2(nx, nz);
        }

        private float CalcNoiseValue(Vector2 coords, Vector2 seed, int octaves, float frequency, float amplitude,
            float redistribution, float scale)
        {
            var value = 0f;
            var normal = 0f;
            for (var i = 0; i < octaves; i++)
            {
                var f = Mathf.Pow(2, i) * frequency;
                var n = 1 / f;
                value += n * Mathf.PerlinNoise(coords.x * f + seed.x, coords.y * f + seed.y);
                normal += n;
            }

            return Mathf.Pow(value / normal, redistribution) * amplitude * (1 + (redistribution - 1) * scale);
        }

        private void SetSandBlock(int index)
        {
            if (index != -1 && m_BlockTypeIds[index] != (int)BlockTypes.Water)
                m_BlockTypeIds[index] = (int)BlockTypes.Sand;
        }
    }
}