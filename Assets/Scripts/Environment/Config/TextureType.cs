using System;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;
using Blox.Utility;

namespace Blox.Environment.Config
{
    public class TextureType
    {
        public readonly int id;
        public readonly string name;
        public readonly Material material;

        public TextureType(JsonTextReader reader)
        {
            reader.NextPropertyValue("id", out id);
            reader.NextPropertyValue("name", out name);

            reader.NextPropertyNameIs("shader");
            reader.NextTokenIsStartObject();
            reader.NextPropertyValue("name", out string shaderName);
            var shader = Shader.Find(shaderName);
            if (shader == null)
                throw new JsonException("Unknown shader name (" + shaderName + ") in " + reader.GetContext());
            material = new Material(shader);
            reader.NextPropertyNameIs("properties");
            reader.NextTokenIsStartObject();
            while (!reader.NextTokenIsEndObject())
            {
                reader.CurrentPropertyName(out var propertyName);
                reader.NextValue(out object value);
                if (value is double doubleValue)
                    material.SetFloat(propertyName, (float)doubleValue);
                else if (value is long longValue)
                    material.SetFloat(propertyName, (float)Convert.ToDouble(longValue));
                else if (value is string stringValue)
                {
                    if (stringValue.Length > 0 && stringValue[0] == '@')
                    {
                        var texturePath = stringValue.Substring(1);
                        //var texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                        var texture2D = Resources.Load<Texture2D>(texturePath);
                        if (texture2D == null)
                            throw new JsonException("Texture2D not found (" + texturePath + ") in " +
                                                    reader.GetContext());
                        material.SetTexture(propertyName, texture2D);
                    }
                    else if (stringValue.Length >= 7 && stringValue[0] == '#')
                    {
                        var red = int.Parse(stringValue.Substring(1, 2), NumberStyles.HexNumber);
                        var green = int.Parse(stringValue.Substring(3, 2), NumberStyles.HexNumber);
                        var blue = int.Parse(stringValue.Substring(5, 2), NumberStyles.HexNumber);
                        var alpha = stringValue.Length == 9
                            ? int.Parse(stringValue.Substring(7, 2), NumberStyles.HexNumber)
                            : 255;
                        material.SetColor(propertyName, new Color(red / 255f, green / 255f, blue / 255f, alpha / 255f));
                    }
                    else if (stringValue.StartsWith("[") && stringValue.EndsWith("]"))
                    {
                        var parts = stringValue.Substring(1, stringValue.Length - 2).Split(',');
                        if (parts.Length < 2 || parts.Length > 4)
                            throw new JsonException("Invalid vector value (" + stringValue + ") in " +
                                                    reader.GetContext());
                        var x = (float)Convert.ToDouble(parts[0], NumberFormatInfo.InvariantInfo);
                        var y = (float)Convert.ToDouble(parts[1], NumberFormatInfo.InvariantInfo);
                        var z = parts.Length >= 3 ? (float)Convert.ToDouble(parts[2]) : 0f;
                        var w = parts.Length == 4 ? (float)Convert.ToDouble(parts[3]) : 0f;
                        material.SetVector(propertyName, new Vector4(x, y, z, w));
                    }
                }
            }

            reader.NextTokenIsEndObject();
        }

        public override bool Equals(object obj)
        {
            if (obj is TextureType tt)
                return id == tt.id;

            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }

        public override string ToString()
        {
            return "TextureType[Id=" + id + ", Name=" + name + "]";
        }
    }
}