using Blox.ConfigurationNS;
using Blox.TerrainNS.Generation;
using Unity.Collections;
using UnityEngine;
using Random = System.Random;

namespace Blox.TerrainNS.JobsNS
{
    public struct GenerateChunkDataJob : IChunkDataProviderJob
    {
        public struct GeneratorParamsJob
        {
            public readonly int randomSeed;
            public readonly int baseLine;
            public readonly GeneratorParams.TerrainParams terrain;
            public readonly GeneratorParams.WaterParams water;
            public readonly GeneratorParams.SandParams sand;
            public readonly GeneratorParams.StoneParams stone;
            public readonly GeneratorParams.TreeParams tree;
            [ReadOnly] public NativeArray<GeneratorParams.ResourcesParams> resources;

            public GeneratorParamsJob(GeneratorParams generatorParams)
            {
                randomSeed = generatorParams.randomSeed;
                baseLine = generatorParams.baseLine;
                terrain = generatorParams.terrain;
                water = generatorParams.water;
                sand = generatorParams.sand;
                stone = generatorParams.stone;
                tree = generatorParams.tree;
                resources = new NativeArray<GeneratorParams.ResourcesParams>(generatorParams.resources.Length,
                    Allocator.Persistent);
                for (var i = 0; i < generatorParams.resources.Length; i++)
                    resources[i] = generatorParams.resources[i];
            }
        }

        [ReadOnly] private ChunkSize m_ChunkSize;
        [ReadOnly] private ChunkPosition m_ChunkPosition;
        [ReadOnly] private GeneratorParamsJob m_GeneratorParams;

        private NativeArray<int> m_BlockTypeIds;

        public void Initialize(ChunkSize chunkSize, ChunkPosition chunkPosition, GeneratorParams generatorParams)
        {
            m_ChunkSize = chunkSize;
            m_ChunkPosition = chunkPosition;
            m_GeneratorParams = new GeneratorParamsJob(generatorParams);

            for (var i = 0; i < generatorParams.resources.Length; i++)
            {
                m_GeneratorParams.resources[i] = generatorParams.resources[i];
            }

            m_BlockTypeIds = new NativeArray<int>(m_ChunkSize.size, Allocator.Persistent);
        }

        public void Execute()
        {
            var random = new Random(m_GeneratorParams.randomSeed);

            GenerateTerrain(random);
            GenerateTrees(random);
        }

        public void Dispose()
        {
            m_BlockTypeIds.Dispose();
            m_GeneratorParams.resources.Dispose();
        }

        public ChunkPosition GetChunkPosition()
        {
            return m_ChunkPosition;
        }

        public int[] GetBlockTypeIds()
        {
            return m_BlockTypeIds.ToArray();
        }

        private void GenerateTerrain(Random random)
        {
            var baseLine = m_GeneratorParams.baseLine;
            var waterLine = baseLine + m_GeneratorParams.water.waterOffset;

            for (var z = 0; z < m_ChunkSize.width; z++)
            {
                for (var x = 0; x < m_ChunkSize.width; x++)
                {
                    // terrain height
                    var terrainNoise = Noise(x, z, m_GeneratorParams.terrain.noise);
                    var terrainHeight = Mathf.FloorToInt(terrainNoise * m_GeneratorParams.terrain.amplitude) +
                                        baseLine;

                    // water depth
                    var waterNoise = Noise(x, z, m_GeneratorParams.water.noise);
                    var waterDepth = Mathf.FloorToInt(waterNoise * m_GeneratorParams.water.amplitude);
                    terrainHeight -= waterDepth;

                    // stone level
                    var stoneParams = m_GeneratorParams.stone;
                    var stoneHeight = baseLine + stoneParams.stoneLineOffset;
                    var stoneScattering = random.Next(-stoneParams.stoneScattering, stoneParams.stoneScattering);
                    stoneHeight += stoneScattering;

                    var stoneNoise = Noise(x, z, stoneParams.noise);
                    var stoneLevel = terrainHeight * (stoneNoise + stoneParams.threshold);

                    // snow level
                    var snowHeight = stoneHeight + stoneParams.snowLineOffset;
                    var snowScatterng = random.Next(-stoneParams.snowScattering, stoneParams.snowScattering);
                    snowHeight += snowScatterng;

                    // sand level
                    var sandParams = m_GeneratorParams.sand;
                    var sandNoise = Noise(x, z, sandParams.noise);
                    var sandDepth = Mathf.FloorToInt(sandNoise * sandParams.amplitude);

                    for (var y = 0; y < m_ChunkSize.height; y++)
                    {
                        var index = m_ChunkSize.ToIndex(x, y, z);

                        if (y <= terrainHeight)
                        {
                            // terrain
                            if (y < stoneHeight)
                            {
                                if (y > terrainHeight - sandDepth && y <= terrainHeight)
                                    // sand
                                    m_BlockTypeIds[index] = GetBlockTypeId(random, (int)BlockType.ID.Sand);
                                else if (y <= terrainHeight && y < stoneLevel)
                                    // stone
                                    m_BlockTypeIds[index] = GetBlockTypeId(random, (int)BlockType.ID.Stone);
                                else if (y == terrainHeight)
                                    // grass
                                    m_BlockTypeIds[index] = GetBlockTypeId(random, (int)BlockType.ID.Grass);
                                else
                                    // ground
                                    m_BlockTypeIds[index] = GetBlockTypeId(random, (int)BlockType.ID.Ground);
                            }
                            else
                            {
                                if (y == terrainHeight && y >= snowHeight)
                                    // snowed stone
                                    m_BlockTypeIds[index] = (int)BlockType.ID.SnowedStone;
                                else
                                    // stone
                                    m_BlockTypeIds[index] = (int)BlockType.ID.Stone;
                            }
                        }
                        else if (y > terrainHeight && y <= waterLine)
                            // water
                            m_BlockTypeIds[index] = (int)BlockType.ID.Water;
                        else
                            // air
                            m_BlockTypeIds[index] = (int)BlockType.ID.Air;
                    }
                }
            }
        }

