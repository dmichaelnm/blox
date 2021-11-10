using System;
using Blox.UtilitiesNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.EnvironmentNS.GeneratorNS
{
    [Serializable]
    public struct NoiseParams
    {
        public Vector2 seed;
        public int scale;
        public int octaves;
        public float frequency;
        public float redistribution;
        [Range(0f, 1f)] public float redistributionScale;
        public float amplitude;

        public void Write(JsonTextWriter writer)
        {
            writer.WritePropertyName("noise");
            writer.WriteStartObject();
            writer.WriteProperty("seedX", seed.x);
            writer.WriteProperty("seedY", seed.y);
            writer.WriteProperty("scale", scale);
            writer.WriteProperty("octaves", octaves);
            writer.WriteProperty("frequency", frequency);
            writer.WriteProperty("redistribution", redistribution);
            writer.WriteProperty("redistributionScale", redistributionScale);
            writer.WriteProperty("amplitude", amplitude);
            writer.WriteEndObject();
        }

        public void Read(JsonTextReader reader)
        {
            reader.NextPropertyNameIs("noise");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("seedX", out float seedX);
            reader.NextPropertyValue("seedY", out float seedY);
            seed = new Vector2(seedX, seedY);
            reader.NextPropertyValue("scale", out scale);
            reader.NextPropertyValue("octaves", out octaves);
            reader.NextPropertyValue("frequency", out frequency);
            reader.NextPropertyValue("redistribution", out redistribution);
            reader.NextPropertyValue("redistributionScale", out redistributionScale);
            reader.NextPropertyValue("amplitude", out amplitude);
            reader.NextTokenIsEndObject();
        }
    }
}