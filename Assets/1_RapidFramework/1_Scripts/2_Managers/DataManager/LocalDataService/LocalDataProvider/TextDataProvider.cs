/*
 *  Comment     :
 */

using System.Collections.Generic;
using System;
using UnityEngine;

namespace RapidFramework
{
    //Interface
    public partial class TextDataProvider : LocalDataProvider
    {
        public interface ITextRefreshable
        {
            void Refresh();
        }

    }
    //Serializable
    public partial class TextDataProvider 
    {
        [Serializable]
        public class TextData : DataRow
        {
            public string Value;            
        }
    }
    //Data
    public partial class TextDataProvider
    {
        private SettingsDataProvider _settings = null;
        private List<ITextRefreshable> _refreshables = new();

        public DataTable<TextData> TextTable { get; private set; } = new();
    }
    //Life Cycle
    public partial class TextDataProvider
    {
        public override void Initialize()
        {
            base.Initialize();

            _settings = RapidFramework.Data.Local.Settings;
            _settings.Language.AddListener(RefreshData, true);
        }        
    }
    //Logic
    public partial class TextDataProvider
    {
        private string GetDataPath(
            string languageCode, 
            string folder, 
            string file
        ) => languageCode switch
        {
            "ko" => $"{folder}/{file}_ko",
            "en" => $"{folder}/{file}_en",
            _ => $"{folder}/{file}_en"
        };

        private void RefreshData(string languageCode)
        {
            TextTable.Clear();

            string commonTextDataPath = GetDataPath(languageCode, "TextData", "CommonTextData");

            LoadFromSources (TextTable,commonTextDataPath);

            NotifyRefreshables();
        }
        private void NotifyRefreshables()
        {
            for (int i = _refreshables.Count - 1; i >= 0; i--)
            {
                if (_refreshables[i] != null)
                    _refreshables[i].Refresh();
                else
                    _refreshables.RemoveAt(i);
            }
        }
        public void Bind(ITextRefreshable refreshable)
        {
            if (refreshable.IsNull())
                return;

            if (!_refreshables.Contains(refreshable))
                _refreshables.Add(refreshable);
        }
        public void Unbind(ITextRefreshable refreshable)
        {
            if (!refreshable.IsNull() && _refreshables.Contains(refreshable))
                _refreshables.Remove(refreshable);
        }
    }
}
