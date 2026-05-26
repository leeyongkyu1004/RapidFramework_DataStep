/*
 *  Comment     :
 */

using System;
using System.Collections.Generic;

namespace RapidFramework
{
    //Data
    public partial class RapidFramework : GenericSingleton<RapidFramework>
    {
        private readonly Dictionary<Type, ManagerBase> _managers = new();
        private readonly List<ManagerBase> _orderedManagers = new();
        private bool _isActivated = false;        
    }

    //Register
    public partial class RapidFramework
    {
        private void Register()
        {
            //Manager Insert Here.
            RegisterManager<LogManager>();
            RegisterManager<ParserManager>();
            RegisterManager<DataManager>();
            RegisterManager<SceneManager>();
        }

        private void RegisterManager<T>() where T : ManagerBase
        {
            var type = typeof(T);

            if (_managers.ContainsKey(type))
                return;

            if (!TryGetComponent<T>(out T manager))
                manager = gameObject.AddComponent<T>();

            if (_managers.TryAdd(type, manager))
            {
                _orderedManagers.Add(manager);
                manager.Register();
            }
            else
            {
                Log.LogError($"Manager Register Failed {type.Name}");
            }
        }
    }

    //Life Cycle
    public partial class RapidFramework 
    {
        public void Initialize()
        {
            foreach (var manager in _orderedManagers)
            {
                manager.Initialize();
            }
        }
        protected override void OnDestroy()
        {
            Deactivate();
            base.OnDestroy();
        }
    }
    
    //Logic
    public partial class RapidFramework
    {
        public void Activate()
        {
            if (_isActivated)
                return;

            _isActivated = true;

            Register();     
            Initialize();  

            Scene.Load("SampleScene");
        }
        public void Deactivate()
        {
            if (!_isActivated)
                return;

            for (int i = _orderedManagers.Count - 1; i >= 0; i--)
            {
                var manager = _orderedManagers[i];

                if (manager != null)
                {
                    manager.Unregister();
                    Destroy(manager);
                }
            }

            _managers.Clear();
            _orderedManagers.Clear();
            _isActivated = false;

            ClearCache();
            
        }
    }
    //Get
    public partial class RapidFramework 
    {
        private T GetManager<T>() where T : ManagerBase
        {
            var type = typeof(T);

            if (_managers.TryGetValue(type, out var manager))
                return (T)manager;

            Log.LogError($"[RapidFramework] Not Found Manager : {type.Name}");
            return null;
        }
            
    }
}
