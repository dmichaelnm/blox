using System;
using System.Globalization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace Blox.CommonNS
{
    public static class JsonExtension
    {
        public static string GetContext(this IJsonLineInfo lineInfo)
        {
            return $"line {lineInfo.LineNumber}, column {lineInfo.LinePosition}";
        }

        public static bool Next(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            if (reader.Read())
                return true;

            if (exceptionOnFail)
                throw new JsonException(Log.ToError(reader, $"Nothing left to read at {reader.GetContext()} ."));

            return false;
        }

        public static bool CurrentTokenIs(this JsonTextReader reader, JsonToken expected, bool exceptionOnFail = true)
        {
            var current = reader.TokenType;
            if (current == expected)
                return true;

            if (exceptionOnFail)
                throw new JsonException(Log.ToError(reader,
                    $"[{expected}] expected but [{current}] found at {reader.GetContext()}."));

            return false;
        }

        public static bool CurrentTokenIsStartObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.StartObject, exceptionOnFail);
        }

        public static bool CurrentTokenIsEndObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.EndObject, exceptionOnFail);
        }

        public static bool NextTokenIs(this JsonTextReader reader, JsonToken expected, bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentTokenIs(expected, exceptionOnFail);

            return false;
        }

        public static bool NextTokenIsStartObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.NextTokenIs(JsonToken.StartObject, exceptionOnFail);
        }

        public static bool NextTokenIsEndObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.NextTokenIs(JsonToken.EndObject, exceptionOnFail);
        }

        public static bool CurrentPropertyName(this JsonTextReader reader, out string propertyName,
            bool exceptionOnFail = true)
        {
            if (reader.CurrentTokenIs(JsonToken.PropertyName, exceptionOnFail))
            {
                propertyName = reader.Value as string;
                return true;
            }

            propertyName = default;
            return false;
        }

        public static bool CurrentPropertyNameIs(this JsonTextReader reader, [NotNull] string expected,
            bool exceptionOnFail = true)
        {
            if (reader.CurrentPropertyName(out var propertyName, exceptionOnFail))
            {
                if (expected.Equals(propertyName))
                    return true;

                if (exceptionOnFail)
                    throw new JsonException(Log.ToError(reader,
                        $"\"{expected}\" expected but \"{propertyName}\" found at {reader.GetContext()}."));
            }

            return false;
        }

        public static bool NextPropertyName(this JsonTextReader reader, out string propertyName,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentPropertyName(out propertyName, exceptionOnFail);

            propertyName = default;
            return false;
        }

        public static bool NextPropertyNameIs(this JsonTextReader reader, [NotNull] string expected,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentPropertyNameIs(expected, exceptionOnFail);

            return false;
        }

        public static bool CurrentValue<T>(this JsonTextReader reader, out T result, T defaultValue = default,
            bool exceptionOnFail = true)
        {
            result = defaultValue;

            var value = reader.Value;

            // null value
            if (value == null && reader.CurrentTokenIs(JsonToken.Null, false))
                return true;

            // unexpected null value
            if (value == null)
            {
                if (exceptionOnFail)
                    throw new JsonException(Log.ToError(reader, $"Unexpected null value at {reader.GetContext()}"));

                return false;
            }

            var isObject = typeof(T) == typeof(object);

            // bool value
            if ((typeof(T) == typeof(bool) || isObject) && reader.CurrentTokenIs(JsonToken.Boolean, false))
            {
                var boolValue = (bool)value;
                var objectValue = (object)boolValue;
                result = (T)objectValue;
                return true;
            }

            // int value
            if ((typeof(T) == typeof(int) || isObject) && reader.CurrentTokenIs(JsonToken.Integer, false))
            {
                var longValue = (long)value;
                var intValue = (int)longValue;
                var objectValue = (object)intValue;
                result = (T)objectValue;
                return true;
            }

            // float value
            if ((typeof(T) == typeof(float) || isObject) &&
                (reader.CurrentTokenIs(JsonToken.Float, false) ||
                 reader.CurrentTokenIs(JsonToken.Integer, false)))
            {
                if (reader.CurrentTokenIs(JsonToken.Float, false))
                {
                    var doubleValue = (double)value;
                    var floatValue = (float)doubleValue;
                    var objectValue = (object)floatValue;
                    result = (T)objectValue;
                    return true;
                }
                else
                {
                    var longValue = (long)value;
                    var floatValue = (float)longValue;
                    var objectValue = (object)floatValue;
                    result = (T)objectValue;
                    return true;
                }
            }

            // string value
            if (value is string str && reader.CurrentTokenIs(JsonToken.String, false))
            {
                // texture path
                if (str.StartsWith("@") && (typeof(T) == typeof(Texture2D) || isObject))
                {
                    var path = str.Substring(1);
                    var asset = Resources.Load<Texture2D>(path);
                    if (asset != null)
                    {
                        result = (T)(object)asset;
                        return true;
                    }

                    if (exceptionOnFail)
                        throw new JsonException(Log.ToError(reader,
                            $"No texture asset found at \"{path}\" at {reader.GetContext()}."));

                    return false;
                }

                // sprite path
                if (str.StartsWith("&") && (typeof(T) == typeof(Sprite) || isObject))
                {
                    var path = str.Substring(1);
                    var asset = Resources.Load<Sprite>(path);
                    if (asset != null)
                    {
                        result = (T)(object)asset;
                        return true;
                    }

                    if (exceptionOnFail)
                        throw new JsonException(Log.ToError(reader,
                            $"No sprite asset found at \"{path}\" at {reader.GetContext()}."));

                    return false;
                }

                // color
                if (str.StartsWith("#") && (typeof(T) == typeof(Color) || isObject))
                {
                    var success = str.Length == 7 || str.Length == 9;
                    success &= Parse(str, 1, out var r);
                    success &= Parse(str, 3, out var g);
                    success &= Parse(str, 5, out var b);
                    success &= Parse(str, 7, out var a);
                    if (success)
                    {
                        result = (T)(object)new Color(r, g, b, a);
                        return true;
                    }

                    if (exceptionOnFail)
                        throw new JsonException(Log.ToError(reader,
                            $"\"{str}\" is not a valid color type at {reader.GetContext()}."));

                    return false;

                    bool Parse(string s, int index, out float val)
                    {
                        if (index >= s.Length)
                        {
                            val = 1f;
                            return true;
                        }

                        var res = int.TryParse(s.Substring(index, 2), NumberStyles.HexNumber,
                            CultureInfo.InvariantCulture, out var v);
                        val = res ? v / 255f : default;
                        return res;
                    }
                }

                // vector
                if (str.StartsWith("[") && str.EndsWith("]"))
                {
                    var parts = str.Substring(1, str.Length - 2).Split(',');
                    var success = true;

                    // vector 2
                    if ((typeof(T) == typeof(Vector2) || isObject) && parts.Length == 2)
                    {
                        success &= Parse(parts[0], out var x);
                        success &= Parse(parts[1], out var y);
                        if (success)
                        {
                            result = (T)(object)new Vector2(x, y);
                            return true;
                        }
                    }

                    // vector 3
                    if ((typeof(T) == typeof(Vector3) || isObject) && parts.Length == 3)
                    {
                        success &= Parse(parts[0], out var x);
                        success &= Parse(parts[1], out var y);
                        success &= Parse(parts[2], out var z);
                        if (success)
                        {
                            result = (T)(object)new Vector3(x, y, z);
                            return true;
                        }
                    }

                    // vector 4
                    if ((typeof(T) == typeof(Vector4) || isObject) && parts.Length == 4)
                    {
                        success &= Parse(parts[0], out var x);
                        success &= Parse(parts[1], out var y);
                        success &= Parse(parts[2], out var z);
                        success &= Parse(parts[3], out var w);
                        if (success)
                        {
                            result = (T)(object)new Vector4(x, y, z, w);
                            return true;
                        }
                    }

                    if (exceptionOnFail)
                        throw new JsonException(Log.ToError(reader,
                            $"\"{str}\" is not a valid <{typeof(T)}> type at {reader.GetContext()}."));

                    return false;

                    bool Parse(string part, out float val)
                    {
                        return float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out val);
                    }
                }

                // enum
                if (typeof(T).IsEnum)
                {
                    result = (T)Enum.Parse(typeof(T), str, true);
                    return true;
                }

                // normal string
                result = (T)value;
                return true;
            }

            if (exceptionOnFail)
                throw new JsonException(Log.ToError(reader,
                    $"<{typeof(T)}> expected but <{value.GetType()}> found at {reader.GetContext()}."));

            return false;
        }

        public static bool NextValue<T>(this JsonTextReader reader, out T result, T defaultValue = default,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentValue(out result, defaultValue, exceptionOnFail);

            result = defaultValue;
            return false;
        }

        public static bool CurrentPropertyValue<T>(this JsonTextReader reader, string expected, out T result,
            T defaultValue = default, bool exceptionOnFail = true)
        {
            if (reader.CurrentPropertyNameIs(expected, exceptionOnFail))
                return reader.NextValue(out result, defaultValue, exceptionOnFail);

            result = defaultValue;
            return false;
        }

        public static bool NextPropertyValue<T>(this JsonTextReader reader, string expected, out T result,
            T defaultValue = default, bool exceptionOnFail = true)
        {
            if (reader.NextPropertyNameIs(expected, exceptionOnFail))
                return reader.NextValue(out result, defaultValue, exceptionOnFail);

            result = defaultValue;
            return false;
        }

        public static void ForEachObject(this JsonTextReader reader, string expected, Action<int> iterator)
        {
            var index = 0;
            reader.NextPropertyNameIs(expected);
            reader.NextTokenIs(JsonToken.StartArray);
            while (!reader.NextTokenIs(JsonToken.EndArray, false))
            {
                reader.CurrentTokenIsStartObject();
                iterator(index);
                index++;
                reader.NextTokenIsEndObject();
            }
        }

        public static void ForEachProperty(this JsonTextReader reader, string expected,
            Action<int, string, object> iterator)
        {
            var index = 0;
            reader.NextPropertyNameIs(expected);
            reader.NextTokenIsStartObject();
            while (!reader.NextTokenIsEndObject(false))
            {
                reader.CurrentPropertyName(out var propertyName);
                reader.NextValue(out object result);
                iterator(index, propertyName, result);
                index++;
            }
        }

        public static void WriteProperty(this JsonTextWriter writer, [NotNull] string propertyName, object value)
        {
            writer.WritePropertyName(propertyName);
            if (value is Vector2 v2)
                writer.WriteValue("["
                                  + v2.x.ToString(CultureInfo.InvariantCulture)
                                  + ", "
                                  + v2.y.ToString(CultureInfo.InvariantCulture)
                                  + "]"
                );
            else if (value is Vector3 v3)
                writer.WriteValue("["
                                  + v3.x.ToString(CultureInfo.InvariantCulture)
                                  + ", "
                                  + v3.y.ToString(CultureInfo.InvariantCulture)
                                  + ", "
                                  + v3.z.ToString(CultureInfo.InvariantCulture)
                                  + "]"
                );
            else
                writer.WriteValue(value);
        }
    }
}