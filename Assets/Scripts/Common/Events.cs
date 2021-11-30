using Blox.ConfigurationNS;
using Blox.UserInterfaceNS.CraftingWindow;
using UnityEngine;

namespace Blox.CommonNS
{
    public static class Events
    {
        public delegate void ComponentEvent<in T>(T component) where T : Component;

        public delegate void ComponentBoolEvent<in T>(T component, bool value) where T : Component;

        public delegate void ComponentArgsEvent<in T, in E>(T component, E eventArgs)
            where T : Component where E : struct;

        public delegate void CraftingQueueEvent(CraftingQueue component, CreatableType creatable);
    }
}