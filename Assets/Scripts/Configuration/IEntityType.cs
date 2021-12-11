using UnityEngine;

namespace Blox.ConfigurationNS
{
    public interface IEntityType
    {
        public int id { get; }
        public string name { get; }
        public Sprite icon { get; }
        public int itemId { get; }
    }
}