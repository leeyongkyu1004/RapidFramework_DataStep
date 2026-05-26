/*
 *  Comment     : 
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace RapidFramework
{
    public class LocalDataServiceFacade : AssetPostprocessor
    {
        private static bool _isProcessing = false;

        private static void OnPostprocessAllAssets(
            string[] importedAssets, 
            string[] deletedAssets, 
            string[] movedAssets,
            string[] movedFromAssetPaths
        )
        {
            if (_isProcessing)
                return;

            foreach (string assetPath in importedAssets)
            {
                if (Path.GetFileName(assetPath) == "LocalDataService.cs")
                {
                    Generate(assetPath);
                    break;
                }
            }
        }

        [MenuItem("RapidFramework/Force Generate LocalData Providers Facade")]
        public static void ForceGenerate()
        {
            string[] guids = AssetDatabase.FindAssets("LocalDataService t:Script");

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileName(path) == "LocalDataService.cs")
                {
                    Generate(path);
                    return;
                }
            }
        }

        private static void Generate(string path)
        {
            if (!File.Exists(path))
                return;

            string[] lines = File.ReadAllLines(path);
            HashSet<string> processedTypes = new HashSet<string>();
            List<string> fieldNames = new List<string>();

            string pattern = @"RegisterDataProvider\s*<\s*(?<type>\w+)\s*>\s*\(";

            StringBuilder properties = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("//")) 
                    continue;

                Match match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    string typeName = match.Groups["type"].Value;

                    if (typeName == "T" || processedTypes.Contains(typeName))
                        continue;

                    processedTypes.Add(typeName);

                    string propertyName = typeName.EndsWith("DataProvider")
                        ? typeName.Replace("DataProvider", "")
                        : typeName;
                   
                    string fieldName = 
                        $"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";

                    fieldNames.Add(fieldName);

                    properties.AppendLine($"        private {typeName} {fieldName};");
                    properties.AppendLine($"        public {typeName} {propertyName} " +
                        $"=> {fieldName} ??= GetDataProvider<{typeName}>();\n");
                }
            }

            string clearCache = 
                string.Join("\n", fieldNames.ConvertAll(f => $"            {f} = null;"));

            string template = $@"/*
 *  Comment     : 이 코드는 LocalDataServiceFacade에 의해 자동 생성되었습니다.
 */

namespace RapidFramework
{{
    public partial class LocalDataService
    {{
{properties}
        public void ClearCache()
        {{
{clearCache}
        }}
    }}
}}";
            SaveFile(path, template);
        }

        private static void SaveFile(string path, string template)
        {
            template = Regex.Replace(template, @"\r\n|\n\r|\n|\r", Environment.NewLine);

            string directory = Path.GetDirectoryName(path);
            string targetDir = Path.Combine(directory, "Facade").Replace("\\", "/");

            if (!Directory.Exists(targetDir)) 
                Directory.CreateDirectory(targetDir);

            string outputPath = 
                Path.Combine(targetDir, "LocalDataService.Providers.cs").Replace("\\", "/");

            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == template) 
                return;

            try
            {
                _isProcessing = true;
                File.WriteAllText(outputPath, template, Encoding.UTF8);
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"<color=#4AF626>[LocalDataService]</color> Providers 업데이트 완료.");
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}