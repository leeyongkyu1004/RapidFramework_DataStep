/*
 *  Comment     :
 */

using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace RapidFramework
{
    //Data
    public partial class LocalFileProvider
    {
        private string _savePath;
    }
    //Life Cycle
    public partial class LocalFileProvider
    {
        public void Initialize()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "Saves");

            if (!Directory.Exists(_savePath))
                Directory.CreateDirectory(_savePath);

            RapidFramework.Log.LogInfo($"Initialized. Path: {_savePath}");
        }
    }
    //Logic
    public partial class LocalFileProvider
    {
        public void Save<T>(string fileName,T data, bool prettyPrint = true) where T : class 
        {
            if (data.IsNull())
                return;

            try
            {
                string json = JsonConvert.SerializeObject
                    (
                        data,
                        typeof(object),
                        prettyPrint ? Formatting.Indented : Formatting.None,
                        JsonExtension.DefaultSettings
                    );
                File.WriteAllText(GetFullPath(fileName), json);                             
            }
            catch(Exception ex)
            {
                RapidFramework.Log.LogError($"Save Failed: {ex.Message}");
            }
            finally
            {
                RapidFramework.Log.LogInfo($"[LocalFileProvider] Save file : {fileName}.");
            }
        }
        public void Save(params (string fileName, object data)[] datas)
        {
            if (datas.IsEmpty()) 
                return;

            foreach (var (fileName, data) in datas)
            {
                Save(fileName, data);
            }
        }
        public void SaveRaw(string fileName, string rawContent)
        {
            if (string.IsNullOrEmpty(rawContent))
            {
                RapidFramework.Log.LogWarning($"rawContent is empty for {fileName}");                
                return;
            }

            try
            {
                File.WriteAllText(GetFullPath(fileName), rawContent);               
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"SaveRaw Failed: {ex.Message}");                
            }
            finally
            {
                RapidFramework.Log.LogInfo($"SaveRaw file : {fileName}.");                
            }
        }
        public void SaveRaw(params (string fileName, string rawContent)[] datas)
        {
            if(datas.IsEmpty())
                return;

            foreach (var (fileName, data) in datas)
            {
                SaveRaw(fileName, data);
            }
        }
        public T Load<T>(string fileName)
        {
            string fullPath = GetFullPath(fileName);

            if (!File.Exists(fullPath))
                return default;

            try
            {
                string json = File.ReadAllText(fullPath);

                var result = JsonConvert.DeserializeObject<object>(
                    json,
                    JsonExtension.DefaultSettings);

                return (result is T casted) ? casted : default;
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"Load Failed: {ex.Message}");
                return default;
            }
            finally
            {
                RapidFramework.Log.LogInfo($"[LocalFileProvider] Load file : {fileName}.");                
            }
        }
        public object[] Load(params string[] fileNames)
        {
            if(fileNames.IsNullOrEmpty())
                return Array.Empty<object>();

            object[] results = new object[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                results[i] = Load<object>(fileNames[i]);
            }
            return results;
        }
        public string LoadRaw(string fileName)
        {
            string fullPath = GetFullPath(fileName);

            if (!File.Exists(fullPath))
                return null;

            try
            {
                return File.ReadAllText(fullPath);
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"LoadRaw Failed: {ex.Message}");
                return null;
            }
            finally
            {
                RapidFramework.Log.LogInfo($"LoadRaw file : {fileName}.");
            }
        }
        public string[] LoadRaw(params string[] fileNames)
        {
            if (fileNames.IsEmpty())
                return Array.Empty<string>();

            string[] results = new string[fileNames.Length];

            for (int i = 0; i < fileNames.Length; i++)
            {
                results[i] = LoadRaw(fileNames[i]);
            }
            return results;
        }
        public T[] LoadArray<T>(string fileName)
        {
            return Load<T[]>(fileName) ?? Array.Empty<T>();
        }
    }

    //Logic Awaitable
    public partial class LocalFileProvider
    {
        public async Awaitable SaveAsync<T>(
            string fileName, 
            T data, 
            bool prettyPrint = true
        ) where T : class
        {
            if (data.IsNull())
                return;

            try
            {
                await Awaitable.BackgroundThreadAsync();

                string json = JsonConvert.SerializeObject
                    (
                        data, 
                        typeof(object),
                        prettyPrint ? Formatting.Indented : Formatting.None,
                        JsonExtension.DefaultSettings
                    );
                await File.WriteAllTextAsync(GetFullPath(fileName), json);
            }
            catch(Exception ex)
            {
                await Awaitable.MainThreadAsync();
                RapidFramework.Log.LogError($"[SaveAsync Failed] {fileName}: {ex.Message}");                
            }
            finally
            {
                await Awaitable.MainThreadAsync();
                RapidFramework.Log.LogInfo($"SaveAsync file : {fileName}.");
            }
        }
        public async Awaitable SaveAsync(params (string fileName, object data)[] datas)
        {
            if (datas.IsEmpty())
                return;

            List<Awaitable> saveTasks = new();

            foreach (var (fileName, data) in datas)
            {
                saveTasks.Add(SaveAsync(fileName, data));
            }

            foreach (var task in saveTasks)
            {
                await task;
            }
        }

        public async Awaitable<T> LoadAsync<T>(string fileName)
        {
            string fullPath = GetFullPath(fileName);
            
            if (!File.Exists(fullPath))
                return default;

            try
            {
                await Awaitable.BackgroundThreadAsync();
                
                string json = await File.ReadAllTextAsync(fullPath);
                return JsonConvert.DeserializeObject<T>(json, JsonExtension.DefaultSettings);
            }
            catch (Exception ex)
            {
                await Awaitable.MainThreadAsync();
                RapidFramework.Log.LogError($"[LoadAsync Failed] {fileName}: {ex.Message}");
                return default;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
                RapidFramework.Log.LogInfo($"LoadAsync file : {fileName}.");
            }
        }
        public async Awaitable<object[]> LoadAsync(params string[] fileNames)
        {
            if(fileNames.IsNullOrEmpty())
                return Array.Empty<object>();

            List<Awaitable<object>> loadTasks = new();

            foreach (var name in fileNames)
            {
                loadTasks.Add(LoadAsyncObject(name));
            }

            object[] results = new object[fileNames.Length];

            for (int i = 0; i < loadTasks.Count; i++)
            {
                results[i] = await loadTasks[i];
            }
            return results;
        }
        private async Awaitable<object> LoadAsyncObject(string fileName)
        {
            string fullPath = GetFullPath(fileName);

            if (!File.Exists(fullPath)) 
                return null;

            try
            {
                await Awaitable.BackgroundThreadAsync();
                string json = await File.ReadAllTextAsync(fullPath);
                return JsonConvert.DeserializeObject<object>(json, JsonExtension.DefaultSettings);
            }
            catch 
            {
                await Awaitable.MainThreadAsync();
                return null;
            }
            finally
            {
                await Awaitable.MainThreadAsync();
                RapidFramework.Log.LogInfo($"LoadAsyncObject file : {fileName}.");                
            }
        }
    }
    //Logic Helper
    public partial class LocalFileProvider
    {
        public bool Exists(string fileName)
            => File.Exists(GetFullPath(fileName));

        public void Delete(string fileName)
        {
            string fullPath = GetFullPath(fileName);

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        public void DeleteAll()
        {
            try
            {
                if (!Directory.Exists(_savePath))
                    return;

                string[] files = Directory.GetFiles(_savePath);

                foreach (string file in files)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                RapidFramework.Log.LogError($"ClearAll Failed: {ex.Message}");
            }
        }
        private string GetFullPath(string fileName)
        {
            if (fileName.IsNullOrEmpty())
                return string.Empty;

            if (!fileName.EndsWith(".json"))
                fileName += ".json";

            return Path.Combine(_savePath, fileName);
        }          
    }
}
