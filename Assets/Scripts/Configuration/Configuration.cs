using System.Collections.Generic;
using System.IO;
using Blox.CommonNS;
using Blox.UtilitiesNS;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blox.ConfigurationNS
{
    /// <summary>
    /// This class contains the configuration information of the game.
    /// </summary>
    public class Configuration
    {
        /// <summary>
        /// Singleton instance of this class.
        /// </summary>
        private static Configuration _configuration;

        /// <summary>
        /// Returns the instance of this class.
        /// </summary>
        /// <returns>Configuration instance</returns>
        public static Configuration GetInstance()
        {
            return _configuration ??= new Configuration();
        }

        /// <summary>
        /// Cache for all texture types.
        /// </summary>
        private Dictionary<string, TextureType> m_TextureTypes;

        /// <summary>
        /// Cache for all block types.
        /// </summary>
        private List<BlockType> m_BlockTypes;

        /// <summary>
        /// List of loaded terrain generator presets.
        /// </summary>
        private List<TerrainGeneratorPreset> m_TerrinaGeneratorPresets;
        
        /// <summary>
        /// Private constructor.
        /// </summary>
        private Configuration()
        {
            var performance = new PerfomanceInfo();
            performance.StartMeasure();

            LoadTextureTypes();
            LoadBlockTypes();
            LoadPresets();

            Debug.Log($"Configuration loaded : {performance}");
            Debug.Log($"  - {m_TextureTypes.Count} texture types");
            Debug.Log($"  - {m_BlockTypes.Count} block types");
            Debug.Log($"  - {m_TerrinaGeneratorPresets.Count} presets");
        }

        /// <summary>
        /// Returns the texture type for the given name.
        /// </summary>
        /// <param name="name">The name of the texture type</param>
        /// <returns>The texture type</returns>
        public TextureType GetTextureType([NotNull] string name)
        {
            Assert.IsTrue(m_TextureTypes.ContainsKey(name));
            return m_TextureTypes[name];
        }

        /// <summary>
        /// Returns the block type for the given ID.
        /// </summary>
        /// <param name="id">The ID of the texture type</param>
        /// <returns>The block type</returns>
        public BlockType GetBlockType(int id)
        {
            Assert.IsTrue(id >= 0 && id < m_BlockTypes.Count);
            return m_BlockTypes[id];
        }

        /// <summary>
        /// Returns an array with the loaded terrain generator presets.
        /// </summary>
        /// <returns>Array of presets</returns>
        public TerrainGeneratorPreset[] GetTerrainGeneratorPresets()
        {
            return m_TerrinaGeneratorPresets.ToArray();
        }
        
        /// <summary>
        /// Loads the texture types defined in the configuration file.
        /// </summary>
        private void LoadTextureTypes()
        {
            m_TextureTypes = new Dictionary<string, TextureType>();
            var asset = Resources.Load<TextAsset>("Configuration/textures");
            Assert.IsNotNull(asset);
            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("textures");
                reader.IterateOverObjectArray(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var textureType = new TextureType(reader);
                    m_TextureTypes.Add(textureType.Name, textureType);
                });
            }
        }

        /// <summary>
        /// Loads the block types defined in the configuration file.
        /// </summary>
        private void LoadBlockTypes()
        {
            m_BlockTypes = new List<BlockType>();
            var asset = Resources.Load<TextAsset>("Configuration/blocks");
            Assert.IsNotNull(asset);
            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("blocks");
                reader.IterateOverObjectArray(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var blockType = new BlockType(reader);
                    m_BlockTypes.Add(blockType);
                });
            }
            m_BlockTypes.Sort((bt1, bt2) => bt1.ID - bt2.ID);
        }

        /// <summary>
        /// Loads the presets defines in the configuration file.
        /// </summary>
        private void LoadPresets()
        {
            m_TerrinaGeneratorPresets = new List<TerrainGeneratorPreset>();
            var asset = Resources.Load<TextAsset>("Configuration/presets");
            Assert.IsNotNull(asset);
            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("terrain-generator");
                reader.IterateOverObjectArray(() =>
                {
                    // ReSharper disable once AccessToDisposedClosure
                    var preset = new TerrainGeneratorPreset(reader);
                    m_TerrinaGeneratorPresets.Add(preset);
                });
            }
        }
    }
}