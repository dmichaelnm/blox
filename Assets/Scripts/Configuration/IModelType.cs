using UnityEngine;

namespace Blox.ConfigurationNS
{
    public interface IModelType :  IEntityType
    {
        public Mesh mesh { get; }
        public Material material { get; }
    }
}