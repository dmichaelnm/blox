using System.Collections.Generic;
using System.IO;
using System.Linq;
using Blox.Utility;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.Environment.Config
{
    public class Configuration
    {
        private static Configuration _instance;

        private readonly List<TextureType> m_TextureTypes;
        private readonly List<BlockType> m_BlockTypes;
        
        public static Configuration GetInstance()
        {
            return _instance ??= new Configuration();
        }
        
        private Configuration()
        {
            var startTime = Time.realtimeSinceStartup;

            m_TextureTypes = new List<TextureType>();

            var asset = Resources.Load<TextAsset>("textures");
            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("textures");
                reader.NextTokenIsStartArray();
                while (!reader.NextTokenIsEndArray())
                {
                    reader.CurrentTokenIsStartObject();
                    var textureType = new TextureType(reader);
                    m_TextureTypes.Add(textureType);
                    reader.NextTokenIsEndObject();
                }
            }

            m_BlockTypes = new List<BlockType>();

            asset = Resources.Load<TextAsset>("blocks");
            using (var reader = new JsonTextReader(new StringReader(asset.text)))
            {
                reader.NextTokenIsStartObject();
                reader.NextPropertyNameIs("blocks");
                reader.NextTokenIsStartArray();
                while (!reader.NextTokenIsEndArray())
                {
                    reader.CurrentTokenIsStartObject();
                    var blockType = new BlockType(reader);
                    m_BlockTypes.Add(blockType);
                    reader.NextTokenIsEndObject();
                }
            }

            var time = (Time.realtimeSinceStartup - startTime) * 1000f;
            Debug.Log("Loading environment configuration (" + time + "ms)");
        }

        public BlockType GetBlockType(int id)
        {
            return m_BlockTypes[id];
        }
        
        public TextureType GetTextureType([NotNull] string textureTypeName)
        {
            return m_TextureTypes.FirstOrDefault(textureType => textureType.name.Equals(textureTypeName));
        }
    }
}