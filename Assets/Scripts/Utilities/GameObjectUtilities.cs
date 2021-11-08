using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Blox.UtilitiesNS
{
    /// <summary>
    /// This utility class provides several helper methods regarding the game object class.
    /// </summary>
    public static class GameObjectUtilities
    {
        /// <summary>
        /// Returns the child object with the given name. If the child object does not exist and the parameter create
        /// is true, a new child object is created.
        /// </summary>
        /// <param name="obj">The game object</param>
        /// <param name="name">The name of the child object</param>
        /// <param name="create">If true, a child object is created if it does not exists yet</param>
        /// <returns>The child object</returns>
        public static GameObject GetChild(this GameObject obj, [NotNull] string name, bool create = false)
        {
            for (var i = 0; i < obj.transform.childCount; i++)
            {
                var child = obj.transform.GetChild(i).gameObject;
                if (child.name.Equals(name))
                    return child;
            }

            if (create)
            {
                var child = new GameObject(name);
                child.transform.parent = obj.transform;
                return child;
            }

            return null;
        }

        /// <summary>
        /// Returns the component of the type T from the game object. If the component does not exist, it will be created.
        /// </summary>
        /// <param name="obj">The game object</param>
        /// <typeparam name="T">The type of the component</typeparam>
        /// <returns>The component itself</returns>
        public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
        {
            var component = obj.GetComponent<T>();
            if (component == null)
                component = obj.AddComponent<T>();
            return component;
        }

        /// <summary>
        /// Iterates over all child objects of the game object and calls an action method for every entry.
        /// </summary>
        /// <param name="obj">The game object</param>
        /// <param name="action">The action method</param>
        public static void Iterate(this GameObject obj, Action<GameObject> action)
        {
            for (var i = 0; i < obj.transform.childCount; i++)
                action.Invoke(obj.transform.GetChild(i).gameObject);
        }
        
        /// <summary>
        /// Iterates over all child transforms of this transform from the last to the first child and calls for evere
        /// child the action method.
        /// </summary>
        /// <param name="transform">The transform object</param>
        /// <param name="action">The action method</param>
        public static void IterateInverse(this Transform transform, Action<Transform> action)
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                action.Invoke(transform.GetChild(i));
        }
    }
}