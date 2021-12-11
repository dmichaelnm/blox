using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blox.CommonNS;
using Blox.TerrainNS.Generation;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.ConfigurationNS
{
    public class Configuration
    {
        private static Configuration _configuration;

        public static Configuration GetInstance()
        {
            return _configuration ??= new Configuration(
                "Configuration/textures",
                "Configuration/entities",
                "Configuration/terrain-presets"
            );
        }

        private Dictionary<string, TextureType> m_TextureTypes;
        private Dictionary<int, EntityType> m_EntityTypes;
        private Dictionary<string, GeneratorParams> m_TerrainPresets;

        private Configuration(
            string textureTypesConfigPath,
            string entityTypesConfigPath,
            string terrainPresetsPath
        )
        {
            Log.Info(this, "Configuration loaded.", () =>
            {
                m_TextureTypes = new Dictionary<string, TextureType>();
                m_EntityTypes = new Dictionary<int, EntityType>();
                m_TerrainPresets = new Dictionary<string, GeneratorParams>();

                LoadTextureTypes(textureTypesConfigPath);
                LoadEntityTypes(entityTypesConfigPath);
                LoadTerrainPresets(terrainPresetsPath);
            });
        }

        public TextureType GetTextureType([NotNull] string name)
        {
            if (m_TextureTypes.ContainsKey(name))
                return m_TextureTypes[name];

            return null;
        }

        public List<T> GetEntities<T>() where T : EntityType
        {
            var entities = new List<T>();
            foreach (var entity in m_EntityTypes.Values)
            {
                if (entity is T typedEntity)
                    entities.Add(typedEntity);
            }

            return entities;
        }

        public T GetEntityType<T>(BlockType.ID id) where T : EntityType
        {
            return GetEntityType<T>((int)id);
        }

        public T GetEntityType<T>(int id) where T : EntityType
        {
            if (m_EntityTypes.ContainsKey(id))
            {
                var entity = m_EntityTypes[id];
                if (entity is T type)
                    return type;
            }

            return null;
        }

        public GeneratorParams GetTerrainPreset([NotNull] string name)
        {
            if (m_TerrainPresets.ContainsKey(name))
                return m_TerrainPresets[name];

            return default;
        }

        public string[] GetTerrainPresetNames()
        {
            return m_TerrainPresets.Keys.ToArray();
        }

        private void LoadTextureTypes(string textureTypesConfigPath)
        {
            var asset = Resources.Load<TextAsset>(textureTypesConfigPath);
            if (asset == null)
            {
                Log.Error(this, $"No texture type configuration found at \"{textureTypesConfigPath}\".");
                return;
            }

            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.ForEachObject("textures", index =>
                {
                    var textureType = new TextureType(reader);
                    m_TextureTypes.Add(textureType.name, textureType);
                });
                reader.NextTokenIsEndObject();
            }

            Log.Debug(this, $"{m_TextureTypes.Count} texture types loaded.");
        }

        private void LoadEntityTypes(string entityTypesConfigPath)
        {
            var asset = Resources.Load<TextAsset>(entityTypesConfigPath);
            if (asset == null)
            {
                Log.Error(this, $"No entity type configuration found at \"{entityTypesConfigPath}\".");
                return;
            }

            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.ForEachObject("entities", index =>
                {
                    reader.NextPropertyValue("type", out EntityType.Type type);
                    EntityType entity = null;
                    if (type == EntityType.Type.Block)
                        entity = new BlockType(reader, this);
                    else if (type == EntityType.Type.CreatableBlock)
                        entity = new CreatableBlockType(reader, this);
                    else if (type == EntityType.Type.CreatableModel)
                        entity = new CreatableModelType(reader, this);

                    if (entity != null)
                        m_EntityTypes.Add(entity.id, entity);
                });
                reader.NextTokenIsEndObject();
            }

            Log.Debug(this, $"{m_EntityTypes.Count} entity types loaded.");
        }

        private void LoadTerrainPresets(string terrainPresetsPath)
        {
            var asset = Resources.Load<TextAsset>(terrainPresetsPath);
            if (asset == null)
            {
                Log.Error(this, $"No terrain preset configuration found at \"{terrainPresetsPath}\".");
                return;
            }

            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.ForEachObject("presets", index =>
                {
                    reader.NextPropertyValue("name", out string name);
                    var generatorParams = new GeneratorParams();
                    generatorParams.Load(reader);
                    m_TerrainPresets.Add(name, generatorParams);
                });
            }

            Log.Debug(this, $"{m_TerrainPresets.Count} terrain presets loaded.");
        }
    }
}