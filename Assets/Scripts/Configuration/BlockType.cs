using System;
using System.Collections.Generic;
using Blox.CommonNS;
using Newtonsoft.Json;

namespace Blox.ConfigurationNS
{
    public class BlockType : EntityType, IBlockType
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

        public bool isSolid { get; }
        public bool isFluid { get; }
        public bool isSoil { get; }
        public bool isEmpty => id == (int)ID.Air;

        private readonly Dictionary<BlockFace, TextureType> m_FaceTextures;

        public BlockType(JsonTextReader reader, Configuration configuration) : base(reader, configuration)
        {
            m_FaceTextures = new Dictionary<BlockFace, TextureType>();

            reader.NextPropertyValue("isSolid", out bool _isSolid);
            isSolid = _isSolid;
            
            reader.NextPropertyValue("isFluid", out bool _isFluid);
            isFluid = _isFluid;
            
            reader.NextPropertyValue("isSoil", out bool _isSoil);
            isSoil = _isSoil;

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

        public TextureType GetTextureType(BlockFace face)
        {
            return m_FaceTextures.ContainsKey(face) ? m_FaceTextures[face] : null;
        }
    }
}