using Blox.UtilitiesNS;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blox.ConfigurationNS
{
    /// <summary>
    /// This class contains information for a specific block type.
    /// </summary>
    public class BlockType
    {
        /// <summary>
        /// This enumeration contains the IDs of the default blocks.
        /// </summary>
        public enum IDs
        {
            Air = 0,
            Ground = 1,
            Grass = 2,
            Wood = 3,
            Leaves = 4,
            Leaves2 = 5,
            Stone = 6,
            SnowedStone = 7,
            Sand = 8,
            Water = 9,
            Snow = 10
        }
        
        /// <summary>
        /// The ID of this block type.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// The base ID of this block.
        /// </summary>
        public readonly int BaseID;

        /// <summary>
        /// The name of this block type.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The sprite for a block icon.
        /// </summary>
        public readonly Sprite Icon;

        /// <summary>
        /// The sprite for a selected block icon.
        /// </summary>
        public readonly Sprite IconSelected;

        /// <summary>
        /// A flag for defining a block as solid or not. 
        /// </summary>
        public readonly bool IsSolid;

        /// <summary>
        /// A flag for defining a block as fluid or not. 
        /// </summary>
        public readonly bool IsFluid;

        /// <summary>
        /// A flag for defining a block as soil or not. 
        /// </summary>
        public readonly bool IsSoil;

        /// <summary>
        /// A flag for defining a block as empty or not. 
        /// </summary>
        public bool IsEmpty => ID == 0;

        /// <summary>
        /// Returns the texture type for the given face.
        /// </summary>
        /// <param name="face">A block face</param>
        public TextureType this[BlockFace face]
        {
            get
            {
                var config = Configuration.GetInstance();
                return config.GetTextureType(m_Faces[(int)face]);
            }
        }

        /// <summary>
        /// An array with a texture type names for each face of this block.
        /// </summary>
        private readonly string[] m_Faces;

        /// <summary>
        /// Creates a new block type and reads it information from the given reader.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        public BlockType(JsonTextReader reader)
        {
            reader.NextPropertyValue("id", out ID);
            reader.NextPropertyValue("baseId", out BaseID);
            reader.NextPropertyValue("name", out Name);
            reader.NextPropertyNameIs("icons");
            reader.NextTokenIsStartObject();
            Icon = GetSprite(reader, "default");
            IconSelected = GetSprite(reader, "selected");
            reader.NextTokenIsEndObject();
            reader.NextPropertyValue("solid", out IsSolid);
            reader.NextPropertyValue("fluid", out IsFluid);
            reader.NextPropertyValue("soil", out IsSoil);

            m_Faces = new string[6];
            reader.NextPropertyNameIs("faces");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("top", out m_Faces[0]);
            reader.NextPropertyValue("bottom", out m_Faces[1]);
            reader.NextPropertyValue("front", out m_Faces[2]);
            reader.NextPropertyValue("back", out m_Faces[3]);
            reader.NextPropertyValue("left", out m_Faces[4]);
            reader.NextPropertyValue("right", out m_Faces[5]);
            reader.NextTokenIsEndObject();
        }

        /// <summary>
        /// Returns a string representation of this block type.
        /// </summary>
        /// <returns>A info string</returns>
        public override string ToString()
        {
            return
                $"BlockType[ID={ID}, BaseID={BaseID}, Name={Name}, IsSolid={IsSolid}, IsFluid={IsFluid}, IsSoil={IsSoil}, IsEmpty={IsEmpty}]";
        }

        /// <summary>
        /// Returns the sprite for the path in the property with the given name.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="propertyName"></param>
        /// <returns>A sprite</returns>
        private Sprite GetSprite(JsonTextReader reader, string propertyName)
        {
            reader.NextPropertyValue(propertyName, out string spritePath);
            if (spritePath != null)
            {
                var sprite = Resources.Load<Sprite>(spritePath);
                Assert.IsNotNull(sprite, $"Sprite [{spritePath}] not found for property [{propertyName}]");
                return sprite;
            }

            return null;
        }
    }
}