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
    public class RapidFrameworkFacade : AssetPostprocessor
    {
        private static bool _isProcessing = false;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets, 
            string[] movedAssets, 
            string[] movedFromAssetPaths
            )
        {
            if (_isProcessing) return;

            foreach (string assetPath in importedAssets)
            {
                if (Path.GetFileName(assetPath) == "RapidFramework.cs")
                {
                    Generate(assetPath);
                    break;
                }
            }
        }

        [MenuItem("RapidFramework/Force Generate Framework Managers Facade")]
        public static void ForceGenerate()
        {
            string[] guids = AssetDatabase.FindAssets("RapidFramework t:Script");
            bool found = false;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (Path.GetFileName(path) == "RapidFramework.cs")
                {
                    Generate(path);
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning("<color=#FF4500>[RapidFramework]</color> RapidFramework.cs 파일을 찾을 수 없습니다.");
            }
        }

        private static void Generate(string frameworkPath)
        {
            if (!File.Exists(frameworkPath)) 
                return;

            string content = File.ReadAllText(frameworkPath);
            string[] lines = content.Split(new[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

            HashSet<string> processedTypes = new HashSet<string>();
            List<string> fieldNames = new List<string>();

            string pattern = @"RegisterManager\s*<\s*(?<type>[\w\.]+)\s*>\s*\(";

            StringBuilder managerProps = new StringBuilder();

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("//") || trimmedLine.StartsWith("/*")) 
                    continue;

                Match match = Regex.Match(line, pattern);

                if (match.Success)
                {
                    string fullTypeName = match.Groups["type"].Value;

                    if (fullTypeName == "T" || processedTypes.Contains(fullTypeName))
                        continue;

                    processedTypes.Add(fullTypeName);


                    string shortName = fullTypeName.Contains(".") ? 
                        fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1) : fullTypeName;

                    string propertyName = shortName.EndsWith("Manager") ? 
                        shortName.Replace("Manager", "") : shortName;

                    string fieldName = $"_{char.ToLower(propertyName[0])}{propertyName.Substring(1)}";

                    fieldNames.Add(fieldName);

                    managerProps.AppendLine($"        private {fullTypeName} {fieldName};");
                    managerProps.AppendLine($"        private {fullTypeName} {propertyName}Internal" +
                        $" => {fieldName} ??= GetManager<{fullTypeName}>();");

                    managerProps.AppendLine($"        public static {fullTypeName} {propertyName}" +
                        $" => Instance.{propertyName}Internal;");

                    managerProps.AppendLine();
                }
            }

            string clearCache = string.Join("\n", fieldNames.ConvertAll(f => $"            {f} = null;"));

            string template = $@"/*
 *  Comment     : 이 코드는 RapidFrameworkFacade에 의해 자동 생성되었습니다. 
 *                수동으로 수정하지 마십시오.
 */

namespace RapidFramework
{{
    public partial class RapidFramework
    {{
{managerProps}
        private void ClearCache()
        {{
{clearCache}
        }}
    }}
}}";
            SaveFile(frameworkPath, template);
        }

        private static void SaveFile(string frameworkPath, string template)
        {
            template = Regex.Replace(template, @"\r\n|\n\r|\n|\r", Environment.NewLine);

            string directory = Path.GetDirectoryName(frameworkPath);
            string targetDir = Path.Combine(directory, "Facade").Replace("\\", "/");

            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);

            string outputPath = Path.Combine(targetDir, "RapidFramework.Managers.cs").Replace("\\", "/");

            if (File.Exists(outputPath) && File.ReadAllText(outputPath) == template)
                return;

            try
            {
                _isProcessing = true;
                File.WriteAllText(outputPath, template, new UTF8Encoding(false));
                AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
                Debug.Log($"<color=#4AF626>[RapidFramework]</color> Facade 업데이트 완료: {outputPath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"<color=#FF4500>[RapidFramework]</color> 파일 저장 중 오류 발생: {e.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}