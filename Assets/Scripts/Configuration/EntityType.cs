using Blox.CommonNS;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.ConfigurationNS
{
    public abstract class EntityType
    {
        public readonly int id;
        public readonly string name;
        public readonly Sprite icon;

        protected EntityType(JsonTextReader reader)
        {
            reader.NextPropertyValue("id", out id);
            reader.NextPropertyValue("name", out name);
            reader.NextPropertyValue("icon", out icon);
        }

        public override bool Equals(object obj)
        {
            if (obj is EntityType entityType)
                return id == entityType.id;

            return false;
        }

        public override int GetHashCode()
        {
            return id;
        }
    }
}