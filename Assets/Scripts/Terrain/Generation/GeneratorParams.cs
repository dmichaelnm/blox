using System;
using Blox.CommonNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.TerrainNS.Generation
{
    [Serializable]
    public struct GeneratorParams
    {
        [Serializable]
        public struct TerrainParams
        {
            public NoiseParams noise;
            public float amplitude;
        }

        [Serializable]
        public struct WaterParams
        {
            public NoiseParams noise;
            public float amplitude;
            public int waterOffset;
        }

        [Serializable]
        public struct SandParams
        {
            public NoiseParams noise;
            public float amplitude;
        }
        
        [Serializable]
        public struct StoneParams
        {
            public int stoneLineOffset;
            public int stoneScattering;
            public int snowLineOffset;
            public int snowScattering;
            public NoiseParams noise;
            public float threshold;
        }

        [Serializable]
        public struct TreeParams
        {
            public int maxTreeCount;
            public NoiseParams noise;
            public float threshold;
            public int minHeight;
            public int maxHeight;
            public int maxRadius;
        }

        public int randomSeed;
        public int baseLine;
        public TerrainParams terrain;
        public WaterParams water;
        public SandParams sand;
        public StoneParams stone;
        public TreeParams tree;

        public void InitializeSeeds(int randomSeed)
        {
            this.randomSeed = randomSeed;
            var random = new System.Random(randomSeed);
            terrain.noise.seed = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
            water.noise.seed = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
            sand.noise.seed = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
            stone.noise.seed = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
            tree.noise.seed = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000));
        }

        public void Load(JsonTextReader reader)
        {
            reader.NextPropertyNameIs("generatorParams");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("randomSeed", out randomSeed);
            reader.NextPropertyValue("baseLine", out baseLine);
            
            reader.NextPropertyNameIs("terrain");
            reader.NextTokenIsStartObject();
            terrain.noise.Load(reader);
            reader.NextPropertyValue("amplitude", out terrain.amplitude);
            reader.NextTokenIsEndObject();
            
            reader.NextPropertyNameIs("water");
            reader.NextTokenIsStartObject();
            water.noise.Load(reader);
            reader.NextPropertyValue("amplitude", out water.amplitude);
            reader.NextPropertyValue("waterOffset", out water.waterOffset);
            reader.NextTokenIsEndObject();

            reader.NextPropertyNameIs("sand");
            reader.NextTokenIsStartObject();
            sand.noise.Load(reader);
            reader.NextPropertyValue("amplitude", out sand.amplitude);
            reader.NextTokenIsEndObject();
            
            reader.NextPropertyNameIs("stone");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("stoneLineOffset", out stone.stoneLineOffset);
            reader.NextPropertyValue("stoneScattering", out stone.stoneScattering);
            reader.NextPropertyValue("snowLineOffset", out stone.snowLineOffset);
            reader.NextPropertyValue("snowScattering", out stone.snowScattering);
            stone.noise.Load(reader);
            reader.NextPropertyValue("threshold", out stone.threshold);
            reader.NextTokenIsEndObject();
            
            reader.NextPropertyNameIs("tree");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("maxTreeCount", out tree.maxTreeCount);
            tree.noise.Load(reader);
            reader.NextPropertyValue("threshold", out tree.threshold);
            reader.NextPropertyValue("minHeight", out tree.minHeight);
            reader.NextPropertyValue("maxHeight", out tree.maxHeight);
            reader.NextPropertyValue("maxRadius", out tree.maxRadius);
            reader.NextTokenIsEndObject();
            
            reader.NextTokenIsEndObject();
        }

        public void Save(JsonTextWriter writer)
        {
            writer.WritePropertyName("generatorParams");
            writer.WriteStartObject();
            writer.WriteProperty("randomSeed", randomSeed);
            writer.WriteProperty("baseLine", baseLine);

            writer.WritePropertyName("terrain");
            writer.WriteStartObject();
            terrain.noise.Save(writer);
            writer.WriteProperty("amplitude", terrain.amplitude);
            writer.WriteEndObject();
            
            writer.WritePropertyName("water");
            writer.WriteStartObject();
            water.noise.Save(writer);
            writer.WriteProperty("amplitude", water.amplitude);
            writer.WriteProperty("waterOffset", water.waterOffset);
            writer.WriteEndObject();

            writer.WritePropertyName("sand");
            writer.WriteStartObject();
            sand.noise.Save(writer);
            writer.WriteProperty("amplitude", sand.amplitude);
            writer.WriteEndObject();
            
            writer.WritePropertyName("stone");
            writer.WriteStartObject();
            writer.WriteProperty("stoneLineOffset", stone.stoneLineOffset);
            writer.WriteProperty("stoneScattering", stone.stoneScattering);
            writer.WriteProperty("snowLineOffset", stone.snowLineOffset);
            writer.WriteProperty("snowScattering", stone.snowScattering);
            stone.noise.Save(writer);
            writer.WriteProperty("threshold", stone.threshold);
            writer.WriteEndObject();
            
            writer.WritePropertyName("tree");
            writer.WriteStartObject();
            writer.WriteProperty("maxTreeCount", tree.maxTreeCount);
            tree.noise.Save(writer);
            writer.WriteProperty("threshold", tree.threshold);
            writer.WriteProperty("minHeight", tree.minHeight);
            writer.WriteProperty("maxHeight", tree.maxHeight);
            writer.WriteProperty("maxRadius", tree.maxRadius);
            writer.WriteEndObject();
            
            writer.WriteEndObject();
        }
    }
}