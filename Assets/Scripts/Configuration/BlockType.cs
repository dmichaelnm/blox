using System;
using System.Collections.Generic;
using Blox.CommonNS;
using Newtonsoft.Json;

namespace Blox.ConfigurationNS
{
    public class BlockType : EntityType
    {
        public enum ID
        {
            Air = 0,
            Ground = 1,
            Grass = 2,
            Water = 3,
            Stone = 4,
            SnowedStone = 5,
            Trunk = 6,
            Leaves = 7,
            Sand = 8,
            Redstone = 9
        }

        public readonly int itemId;
        public readonly bool isSolid;
        public readonly bool isFluid;
        public readonly bool isSoil;
        
        public bool isEmpty => id == (int)ID.Air;
        public TextureType this[BlockFace face] => m_FaceTextures.ContainsKey(face) ? m_FaceTextures[face] : null;
        public BlockType ItemBlockType => Configuration.GetInstance().GetEntityType<BlockType>(itemId);

        private readonly Dictionary<BlockFace, TextureType> m_FaceTextures;

        public BlockType(JsonTextReader reader, Configuration configuration) : base(reader)
        {
            m_FaceTextures = new Dictionary<BlockFace, TextureType>();

            reader.NextPropertyValue("itemId", out itemId);
            reader.NextPropertyValue("isSolid", out isSolid);
            reader.NextPropertyValue("isFluid", out isFluid);
            reader.NextPropertyValue("isSoil", out isSoil);

            reader.ForEachProperty("faceTextures", (index, propertyName, value) =>
            {
                if (!Enum.TryParse(propertyName, true, out BlockFace face))
                    throw new JsonException(Log.ToError(this,
                        $"Invalid face name \"{propertyName}\" found at {reader.GetContext()}."));

                if (value != null)
                {
                    if (!(value is string textureName))
                        throw new JsonException(Log.ToError(this, $"String value expected at {reader.GetContext()}."));

                    var textureType = configuration.GetTextureType(textureName);
                    if (textureType == null)
                        throw new JsonException(
                            Log.ToError(this,
                                $"Unknown texture type name \"{textureName}\" at {reader.GetContext()}."));

                    m_FaceTextures.Add(face, textureType);
                }
            });
        }
    }
}