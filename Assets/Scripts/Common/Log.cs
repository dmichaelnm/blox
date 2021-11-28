using System;
using JetBrains.Annotations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Blox.CommonNS
{
    public static class Log
    {
        public enum Level
        {
            None,
            Error,
            Warning,
            Info,
            Debug
        }

        public static Level LogLevel = Level.Debug;

        public static void Error([NotNull] object source, [NotNull] string message, float duration = -1f)
        {
            Message(Level.Error, source, message, duration);
        }

        public static void Warning([NotNull] object source, [NotNull] string message, float duration = -1f)
        {
            Message(Level.Warning, source, message, duration);
        }

        public static void Info([NotNull] object source, [NotNull] string message, float duration = -1f)
        {
            Message(Level.Info, source, message, duration);
        }

        public static void Info([NotNull] object source, [NotNull] string message, [NotNull] Action action)
        {
            var start = Time.realtimeSinceStartup;
            action();
            var duration = (Time.realtimeSinceStartup - start) * 1000f;
            Info(source, message, duration);
        }

        public static void Debug([NotNull] object source, [NotNull] string message, float duration = -1f)
        {
            Message(Level.Debug, source, message, duration);
        }

        public static void Message(Level level, [NotNull] object source, [NotNull] string message, float duration = -1f)
        {
            if (level <= LogLevel)
            {
                var text = ToMessage(level, source, message, duration);

                var context = source as Object;
                if (context != null)
                {
                    if (level == Level.Error)
                        UnityEngine.Debug.LogError(text, context);
                    else if (level == Level.Warning)
                        UnityEngine.Debug.LogWarning(text, context);
                    else
                        UnityEngine.Debug.Log(text, context);
                }
                else
                {
                    if (level == Level.Error)
                        UnityEngine.Debug.LogError(text);
                    else if (level == Level.Warning)
                        UnityEngine.Debug.LogWarning(text);
                    else
                        UnityEngine.Debug.Log(text);
                }
            }
        }

        public static string ToError([NotNull] object source, [NotNull] string message)
        {
            return ToMessage(Level.Error, source, message);
        }

        public static string ToMessage(Level level, [NotNull] object source, [NotNull] string message,
            float duration = -1f)
        {
            var levelColor = "#FFFFFF";
            if (level == Level.Error)
                levelColor = "#FF0000";
            else if (level == Level.Warning)
                levelColor = "#FFFF00";
            else if (level == Level.Debug)
                levelColor = "#7FFFFF";

            var text = $"<color={levelColor}>[{level}]</color> <color=#7F7FFF>[{source}]</color> : {message}";
            if (duration >= 0f)
                text += $" <color=#7FFF7F>({duration}ms)</color>";

            return text;
        }
    }
}