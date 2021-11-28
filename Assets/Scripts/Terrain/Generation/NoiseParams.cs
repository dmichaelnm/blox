using System;
using Blox.CommonNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.TerrainNS.Generation
{
    [Serializable]
    public struct NoiseParams
    {
        public int scale;
        public Vector2 seed;
        public int octaves;
        public float frequency;
        public float redistribution;
        [Range(0f, 1f)] public float redistributionScale;

        public float redistributionScaleFactor => (redistribution - 1f) * redistributionScale + 1f;

        public void Load(JsonTextReader reader)
        {
            reader.NextPropertyNameIs("noise");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("scale", out scale);
            reader.NextPropertyValue("seed", out seed);
            reader.NextPropertyValue("octaves", out octaves);
            reader.NextPropertyValue("frequency", out frequency);
            reader.NextPropertyValue("redistribution", out redistribution);
            reader.NextPropertyValue("redistributionScale", out redistributionScale);
            reader.NextTokenIsEndObject();
        }

        public void Save(JsonTextWriter writer)
        {
            writer.WritePropertyName("noise");
            writer.WriteStartObject();
            writer.WriteProperty("scale", scale);
            writer.WriteProperty("seed", seed);
            writer.WriteProperty("octaves", octaves);
            writer.WriteProperty("frequency", frequency);
            writer.WriteProperty("redistribution", redistribution);
            writer.WriteProperty("redistributionScale", redistributionScale);
            writer.WriteEndObject();
        }
    }
}