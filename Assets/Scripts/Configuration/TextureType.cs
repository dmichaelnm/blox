using System;
using System.Globalization;
using Blox.UtilitiesNS;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Assertions;

namespace Blox.ConfigurationNS
{
    /// <summary>
    /// This class contains information for a specific texture type.
    /// </summary>
    public class TextureType
    {
        /// <summary>
        /// The ID of this texture type.
        /// </summary>
        public readonly int ID;

        /// <summary>
        /// The name of this texture type.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The material for this texture type.
        /// </summary>
        public readonly Material Material;

        /// <summary>
        /// Creates a new texture type and reads it information from the given reader.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        public TextureType(JsonTextReader reader)
        {
            reader.NextPropertyValue("id", out ID);
            reader.NextPropertyValue("name", out Name);
            reader.NextPropertyNameIs("shader");
            reader.NextTokenIsStartObject();

            // Shader initialization
            reader.NextPropertyValue("name", out string shaderName);
            var shader = Shader.Find(shaderName);
            Assert.IsNotNull(shader, $"Shader [{shaderName}] not found.");

            // Material initialization
            Material = new Material(shader);
            reader.NextPropertyNameIs("properties");
            reader.IterateOverProperties((propertyName, token, value) =>
            {
                if (value is double dValue)
                    Material.SetFloat(propertyName, (float)dValue);
                else if (value is long lValue)
                    Material.SetFloat(propertyName, lValue);
                else if (value is string sValue)
                {
                    // string is a path to texture2D asset
                    if (sValue.StartsWith("@"))
                    {
                        var texturePath = sValue.Substring(1);
                        var texture = Resources.Load<Texture2D>(texturePath);
                        Assert.IsNotNull(texture, $"Texture [{texturePath}] not found.");
                        Material.SetTexture(propertyName, texture);
                    }
                    // string is a color value
                    else if (sValue.StartsWith("#"))
                    {
                        var r = int.Parse(sValue.Substring(1, 2), NumberStyles.HexNumber);
                        var g = int.Parse(sValue.Substring(3, 2), NumberStyles.HexNumber);
                        var b = int.Parse(sValue.Substring(5, 2), NumberStyles.HexNumber);
                        var a = sValue.Length == 9 ? int.Parse(sValue.Substring(7, 2), NumberStyles.HexNumber) : 255;
                        Material.SetColor(propertyName, new Color(r / 255f, g / 255f, b / 255f, a / 255f));
                    }
                    // string is a vector value
                    else if (sValue.StartsWith("[") && sValue.EndsWith("]"))
                    {
                        var vectorParts = sValue.Substring(1, sValue.Length - 2).Split(',');
                        Assert.IsTrue(vectorParts.Length >= 1 && vectorParts.Length <= 4,
                            $"{sValue} is not a valid vector value");
                        switch (vectorParts.Length)
                        {
                            case 1:
                                // This is not a vector just a float value
                                Material.SetFloat(propertyName,
                                    (float)Convert.ToDouble(vectorParts[0], NumberFormatInfo.InvariantInfo));
                                break;
                            case 2:
                                // This is a Vector2
                                Material.SetVector(propertyName,
                                    new Vector4(
                                        (float)Convert.ToDouble(vectorParts[0], NumberFormatInfo.InvariantInfo),
                                        (float)Convert.ToDouble(vectorParts[1], NumberFormatInfo.InvariantInfo)));
                                break;
                            case 3:
                                // This is a Vector3
                                Material.SetVector(propertyName,
                                    new Vector4(
                                        (float)Convert.ToDouble(vectorParts[0], NumberFormatInfo.InvariantInfo),
                                        (float)Convert.ToDouble(vectorParts[1], NumberFormatInfo.InvariantInfo),
                                        (float)Convert.ToDouble(vectorParts[2], NumberFormatInfo.InvariantInfo)));
                                break;
                            case 4:
                                // This is a Vector4
                                Material.SetVector(propertyName,
                                    new Vector4(
                                        (float)Convert.ToDouble(vectorParts[0], NumberFormatInfo.InvariantInfo),
                                        (float)Convert.ToDouble(vectorParts[1], NumberFormatInfo.InvariantInfo),
                                        (float)Convert.ToDouble(vectorParts[2], NumberFormatInfo.InvariantInfo),
                                        (float)Convert.ToDouble(vectorParts[3], NumberFormatInfo.InvariantInfo)));
                                break;
                        }
                    }
                    else
                        throw new Exception($"Undefined string value in property {propertyName}: {value}");
                }
            });
            reader.NextTokenIsEndObject();
        }

        /// <summary>
        /// Compares this texture type with an object. If the object is also a texture type and has the same ID, both
        /// are considered as equal.
        /// </summary>
        /// <param name="obj">Another object</param>
        /// <returns>True if both objects are equal, otherwise false</returns>
        public override bool Equals(object obj)
        {
            if (obj is TextureType textureType)
                return ID == textureType.ID;

            return false;
        }

        /// <summary>
        /// Returns the ID of this texture type as hash code.
        /// </summary>
        /// <returns>ID</returns>
        public override int GetHashCode()
        {
            return ID;
        }

        /// <summary>
        /// Returns a string representation of this texture type.
        /// </summary>
        /// <returns>A info string</returns>
        public override string ToString()
        {
            return $"TextureType[ID={ID}, Name={Name}]";
        }
    }
}