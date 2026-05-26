/*
 *  Comment :
 */

#if UNITY_EDITOR

using ExcelDataReader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RapidFramework
{
    //Data
    public partial class ExcelToJson 
    {
        private static readonly string XlsxFolderPath =
            Path.Combine(Application.dataPath, "1_RapidFramework", "3_LocalDatabase", "Xlsx");

        public static readonly string OutputJsonFolderPath =
            Path.Combine(Application.dataPath, "1_RapidFramework", "3_LocalDatabase", "Resources", "Json");

        private const int TypeRowIndex = 0;
        private const int KeyRowIndex = 1;
        private const int DataStartRowIndex = 4;

        private const string TypeString = "s";
        private const string TypeNumberic = "n";
        private const string TypeBool = "b";

        private const string TempFIlePrefix = "~$";

        private const string RootXlsxFolderName = "Xlsx";
    }
    //Logic
    public partial class ExcelToJson   
    {
        [MenuItem("Assets/RapidFramework - Convert ExcelToJson")]
        public static void ConvertExcelToJson()
        {
            if(!Directory.Exists(XlsxFolderPath))
            {
                Debug.LogError($"[ConvertExcelToJson] source folder not found : {XlsxFolderPath}");
                return;
            }

            if (!Directory.Exists(OutputJsonFolderPath))
                Directory.CreateDirectory(OutputJsonFolderPath);

            var directoryInfo = new DirectoryInfo(XlsxFolderPath);
            FileInfo[] xlsxFiles = directoryInfo.GetFiles("*.xlsx", SearchOption.AllDirectories);

            foreach(FileInfo fileInfo in xlsxFiles)
            {
                if (fileInfo.Name.Contains(TempFIlePrefix))
                    continue;

                string parentFolderName = fileInfo.Directory.Name;
                string subFolderNmae = parentFolderName.Equals(RootXlsxFolderName, StringComparison.OrdinalIgnoreCase)
                    ? string.Empty
                    : parentFolderName;

                if(!string.IsNullOrEmpty(subFolderNmae))
                {
                    string targetSubFolderPath = Path.Combine(OutputJsonFolderPath, subFolderNmae);

                    if (!Directory.Exists(targetSubFolderPath))
                        Directory.CreateDirectory(targetSubFolderPath);
                }

                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);

                try
                {
                    ReadExcelFile(fileInfo.FullName, fileNameWithoutExtension, subFolderNmae);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ConvertExcelToJson] Error in {fileInfo.Name} : {ex.Message}");
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Completed Convert ExcelToJson");
        }

        public static void ReadExcelFile(
            string excelFilePath, 
            string fileName, 
            string subFolderNmae
        )
        {
            using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var dataSet = reader.AsDataSet();

                for(int tableIndex = 0; tableIndex < dataSet.Tables.Count; tableIndex++)
                {
                    var table = dataSet.Tables[tableIndex];

                    if (table.Rows.Count < 5)
                        continue;

                    var typeList = new List<string>();
                    int validColumnCount = 0;

                    for(int col = 1; col < table.Columns.Count; col++)
                    {
                        string type = table.Rows[TypeRowIndex][col]?.ToString();

                        if(string.IsNullOrEmpty(type))
                        {
                            validColumnCount = col;
                            break;
                        }
                     
                        typeList.Add(type.ToLower());
                    }

                    if (validColumnCount == 0)
                        validColumnCount = table.Columns.Count;

                    string nestedObjectName = table.Rows[KeyRowIndex][0]?.ToString() ?? "Data";

                    var keyList = new List<string>();
                    for(int col = 1; col < validColumnCount; col++)
                    {
                        keyList.Add(table.Rows[KeyRowIndex][col]?.ToString() ?? string.Empty);
                    }

                    var rowDataList = new List<List<string>>();
                    for(int row = DataStartRowIndex; row < table.Rows.Count; row++)
                    {
                        if (string.IsNullOrEmpty(table.Rows[row][1]?.ToString()))
                            continue;

                        var rowValues = new List<string>();
                        for(int col = 1; col < validColumnCount; col++)
                        {
                            rowValues.Add(table.Rows[row][col]?.ToString() ?? string.Empty);
                        }
                        rowDataList.Add(rowValues);
                    }

                    string outputFolderPath = OutputJsonFolderPath;
                    if(!string.IsNullOrEmpty(subFolderNmae))
                    {
                        outputFolderPath = Path.Combine(OutputJsonFolderPath, subFolderNmae);

                        if (!Directory.Exists(outputFolderPath))
                            Directory.CreateDirectory(outputFolderPath);
                    }

                    string outputFileName = dataSet.Tables.Count > 1
                        ? $"{fileName}_{table.TableName}"
                        : fileName;

                    outputFileName = Path.Combine(outputFolderPath, $"{outputFileName}.json");

                    CreateJsonFile(outputFileName, nestedObjectName, typeList, keyList, rowDataList);
                }
            }                
        }
        public static void CreateJsonFile(
            string outputFilePath,
            string nestedObjectName,
            List<string> typeList,
            List<string> keyList,
            List<List<string>> rowDataList
        )
        {
            var records = new List<Dictionary<string, object>>();

            foreach (var rowData in rowDataList)
            {
                var record = new Dictionary<string, object>();

                for(int col = 0; col < keyList.Count; col++)
                {
                    string key = keyList[col];
                    string type = typeList[col];
                    string row = rowData[col];

                    record[key] = type switch
                    {
                        TypeString => row ?? string.Empty,
                        TypeNumberic => ParseNumeric(row),
                        TypeBool => row?.ToLower() == "true" || row == "1",
                        _ => row
                    };
                }

                records.Add(record);
            }

            string json = JsonConvert.SerializeObject(records, Formatting.Indented);
            File.WriteAllText(outputFilePath, json, Encoding.UTF8);
        }

        private static object ParseNumeric(string row)
        {
            if (!float.TryParse(row, out float value))
                return 0;

            return value == Mathf.Floor(value) ? (object)(int)value : value;
        }
    }

}
#endif
