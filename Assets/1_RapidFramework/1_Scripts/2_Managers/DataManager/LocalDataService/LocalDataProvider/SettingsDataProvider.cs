/*
 *  Comment     :
 */

using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using static RapidFramework.SettingsDataProvider;

namespace RapidFramework
{
    // Serializable
    public partial class SettingsDataProvider : LocalDataProvider
    {
        [Serializable]
        public class SettingsData : DataRow
        {
            public string Language;
            public string SupportLanguages;
        }
    }
    // Data
    public partial class SettingsDataProvider
    {
        private const string _defaultId = "default";
        private const string _saveFileName = "SettingData";

        private SettingsData _current;
        public SettingsData Current => _current;

        private string[] _cachedSupportLanguages;

        public Observable<string> Language { get; private set; } = new();

        public DataTable<SettingsData> SettingsTable { get; private set; } = new();        
    }
    // Life Cycle
    public partial class SettingsDataProvider
    {
        public override void Initialize()
        {
            base.Initialize();

            if (File.Exists(_saveFileName))
            {
                _current = File.Load<SettingsData>(_saveFileName);
            }
            else
            {
                LoadFromSources(SettingsTable, "SettingsData/SettingsData");

                _current = SettingsTable[_defaultId];
            }

            if (_current.IsNull())
                return;

            _cachedSupportLanguages = GetSupportLanguages();

            if (_current.Language.IsNullOrEmpty())
                SetLanguage(GetDefaultLanguageByOS(), true).Forget();
            else
                Language.Set(_current.Language);
        }
    }
    //Logic
    public partial class SettingsDataProvider
    {

        public async Awaitable SetLanguage(string languageCode, bool isForce = false)
        {
            if (_current.IsNull())
                return;

            if (!isForce && string.Equals(_current.Language, languageCode, StringComparison.OrdinalIgnoreCase))
                return;

            var supports = GetSupportLanguages();

            if (!supports.IsEmpty() && supports.Contains(languageCode, StringComparer.Ordinal))
            {
                _current.Language = languageCode;
                Language.Set(_current.Language);

                await File.SaveAsync(_saveFileName, _current);

                RapidFramework.Log.LogInfo($"Language Changed to: {languageCode}");
            }
        }
    }
    //Logic Get
    public partial class SettingsDataProvider
    {
        public string GetLanguage()
            => Language.Value ?? string.Empty;

        public string[] GetSupportLanguages()
        {
            if(_cachedSupportLanguages == null)
            {
                if (_current.IsNull() || _current.SupportLanguages.IsNullOrEmpty())
                    return Array.Empty<string>();

                _cachedSupportLanguages = _current.SupportLanguages
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);
            }
            return _cachedSupportLanguages;
        }

        private string GetDefaultLanguageByOS()
        {
            string osLang = Application.systemLanguage switch
            {
                SystemLanguage.Korean => "ko",
                SystemLanguage.English => "en",
                _ => "en"
            };

            var supports = GetSupportLanguages();

            if (!supports.IsNull() && supports.Contains(osLang, StringComparer.Ordinal))
                return osLang;

            return "en";
        }
    }
}
