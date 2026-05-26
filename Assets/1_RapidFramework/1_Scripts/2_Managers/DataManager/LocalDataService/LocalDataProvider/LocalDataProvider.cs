/*
 *  Comment     :
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static RapidFramework.ParserManager;

namespace RapidFramework
{
    //Serializable
    public abstract partial class LocalDataProvider 
    {
        [Serializable]
        public class DataRow
        {
            public string Id;

            public virtual void Initialize() { }

            public virtual T Clone<T>() where T : DataRow
                => (T)this.MemberwiseClone();
        }

        //Data Table
        public class DataTable<T> : IEnumerable<T> where T : DataRow
        {
            private Dictionary<string, T> _dataMap;
            private List<T> _itemCache;

            private static readonly List<T> EmptyList = new List<T>(0);

            public void Initialize(Dictionary<string, T> source)
            {
                _dataMap = source ?? new Dictionary<string, T>(StringComparer.Ordinal);
                ClearCache();

            }
            public void Clear()
            {
                _dataMap?.Clear();
                ClearCache();
            }

            public T Get(string id)
            {
                if (id.IsNullOrEmpty())
                    return null;

                return _dataMap.TryGetValue(id, out var data) ? data : null;

            }

            public IReadOnlyList<T> Items
                => _itemCache ??= (_dataMap.Count > 0 ? _dataMap.Values.ToList() : EmptyList);

            public void ClearCache()
                => _itemCache = null;

            public bool Contains(string id)
                => !id.IsNullOrEmpty() && _dataMap.ContainsKey(id);

            public int Count
                => _dataMap?.Count ?? 0;

            public T this[string id] => Get(id);

            public IEnumerator<T> GetEnumerator()
                => (_dataMap?.Values ?? (IEnumerable<T>)EmptyList).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator()
                => GetEnumerator();
        }
    }
    //Data
    public abstract partial class LocalDataProvider
    {
        protected static JsonService _json;
        protected static LocalFileProvider _fileProvider;
        public LocalFileProvider File => _fileProvider;

        private const int EstimatedRecordsPerFile = 50;
    }
    //Register
    public abstract partial class LocalDataProvider
    {
        public virtual void Register()
        {
            if (_json != null && _fileProvider != null)
                return;
            
            _json = RapidFramework.Parser.Json;            
            _fileProvider = RapidFramework.Data.Local.File;            
        }
    }
    //Life Cycle
    public abstract partial class LocalDataProvider
    {      
        public virtual void Initialize() { }        
    }    
    //Logic
    public abstract partial class LocalDataProvider
    {
        protected void LoadFromSources<T>(DataTable<T> targetTable, params string[] paths) 
            where T : DataRow
        {
            if (targetTable.IsNull() || paths.IsNullOrEmpty())
                return;

            var tempStorage = 
                new Dictionary<string, T>(paths.Length * EstimatedRecordsPerFile, StringComparer.Ordinal);

            foreach (var path in paths)
            {
                string fullPath = Path.Combine("Json", path);
                string rawData = _json.LoadRawJson(fullPath);

                if (rawData.IsNullOrEmpty($"File not found or empty: {fullPath}"))
                    continue;

                var rows = _json.FromJsonArray<T>(rawData);

                if (rows.IsNull($"JSON Parse Failed: {fullPath}"))
                    continue;

                foreach (var row in rows)
                {
                    if (row.IsNull())
                        continue;

                    if(row.Id.IsNullOrEmpty($"Empty Id Detected : {path}"))
                        continue;

                    if (tempStorage.TryAdd(row.Id, row))
                        row.Initialize();
                    else
                        RapidFramework.Log.LogError($"Duplicate Id: {row.Id} in {path}");
                }
            }
            targetTable.Initialize(tempStorage);
        }
    }
}
