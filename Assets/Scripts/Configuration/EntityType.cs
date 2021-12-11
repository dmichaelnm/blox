using Blox.CommonNS;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.ConfigurationNS
{
    public abstract class EntityType : IEntityType
    {
        public enum Type
        {
            Block,
            CreatableBlock,
            CreatableModel
        }
        
        public int id { get; }
        public  string name { get; }
        public  Sprite icon { get; }
        public  int itemId { get; }

        public EntityType ItemType => configuration.GetEntityType<EntityType>(itemId);
        
        protected readonly Configuration configuration;
        
        protected EntityType([NotNull] JsonTextReader reader, [NotNull] Configuration configuration)
        {
            this.configuration = configuration;
            
            reader.NextPropertyValue("id", out int _id);
            id = _id;
            
            reader.NextPropertyValue("name", out string _name);
            name = _name;
            
            reader.NextPropertyValue("icon", out Sprite _icon);
            icon = _icon;
            
            reader.NextPropertyValue("itemId", out int _itemId);
            itemId = _itemId;
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