using JetBrains.Annotations;
using UnityEngine;

namespace Blox.CommonNS
{
    public static class GameObjectExtension
    {
        [CanBeNull]
        public static GameObject GetChild(this GameObject parent, [NotNull] string name, bool create = false)
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
                child.transform.localPosition = Vector3.zero;
                return child;
            }

            return null;
        }

        public static T GetOrAddComponent<T>(this GameObject parent) where T : Component
        {
            var comp = parent.GetComponent<T>();
            if (comp == null)
                comp = parent.AddComponent<T>();
            return comp;
        }

        public static void RemoveChildren(this GameObject parent)
        {
            for (var i = parent.transform.childCount - 1; i >= 0; i--)
            {
                var child = parent.transform.GetChild(i).gameObject;
                child.RemoveChildren();
                Object.Destroy(child);
            }
        }
    }
}