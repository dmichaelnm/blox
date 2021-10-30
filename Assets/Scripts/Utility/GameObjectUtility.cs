using JetBrains.Annotations;
using UnityEngine;

namespace Blox.Utility
{
    public static class GameObjectUtility
    {
        public static GameObject GetChildObject(this GameObject parent, [NotNull] string name, bool create = false)
        {
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i).gameObject;
                if (child.name.Equals(name))
                    return child;
            }

            if (create)
            {
                var child = new GameObject(name);
                child.transform.parent = parent.transform;
                return child;
            }

            return null;
        }

        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            var comp = obj.GetComponent<T>();
            if (comp == null)
                comp = obj.AddComponent<T>();
            return comp;
        }
    }
}