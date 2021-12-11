using System.Collections.Generic;
using Blox.CommonNS;
using Newtonsoft.Json;

namespace Blox.ConfigurationNS
{
    public class CreatableType : EntityType
    {
        public class Ingredient
        {
            public readonly EntityType entityType;
            public readonly int count;

            public Ingredient(JsonTextReader reader, Configuration configuration)
            {
                reader.NextPropertyValue("entityId", out int id);
                entityType = configuration.GetEntityType<EntityType>(id);
                reader.NextPropertyValue("count", out count);
            }
        }

        public readonly List<Ingredient> ingredients;
        public readonly float duration;
        public readonly int resultCount;
        
        protected CreatableType(JsonTextReader reader, Configuration configuration) : base(reader, configuration)
        {
            ingredients = new List<Ingredient>();
            reader.ForEachObject("ingredients", index =>
            {
                var ingredient = new Ingredient(reader, configuration);
                ingredients.Add(ingredient);
            });
            reader.NextPropertyValue("duration", out duration);
            reader.NextPropertyValue("resultCount", out resultCount);
        }
    }
}