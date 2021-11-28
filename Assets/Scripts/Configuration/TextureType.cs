using Blox.CommonNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.ConfigurationNS
{
    public class TextureType
    {
        public readonly string name;
        public readonly Material material;

        public TextureType(JsonTextReader reader)
        {
            reader.NextPropertyValue("name", out name);
            reader.NextPropertyValue("shader", out string shaderName);
            var shader = Shader.Find(shaderName);
            if (shader == null)
                throw new JsonException(Log.ToError(this,
                    $"No shader found for \"{shaderName}\" at {reader.GetContext()}."));
            material = new Material(shader);
            reader.ForEachProperty("properties", (index, propertyName, value) =>
            {
                if (value is float floatValue)
                    material.SetFloat(propertyName, floatValue);
                else if (value is Vector2 vector2)
                    material.SetVector(propertyName, vector2);
                else if (value is Vector3 vector3)
                    material.SetVector(propertyName, vector3);
                else if (value is Vector4 vector4)
                    material.SetVector(propertyName, vector4);
                else if (value is Color color)
                    material.SetColor(propertyName, color);
                else if (value is Texture2D texture)
                    material.SetTexture(propertyName, texture);
            });
        }

        public override bool Equals(object obj)
        {
            if (obj is TextureType textureType)
                return name.Equals(textureType.name);

            return false;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return $"TextureType(Name={name})";
        }
    }
}