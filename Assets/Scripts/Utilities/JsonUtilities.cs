using System;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Blox.UtilitiesNS
{
    /// <summary>
    /// This utility class provides several methods to read from a json text reader instance.
    /// </summary>
    public static class JsonUtilities
    {
        /// <summary>
        /// Reads the next token from the reader.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        /// <exception cref="JsonException">When an error occures and exceptionOnFail is true.</exception>
        public static bool Next(this JsonTextReader reader, bool exceptionOnFail = false)
        {
            if (reader.Read())
                return true;

            if (exceptionOnFail)
                throw new JsonException("Nothing left to read.");

            return false;
        }

        /// <summary>
        /// Checks if the current token is of a specific type.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedToken">The expected token</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        /// <exception cref="JsonException">When an error occures and exceptionOnFail is true.</exception>
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

        /// <summary>
        /// Checks if the next token is of a specific type.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedToken">The expected token</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextTokenIs(this JsonTextReader reader, JsonToken expectedToken, bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentTokenIs(expectedToken, exceptionOnFail);

            return false;
        }

        /// <summary>
        /// Reads the current property name from the reader.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
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

        /// <summary>
        /// Checks if the current property name has a specific name.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedPropertyName">The expected property name</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        /// <exception cref="JsonException">When an error occures and exceptionOnFail is true.</exception>
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

        /// <summary>
        /// Reads the next property name from the reader.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="propertyName">The property name</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextPropertyName(this JsonTextReader reader, out string propertyName,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentPropertyName(out propertyName, exceptionOnFail);

            propertyName = default;
            return false;
        }

        /// <summary>
        /// Checks if the next property name has a specific name.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedPropertyName">The expected property name</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        /// <exception cref="JsonException">When an error occures and exceptionOnFail is true.</exception>
        public static bool NextPropertyNameIs(this JsonTextReader reader, [NotNull] string expectedPropertyName,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentPropertyNameIs(expectedPropertyName, exceptionOnFail);

            return false;
        }

        /// <summary>
        /// Reads the value of the current token.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="result">The value of the property</param>
        /// <param name="defaultResult">A default property value</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <typeparam name="T">The type of the property (object, bool, int, float, string)</typeparam>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        /// <exception cref="JsonException">When an error occures and exceptionOnFail is true.</exception>
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

        /// <summary>
        /// Reads the value of the next token.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="result">The value of the property</param>
        /// <param name="defaultResult">A default property value</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <typeparam name="T">The type of the property (object, bool, int, float, string)</typeparam>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextValue<T>(this JsonTextReader reader, out T result, T defaultResult = default,
            bool exceptionOnFail = true)
        {
            if (reader.Next(exceptionOnFail))
                return reader.CurrentValue(out result, defaultResult, exceptionOnFail);

            result = defaultResult;
            return false;
        }

        /// <summary>
        /// Reads the value of the current property. The property must have an expected name. 
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedPropertyName">The expected name of the property</param>
        /// <param name="result">The value of the property</param>
        /// <param name="defaultResult">A default property value</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <typeparam name="T">The type of the property (object, bool, int, float, string)</typeparam>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool CurrentPropertyValue<T>(this JsonTextReader reader, string expectedPropertyName,
            out T result, T defaultResult = default, bool exceptionOnFail = true)
        {
            if (reader.CurrentPropertyNameIs(expectedPropertyName, exceptionOnFail))
                return reader.NextValue(out result, defaultResult, exceptionOnFail);

            result = defaultResult;
            return false;
        }

        /// <summary>
        /// Reads the value of the next property. The property must have an expected name. 
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedPropertyName">The expected name of the property</param>
        /// <param name="result">The value of the property</param>
        /// <param name="defaultResult">A default property value</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <typeparam name="T">The type of the property (object, bool, int, float, string)</typeparam>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextPropertyValue<T>(this JsonTextReader reader, string expectedPropertyName,
            out T result, T defaultResult = default, bool exceptionOnFail = true)
        {
            if (reader.NextPropertyNameIs(expectedPropertyName, exceptionOnFail))
                return reader.NextValue(out result, defaultResult, exceptionOnFail);

            result = defaultResult;
            return false;
        }

        /// <summary>
        /// Checks if the current token is of type "StartObject".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool CurrentTokenIsStartObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.StartObject, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the current token is of type "EndObject".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool CurrentTokenIsEndObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.EndObject, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the current token is of type "StartArray".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool CurrentTokenIsStartArray(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.StartArray, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the current token is of type "EndObject".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool CurrentTokenIsEndArray(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.CurrentTokenIs(JsonToken.EndArray, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the next token is of type "StartObject".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextTokenIsStartObject(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.NextTokenIs(JsonToken.StartObject, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the next token is of type "EndObject".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextTokenIsEndObject(this JsonTextReader reader, bool exceptionOnFail = false)
        {
            return reader.NextTokenIs(JsonToken.EndObject, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the next token is of type "StartArray".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextTokenIsStartArray(this JsonTextReader reader, bool exceptionOnFail = true)
        {
            return reader.NextTokenIs(JsonToken.StartArray, exceptionOnFail);
        }

        /// <summary>
        /// Checks if the next token is of type "EndArray".
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="exceptionOnFail">if true, an exception is raised when an error occures.</param>
        /// <returns>Returns true if the operation succeeds or false when an error occurs but exceptionOnFail is false.</returns>
        public static bool NextTokenIsEndArray(this JsonTextReader reader, bool exceptionOnFail = false)
        {
            return reader.NextTokenIs(JsonToken.EndArray, exceptionOnFail);
        }

        /// <summary>
        /// Returns a string containing context information like line number and column.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <returns>a string with line number and column</returns>
        public static string GetContext(this IJsonLineInfo reader)
        {
            return "Line " + reader.LineNumber + ", Column " + reader.LinePosition;
        }

        /// <summary>
        /// Returns the value of the next property. The property must have an expected name.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="expectedPropertyName">The expected name of the property</param>
        /// <param name="defaultResult">A default value</param>
        /// <typeparam name="T">The type of the property (object, bool, int, float, string)</typeparam>
        /// <returns>The property value</returns>
        public static T GetNextPropertyValue<T>(this JsonTextReader reader, string expectedPropertyName,
            T defaultResult = default)
        {
            reader.NextPropertyValue(expectedPropertyName, out var value, defaultResult);
            return value;
        }

        /// <summary>
        /// Iterates over a JSON array and calls for every step the action method.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="action">Action method called every iteration step</param>
        public static void IterateOverObjectArray(this JsonTextReader reader, Action action)
        {
            reader.NextTokenIsStartArray();
            while (!reader.NextTokenIsEndArray())
            {
                reader.CurrentTokenIsStartObject();
                action.Invoke();
                reader.NextTokenIsEndObject();
            }
        }

        /// <summary>
        /// Iterates over all properties of the current JSON object and calls for every property the action method.
        /// </summary>
        /// <param name="reader">JSON text reader</param>
        /// <param name="action">Action method called every iteration step</param>
        public static void IterateOverProperties(this JsonTextReader reader, Action<string, JsonToken, object> action)
        {
            reader.NextTokenIsStartObject();
            while (!reader.NextTokenIsEndObject())
            {
                reader.CurrentPropertyName(out var propertyName);
                reader.NextValue(out object value);
                var token = reader.TokenType;
                action.Invoke(propertyName, token, value);
            }
        }
    }
}