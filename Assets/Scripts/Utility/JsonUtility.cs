using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Blox.Utility
{
    public static class JsonUtility
    {
        public static bool Next(this JsonTextReader reader, bool exceptionOnFail = false)
        {
            if (reader.Read())
                return true;

            if (exceptionOnFail)
                throw new JsonException("Nothing left to read.");

            return false;
        }

        public static bool CurrentTokenIs(this JsonTextReader reader, JsonToken expectedToken,
            bool exceptionOnFail = true)
        {
            if (reader.TokenType == expectedToken)
                return true;

            if (exceptionOnFail)
                throw new JsonException("Unexpected Token (expected: " + expectedToken + "; found: " +
                                        reader.TokenType + ") in " + reader.GetContext());

            return false;
        }

        public static bool NextTokenIs(this JsonTextReader reader, JsonToken expectedToken, bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentTokenIs(expectedToken, exceptionOnFail);

            return false;
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

        public static bool CurrentPropertyNameIs(this JsonTextReader reader, [NotNull] string expectedPropertyName,
            bool exceptionOnFail)
        {
            if (reader.CurrentPropertyName(out var propertyName, exceptionOnFail))
            {
                if (expectedPropertyName.Equals(propertyName))
                    return true;
                if (exceptionOnFail)
                    throw new JsonException("Unexpected property (expected: " + expectedPropertyName + ", found: " +
                                            propertyName + ") in " + reader.GetContext());
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

        public static bool NextPropertyNameIs(this JsonTextReader reader, [NotNull] string expectedPropertyName,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentPropertyNameIs(expectedPropertyName, exceptionOnFail);

            return false;
        }

        public static bool CurrentValue<T>(this JsonTextReader reader, out T result, T defaultResult = default,
            bool exceptionOnFail = true)
        {
            result = defaultResult;

            var value = reader.Value;
            if (value != null)
            {
                if (typeof(T) == typeof(object) ||
                    reader.CurrentTokenIs(JsonToken.Boolean, false) && typeof(T) == typeof(bool) ||
                    reader.CurrentTokenIs(JsonToken.String, false) && typeof(T) == typeof(string))
                {
                    result = (T)value;
                    return true;
                }

                if (reader.CurrentTokenIs(JsonToken.Float, false) && typeof(T) == typeof(float))
                {
                    var lv = (double)value;
                    var cv = (float)lv;
                    var ov = (object)cv;
                    var tv = (T)ov;
                    result = tv;
                    return true;
                }
                
                if (reader.CurrentTokenIs(JsonToken.Integer, false) &&
                    (typeof(T) == typeof(int) || typeof(T) == typeof(float)))
                {
                    if (typeof(T) == typeof(float))
                    {
                        var lv = (long)value;
                        var cv = (float)lv;
                        var ov = (object)cv;
                        var tv = (T)ov;
                        result = tv;
                    }
                    else
                        result = (T)(object)Convert.ToInt32((long)value);

                    return true;
                }

                if (exceptionOnFail)
                    throw new JsonException("Unexpected data type (expected: " + typeof(T) + ", found: " +
                                            value.GetType() + ") in " + reader.GetContext());
            }
            else
                return true;

            return false;
        }

        public static bool NextValue<T>(this JsonTextReader reader, out T result, T defaultResult = default,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentValue(out result, defaultResult, exceptionOnFail);

            result = defaultResult;
            return false;
        }

        public static bool CurrentPropertyValue<T>(this JsonTextReader reader, string expectedPropertyName,
            out T result, T defaultResult = default, bool exceptionOnFail = true)
        {
            if (reader.CurrentPropertyNameIs(expectedPropertyName, exceptionOnFail))
                return reader.NextValue(out result, defaultResult, exceptionOnFail);

            result = defaultResult;
            return false;
        }

        public static bool NextPropertyValue<T>(this JsonTextReader reader, string expectedPropertyName,
            out T result, T defaultResult = default, bool exceptionOnFail = true)
        {
            if (reader.NextPropertyNameIs(expectedPropertyName, exceptionOnFail))
                return reader.NextValue(out result, defaultResult, exceptionOnFail);

            result = defaultResult;
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

        public static bool CurrentTokenIsStartArray(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.StartArray, exceptionOnFail);
        }

        public static bool CurrentTokenIsEndArray(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.EndArray, exceptionOnFail);
        }

        public static bool NextTokenIsStartObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.NextTokenIs(JsonToken.StartObject, exceptionOnFail);
        }

        public static bool NextTokenIsEndObject(this JsonTextReader reader, bool exceptionOnFail = false)
        {
            return reader.NextTokenIs(JsonToken.EndObject, exceptionOnFail);
        }

        public static bool NextTokenIsStartArray(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.NextTokenIs(JsonToken.StartArray, exceptionOnFail);
        }

        public static bool NextTokenIsEndArray(this JsonTextReader reader, bool exceptionOnFail = false)
        {
            return reader.NextTokenIs(JsonToken.EndArray, exceptionOnFail);
        }

        public static string GetContext(this IJsonLineInfo reader)
        {
            return "Line " + reader.LineNumber + ", Column " + reader.LinePosition;
        }

        public static T GetNextPropertyValue<T>(this JsonTextReader reader, string expectedPropertyName,
            T defaultResult = default)
        {
            reader.NextPropertyValue(expectedPropertyName, out var value, defaultResult);
            return value;
        }
    }
}