        private void GenerateTrees(Random random)
        {
            var treeParams = m_GeneratorParams.tree;
            for (var i = 0; i < treeParams.maxTreeCount; i++)
            {
                var x = random.Next(treeParams.maxRadius, m_ChunkSize.width - treeParams.maxRadius);
                var z = random.Next(treeParams.maxRadius, m_ChunkSize.width - treeParams.maxRadius);
                var treeNoise = Noise(x, z, treeParams.noise);
                if (treeNoise > treeParams.threshold)
                {
                    var treeHeight = random.Next(treeParams.minHeight, treeParams.maxHeight);
                    var ty = GetSoilLevel(x, z) + 1;
                    if (ty > 0)
                    {
                        var sy = Mathf.Min(ty + treeHeight, m_ChunkSize.height);

                        for (var y = ty; y < sy; y++)
                            m_BlockTypeIds[m_ChunkSize.ToIndex(x, y, z)] = (int)BlockType.ID.Trunk;

                        var radius = random.Next(1, treeParams.maxRadius);
                        var center = new Vector3(x + 0.5f, sy, z + 0.5f);
                        for (var py = sy - radius; py <= sy + radius; py++)
                        {
                            for (var pz = z - radius; pz <= z + radius; pz++)
                            {
                                for (var px = x - radius; px <= x + radius; px++)
                                {
                                    var index = m_ChunkSize.ToIndex(px, py, pz);
                                    var point = new Vector3(px + 0.5f, py, pz + 0.5f);
                                    var distance = (point - center).magnitude;
                                    if (distance <= radius)
                                    {
                                        if (m_BlockTypeIds[index] == (int)BlockType.ID.Air)
                                            m_BlockTypeIds[index] = (int)BlockType.ID.Leaves;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private float Noise(int x, int z, NoiseParams noiseParams)
        {
            var px = (float)x / noiseParams.scale + m_ChunkPosition.x * m_ChunkSize.width / (float)noiseParams.scale;
            var pz = (float)z / noiseParams.scale + m_ChunkPosition.z * m_ChunkSize.width / (float)noiseParams.scale;

            var sum = 0f;
            var normalize = 0f;
            for (var i = 0; i < noiseParams.octaves; i++)
            {
                var frq = Mathf.Pow(2, i) * noiseParams.frequency;
                var nrm = 1f / frq;
                sum += nrm * Mathf.PerlinNoise(px * frq + noiseParams.seed.x, pz * frq + noiseParams.seed.y);
                normalize += nrm;
            }

            return Mathf.Pow(sum / normalize, noiseParams.redistribution) * noiseParams.redistributionScaleFactor;
        }

        private int GetSoilLevel(int x, int z)
        {
            var config = Configuration.GetInstance();
            var y = m_ChunkSize.height - 1;
            do
            {
                var id = m_BlockTypeIds[m_ChunkSize.ToIndex(x, y, z)];
                var blockType = config.GetEntityType<BlockType>(id);
                if (blockType.isSoil)
                    return y;
                if (!blockType.isEmpty)
                    return -1;
                y--;
            } while (y >= 0);

            return -1;
        }

        private int GetBlockTypeId(Random random, int masterBlockId)
        {
            foreach (var resource in m_GeneratorParams.resources)
            {
                if (resource.masterBlockId == masterBlockId)
                {
                    var value = (float)random.NextDouble();
                    if (value <= resource.probability)
                        return resource.resourceBlockId;
                }
            }

            return masterBlockId;
        }
    }
}