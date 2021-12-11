using Blox.CommonNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.ConfigurationNS
{
    public class CreatableModelType : CreatableType, IModelType
    {
        public enum Rotation
        {
            North,
            East,
            South,
            West
        }
        
        public  Mesh mesh { get; }
        public  Material material { get; }
        
        public CreatableModelType(JsonTextReader reader, Configuration configuration) : base(reader, configuration)
        {
            reader.NextPropertyValue("model", out string modelPath);
            mesh = Resources.Load<Mesh>(modelPath);
            material = Resources.Load<Material>(modelPath);
        }
    }
}