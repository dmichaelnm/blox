using Blox.Environment;
using Blox.Utility;
using Newtonsoft.Json;

namespace Blox.MainMenu
{
    public class Preset
    {
        public readonly string name;
        public readonly NoiseParameter terrainNoiseParameter;
        public readonly float terrainAmplitude;
        public readonly NoiseParameter waterNoiseParameter;
        public readonly float waterAmplitude;
        public readonly int waterLineOffset;
        public readonly NoiseParameter stoneNoiseParameter;    
        public readonly int stoneRelativeLevel;
        public readonly int stoneScattering; 
        public readonly int snowRelativeLevel;
        public readonly int snowScattering;
        public readonly int maxTreeCount;
        public readonly NoiseParameter treeNoiseParameter;
        public readonly float threshold;
        public readonly int minTreeHeight;
        public readonly int maxTreeHeight;
        public readonly float coalProbability;

        public int randomSeed;
        
        public Preset(JsonTextReader reader)
        {
            reader.NextPropertyValue("name", out name);
            reader.NextPropertyNameIs("properties");
            reader.NextTokenIsStartObject();

            // terrain
            reader.NextPropertyNameIs("terrain");
            reader.NextTokenIsStartObject();
            terrainNoiseParameter = ReadNoiseParameter(reader);
            reader.NextPropertyValue("amplitude", out terrainAmplitude);
            reader.NextTokenIsEndObject();
            
            // water
            reader.NextPropertyNameIs("water");
            reader.NextTokenIsStartObject();
            waterNoiseParameter = ReadNoiseParameter(reader);
            reader.NextPropertyValue("amplitude", out waterAmplitude);
            reader.NextPropertyValue("waterLineOffset", out waterLineOffset);
            reader.NextTokenIsEndObject();
            
            // stone
            reader.NextPropertyNameIs("stone");
            reader.NextTokenIsStartObject();
            stoneNoiseParameter = ReadNoiseParameter(reader);
            reader.NextPropertyValue("stoneRelativeLevel", out stoneRelativeLevel);
            reader.NextPropertyValue("stoneScattering", out stoneScattering);
            reader.NextPropertyValue("snowRelativeLevel", out snowRelativeLevel);
            reader.NextPropertyValue("snowScattering", out snowScattering);
            reader.NextTokenIsEndObject();
            
            // tree
            reader.NextPropertyNameIs("tree");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("maxTreeCount", out maxTreeCount);
            treeNoiseParameter = ReadNoiseParameter(reader);
            reader.NextPropertyValue("threshold", out threshold);
            reader.NextPropertyValue("minTreeHeight", out minTreeHeight);
            reader.NextPropertyValue("maxTreeHeight", out maxTreeHeight);
            reader.NextTokenIsEndObject();
            
            // resources
            reader.NextPropertyNameIs("resources");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("coalProbability", out coalProbability);
            reader.NextTokenIsEndObject();
            
            reader.NextTokenIsEndObject();
        }

        private NoiseParameter ReadNoiseParameter(JsonTextReader reader)
        {
            var np = new NoiseParameter();
            reader.NextPropertyValue("noiseScale", out np.noiseScale);
            reader.NextPropertyValue("octaves", out np.octaves);
            reader.NextPropertyValue("frequency", out np.frequency);
            reader.NextPropertyValue("redistribution", out np.redistribution);
            reader.NextPropertyValue("redistributionScale", out np.redistributionScale);
            return np;
        } 
    }
}