/*
 *  Comment :
 */

using System;
using UnityEngine;

namespace RapidFramework
{
    //Life Cycle
    public partial class JsonService 
    {
        public void Initialize() {}
    }
    //Logic
    public partial class JsonService 
    {
        public string LoadRawJson(string jsonPath)
        {
            var asset = Resources.Load<TextAsset>(jsonPath);

            if (asset.IsNull($"Asset not found at path: {jsonPath}"))
                return string.Empty;

            return asset.text;
        }

        public T[] LoadJsonArray<T>(string jsonPath)
        {
            string jsonText = LoadRawJson(jsonPath);

            if (string.IsNullOrEmpty($"LoadJsonArray: json is null or empty at path: {jsonPath}"))
                return Array.Empty<T>();

            return FromJsonArray<T>(jsonText);
        }

        public string ToJson<T>(T obj, bool prettyPrint = false)
            => JsonExtension.ToJson(obj, prettyPrint);

        public string ToJsonArray<T>(T[] array, bool prettyPrint = false)
            => JsonExtension.ToJsonArray<T>(array, prettyPrint);

        public T FromJson<T>(string json)
            => JsonExtension.FromJson<T>(json);

        public T[] FromJsonArray<T>(string json)
            => JsonExtension.FromJsonArray<T>(json);

     
    }
}
