/*
 *  Comment :
 */

using UnityEngine;

namespace RapidFramework
{
    public abstract class GenericSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        private static T _instance = null;                      
        private static readonly object _lock = new object();             
        private static bool _isQuitting = false;     

        public static bool IsQuitting => _isQuitting;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                    return null;

                lock(_lock)
                {
                    if(_instance != null)
                        return _instance;

                    _instance = FindAnyObjectByType<T>(FindObjectsInactive.Exclude);

#if UNITY_EDITOR                    
                    if(FindObjectsByType<T>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length >1 )
                    {
                        Debug.LogError($"[Singleton] {typeof(T)} is already instanced.");
                        return _instance;
                    }
#endif
                    if(_instance == null)
                    {
                        GameObject singleton = new GameObject($"@Singleton_{typeof(T).Name}");
                        _instance = singleton.AddComponent<T>();
                        DontDestroyOnLoad(singleton);
                    }
                    return _instance;
                }
            }            
        }    
        protected virtual void Awake()
        {
            if(_instance == null)
            {
                _instance = this as T;

                if (transform.parent != null)
                    transform.SetParent(null);

                DontDestroyOnLoad(gameObject);
            }
            else if(_instance != this)
            {
                Debug.LogWarning($"[Singleton] {typeof(T)} 중복이라 파괴함.");
                Destroy(gameObject);
            }
        }
        protected virtual void OnApplicationQuit()
            => _isQuitting = true;
        
        protected virtual void OnDestroy()
        {
            if(_instance != null && _instance == this )
            {
                Destroy(_instance.gameObject);
                _instance = null;
                _isQuitting = true;
            }
        }        
    }
}
