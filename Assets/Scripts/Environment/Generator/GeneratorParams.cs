using System;
using Blox.UtilitiesNS;
using Newtonsoft.Json;

namespace Blox.EnvironmentNS.GeneratorNS
{
    [Serializable]
    public struct GeneratorParams
    {
        [Serializable]
        public struct TerrainParams
        {
            public NoiseParams Noise;
        }

        [Serializable]
        public struct WaterParams
        {
            public NoiseParams Noise;
            public int waterOffset;
        }

        [Serializable]
        public struct StoneParams
        {
            public NoiseParams Noise;
            public int RelativeLevel;
            public int Scattering;
            public int SnowBorderLevel;
            public int SnowBorderScattering;
        }

        [Serializable]
        public struct TreeParams
        {
            public int MaximalTreeCount;
            public NoiseParams Noise;
            public float Threshold;
            public int MinimumHeight;
            public int MaximumHeight;
        }
        
        public int randomSeed;
        public int baseLine;
        public TerrainParams Terrain;
        public WaterParams Water;
        public StoneParams Stone;
        public TreeParams Tree;

        public void Write(JsonTextWriter writer)
        {
            writer.WritePropertyName("generatorParams");
            
            writer.WriteStartObject();
            writer.WriteProperty("randomSeed", randomSeed);
            writer.WriteProperty("baseLine", baseLine);
            
            writer.WritePropertyName("terrain");
            writer.WriteStartObject();
            Terrain.Noise.Write(writer);
            writer.WriteEndObject();
            
            writer.WritePropertyName("water");
            writer.WriteStartObject();
            Water.Noise.Write(writer);
            writer.WriteProperty("waterOffset", Water.waterOffset);
            writer.WriteEndObject();

            writer.WritePropertyName("stone");
            writer.WriteStartObject();
            Stone.Noise.Write(writer);
            writer.WriteProperty("relativeLevel", Stone.RelativeLevel);
            writer.WriteProperty("scattering", Stone.Scattering);
            writer.WriteProperty("snowBorderLevel", Stone.SnowBorderLevel);
            writer.WriteProperty("snowBorderScattering", Stone.SnowBorderScattering);
            writer.WriteEndObject();
            
            writer.WritePropertyName("tree");
            writer.WriteStartObject();
            writer.WriteProperty("maximalTreeCount", Tree.MaximalTreeCount);
            Tree.Noise.Write(writer);
            writer.WriteProperty("threshold", Tree.Threshold);
            writer.WriteProperty("minimumHeight", Tree.MinimumHeight);
            writer.WriteProperty("maximumHeight", Tree.MaximumHeight);
            writer.WriteEndObject();
            
            writer.WriteEndObject();
        }

        public void Read(JsonTextReader reader)
        {
            reader.NextPropertyNameIs("generatorParams");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("randomSeed", out randomSeed);
            reader.NextPropertyValue("baseLine", out baseLine);

            reader.NextPropertyNameIs("terrain");
            reader.NextTokenIsStartObject();
            Terrain.Noise.Read(reader);
            reader.NextTokenIsEndObject();

            reader.NextPropertyNameIs("water");
            reader.NextTokenIsStartObject();
            Water.Noise.Read(reader);
            reader.NextPropertyValue("waterOffset", out Water.waterOffset);
            reader.NextTokenIsEndObject();

            reader.NextPropertyNameIs("stone");
            reader.NextTokenIsStartObject();
            Stone.Noise.Read(reader);
            reader.NextPropertyValue("relativeLevel", out Stone.RelativeLevel);
            reader.NextPropertyValue("scattering", out Stone.Scattering);
            reader.NextPropertyValue("snowBorderLevel", out Stone.SnowBorderLevel);
            reader.NextPropertyValue("snowBorderScattering", out Stone.SnowBorderScattering);
            reader.NextTokenIsEndObject();
            
            reader.NextPropertyNameIs("tree");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("maximalTreeCount", out Tree.MaximalTreeCount);
            Tree.Noise.Read(reader);
            reader.NextPropertyValue("threshold", out Tree.Threshold);
            reader.NextPropertyValue("minimumHeight", out Tree.MinimumHeight);
            reader.NextPropertyValue("maximumHeight", out Tree.MaximumHeight);
        }
    }
}