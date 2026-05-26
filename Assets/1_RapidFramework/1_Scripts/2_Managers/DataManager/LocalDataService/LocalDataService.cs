/*
 *  Comment     :
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RapidFramework
{
    //Data
    public partial class LocalDataService 
    {
        private readonly LocalFileProvider _file = new();
        public LocalFileProvider File => _file;

        private readonly Dictionary<Type, LocalDataProvider> _providers = new();
        private readonly List<LocalDataProvider> _orderedProviders = new();
    }
    //Register
    public partial class LocalDataService
    {
        public void Register()
        {
            RegisterDataProvider<SettingsDataProvider>();
            RegisterDataProvider<TextDataProvider>();        
        }

        private void RegisterDataProvider<T>() where T : LocalDataProvider, new()
        {
            var type = typeof(T);

            if (_providers.ContainsKey(type))
            {
                RapidFramework.Log.LogWarning($"이미 등록된 프로바이더입니다 : {type.Name}");
                return;
            }

            var provider = new T();
            provider.AgainstNull(false, p => p.Register(), $"[{type.Name}] Register 실패");

            _providers.Add(type, provider);
            _orderedProviders.Add(provider);
        }
    }
    //Life Cycle
    public partial class LocalDataService
    { 
        public void Initialize()
        {
            _file.Initialize();

            foreach (var provider in _orderedProviders)
            {
                provider.Initialize();
            }
        }        
    }
    //Get
    public partial class LocalDataService 
    {
        private T GetDataProvider<T>() where T : LocalDataProvider
        {
            var type = typeof(T);

            if(_providers.TryGetValue(type, out var provider))
                return (T)provider;

            RapidFramework.Log.LogError($"등록되지 않는 프로바이더 호출 시도 : {type.Name}");
            return null;
        }
    }
}
