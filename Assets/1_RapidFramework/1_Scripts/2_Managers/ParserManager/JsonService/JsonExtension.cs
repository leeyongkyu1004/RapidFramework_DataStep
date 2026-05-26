/*
 *  Comment :
 */

using System;
using Newtonsoft.Json;
using UnityEngine;


namespace RapidFramework
{
    //Logic
    public static partial class JsonExtension
    {
        public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Converters = { new ObservableConverter() }
        };
        
        public static string ToJson<T>(this T obj, bool prettyPrint = false)
        {
            if (obj == null)
                return "{}";

            try
            {
                var settings = DefaultSettings;
                settings.Formatting = prettyPrint
                    ? Formatting.Indented 
                    : Formatting.None;

                return JsonConvert.SerializeObject(obj, settings);
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"ToJson Failed: {ex.Message}");
                return "{}";
            }                
        }
        public static string ToJsonArray<T>(this T[] array, bool prettyPrint = false)
        {
            if (array == null || array.Length == 0)
                return "[]";

            try
            {
                var settings = DefaultSettings;
                settings.Formatting = prettyPrint 
                    ? Formatting.Indented 
                    : Formatting.None;

                return JsonConvert.SerializeObject(array, settings);
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"Newtonsoft Serialize Failed: {ex.Message}");
                return "[]";
            }
        }
        public static T FromJson<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            try
            {
                if (!TryResolvePlainJson(json, out string plainJson))
                    return default;

                return JsonConvert.DeserializeObject<T>(plainJson, DefaultSettings);
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"FromJson Failed: {ex.Message}");
                return default;
            }
        }

        public static T[] FromJsonArray<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
                return Array.Empty<T>();
            
            try
            {
                if (!TryResolvePlainJson(json, out string plainJson))
                    return Array.Empty<T>();

                return JsonConvert.DeserializeObject<T[]>(plainJson, DefaultSettings) ?? Array.Empty<T>();
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"FromJsonArray Failed: {ex.Message}");
                return Array.Empty<T>();
            }
        }        

        private static bool TryResolvePlainJson(string raw, out string plainJson)
        {
            plainJson = string.Empty;

            if (string.IsNullOrEmpty(raw))
                return false;

            string trimmed = raw.TrimStart();

            if (trimmed.StartsWith("{", StringComparison.Ordinal) || 
                trimmed.StartsWith("[", StringComparison.Ordinal))
            {
                plainJson = raw;
                return true;
            }
            return !string.IsNullOrEmpty(plainJson);
        }
    }
}
