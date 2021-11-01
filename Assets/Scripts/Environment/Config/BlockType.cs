using Newtonsoft.Json;
using Blox.Utility;
using UnityEngine;

namespace Blox.Environment.Config
{
    public enum BlockTypes
    {
        Air,
        Ground,
        Grass,
        Water,
        Stone,
        Snow,
        Wood,
        Leaves,
        Leaves2,
        Coal,
        Sand
    }
    
    public class BlockType
    {
        public readonly int id;
        public readonly int baseId;
        public readonly string name;
        public readonly Sprite iconNormal;
        public readonly Sprite iconSelected;
        public readonly bool isSolid;
        public readonly bool isFluid;
        public readonly bool isSoil;
        public bool isEmpty => id == 0;

        private readonly string[] m_FaceTextureNames;

        public BlockType(JsonTextReader reader)
        {
            reader.NextPropertyValue("id", out id);
            reader.NextPropertyValue("baseId", out baseId);
            reader.NextPropertyValue("name", out name);

            reader.NextPropertyNameIs("icons");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("normal", out string spriteNormalPath);
            reader.NextPropertyValue("selected", out string spriteSelectedPath);
            reader.NextTokenIsEndObject();
            iconNormal = LoadSprite(spriteNormalPath);
            iconSelected = LoadSprite(spriteSelectedPath);
            
            reader.NextPropertyValue("solid", out isSolid);
            reader.NextPropertyValue("fluid", out isFluid);
            reader.NextPropertyValue("soil", out isSoil);

            m_FaceTextureNames = new string[6];
            reader.NextPropertyNameIs("faces");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("top", out m_FaceTextureNames[0]);
            reader.NextPropertyValue("bottom", out m_FaceTextureNames[1]);
            reader.NextPropertyValue("front", out m_FaceTextureNames[2]);
            reader.NextPropertyValue("back", out m_FaceTextureNames[3]);
            reader.NextPropertyValue("left", out m_FaceTextureNames[4]);
            reader.NextPropertyValue("right", out m_FaceTextureNames[5]);
            reader.NextTokenIsEndObject();

        }

        public TextureType GetTextureType(BlockFace face)
        {
            var f = (int)face;
            var textureTypeName = m_FaceTextureNames[f];
            var config = Configuration.GetInstance();
            return config.GetTextureType(textureTypeName);
        }

        public override bool Equals(object obj)
        {
            if (obj is BlockType bt)
                return id == bt.id;
            
            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "BlockType[Id=" + id + ", Name=" + name + ", Solid=" + isSoil + ", Fluid=" + isFluid + ", Soil=" +
                   isSoil + ", Empty=" + isEmpty + "]";
        }

        private Sprite LoadSprite(string path)
        {
            if (path != null)
            {
                var icon = Resources.Load<Sprite>(path);
                if (icon == null)
                    Debug.LogWarning("Sprite not found: " + path);
                return icon;
            }
            return null;
        }
    }
}