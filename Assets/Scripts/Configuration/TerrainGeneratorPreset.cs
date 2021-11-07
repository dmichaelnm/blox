using Blox.EnvironmentNS.GeneratorNS;
using Blox.UtilitiesNS;
using Newtonsoft.Json;

namespace Blox.ConfigurationNS
{
    /// <summary>
    /// This class contains informations for a terrain generator preset defined in a json config file.
    /// </summary>
    public class TerrainGeneratorPreset
    {
        /// <summary>
        /// The name of the preset.
        /// </summary>
        public readonly string Name;
        
        /// <summary>
        /// The generator parameters for this preset.
        /// </summary>
        public readonly GeneratorParams GeneratorParams;
        
        /// <summary>
        /// Creates a new terrain generator preset and loads the content from the reader object.
        /// </summary>
        /// <param name="reader">The JSON text reader instance</param>
        public TerrainGeneratorPreset(JsonTextReader reader)
        {
            reader.NextPropertyValue("name", out Name);
            
            GeneratorParams = new GeneratorParams();
            reader.NextPropertyNameIs("properties");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("baseline", out GeneratorParams.baseLine);
            
            // terrain
            reader.NextPropertyNameIs("terrain");
            reader.NextTokenIsStartObject();
            GeneratorParams.Terrain.Noise = ReadNoiseParams(reader);
            reader.NextTokenIsEndObject();
            
            // water
            reader.NextPropertyNameIs("water");
            reader.NextTokenIsStartObject();
            GeneratorParams.Water.Noise = ReadNoiseParams(reader);
            reader.NextPropertyValue("waterOffset", out GeneratorParams.Water.waterOffset);
            reader.NextTokenIsEndObject();
            
            // stone
            reader.NextPropertyNameIs("stone");
            reader.NextTokenIsStartObject();
            GeneratorParams.Stone.Noise = ReadNoiseParams(reader);
            reader.NextPropertyValue("stoneLevel", out GeneratorParams.Stone.RelativeLevel);
            reader.NextPropertyValue("stoneScattering", out GeneratorParams.Stone.Scattering);
            reader.NextPropertyValue("snowLevel", out GeneratorParams.Stone.SnowBorderLevel);
            reader.NextPropertyValue("snowScattering", out GeneratorParams.Stone.SnowBorderScattering);
            reader.NextTokenIsEndObject();
            
            // tree
            reader.NextPropertyNameIs("tree");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("maxTreeCount", out GeneratorParams.Tree.MaximalTreeCount);
            GeneratorParams.Tree.Noise = ReadNoiseParams(reader);
            reader.NextPropertyValue("threshold", out GeneratorParams.Tree.Threshold);
            reader.NextPropertyValue("minHeight", out GeneratorParams.Tree.MinimumHeight);
            reader.NextPropertyValue("maxHeight", out GeneratorParams.Tree.MaximumHeight);
            reader.NextTokenIsEndObject();
            
            reader.NextTokenIsEndObject();
        }

        /// <summary>
        /// Reads noise paramater values from the json text reader.
        /// </summary>
        /// <param name="reader">The JSON text reader instance</param>
        /// <returns>Noise parameters</returns>
        private NoiseParams ReadNoiseParams(JsonTextReader reader)
        {
            var np = new NoiseParams();
            reader.NextPropertyValue("scale", out np.scale);
            reader.NextPropertyValue("octaves", out np.octaves);
            reader.NextPropertyValue("frequency", out np.frequency);
            reader.NextPropertyValue("redistribution", out np.redistribution);
            reader.NextPropertyValue("redistributionScale", out np.redistributionScale);
            reader.NextPropertyValue("amplitude", out np.amplitude);
            return np;
        }
    }
}