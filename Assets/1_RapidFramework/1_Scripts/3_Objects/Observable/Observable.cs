/*
 *  Comment     :
 */

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RapidFramework
{
    //Static class Interface
    public static class Observable
    {
        public interface IField
        {
            object GetValue();
            void AddListenerObject(Action<object> listener, bool immediateNotify = true);
            void RemoveListenerObject(Action<object> listener);
        }
    }

    public class ObservableConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Observable<>);

        public override object ReadJson(
            JsonReader reader, 
            Type objectType, 
            object existingValue,
            JsonSerializer serializer
        )
        {
            if (reader.TokenType == JsonToken.Null) 
                return null;

            Type valueType = objectType.GetGenericArguments()[0];
            object rawValue = serializer.Deserialize(reader, valueType);

            var instance = existingValue ?? Activator.CreateInstance(objectType, rawValue);

            var field = objectType.GetField("_value", BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(instance, rawValue);

            return instance;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value, 
            JsonSerializer serializer
        )
        {
            if (value is Observable.IField field) 
                serializer.Serialize(writer, field.GetValue());
        }
    }

    [Serializable]
    [JsonConverter(typeof(ObservableConverter))]
    public class Observable<T> : Observable.IField , ISerializationCallbackReceiver
    {
        [SerializeField] private T _value;

        private T _lastSnapshot;

        private readonly List<Action<T>> _listeners = new();
        private readonly HashSet<Action<T>> _listenerSet = new();
        private readonly Dictionary<Action<object>, Action<T>> _bridgeMap = new();
        private readonly Dictionary<Action<T>, Action<object>> _reverseBridgeMap = new();

        private Action<T> _externalNotifyAction;

        private readonly Dictionary<string, Func<T, T>> _guardMap = new();
        private readonly List<string> _guardKeys = new();

        private static readonly bool IsUnityObject = 
            typeof(UnityEngine.Object).IsAssignableFrom(typeof(T));

        private static readonly Func<T, T, bool> EqualsCheck = DetermineEqualsCheck();

        private static Func<T, T, bool> DetermineEqualsCheck()
        {
            if (IsUnityObject)
                return (a, b) => (a as UnityEngine.Object) == (b as UnityEngine.Object);

            return (a, b) => EqualityComparer<T>.Default.Equals(a, b);
        }

        public T Value
        {
            get => _value;
         
            set
            {
                T finalValue = value;

                for (int i = 0; i < _guardKeys.Count; i++)
                {
                    string key = _guardKeys[i];
                    finalValue = _guardMap[key](finalValue);
                }

                if (EqualsCheck(_value, finalValue)) 
                    return;

                _value = finalValue;
                _lastSnapshot = finalValue;
                
                Notify();
                _externalNotifyAction?.Invoke(finalValue);
            }
        }
        public bool HasGuard(string key)
        {
            if (key.IsNullOrEmpty())
                return false;

            return _guardMap.ContainsKey(key);
        }
        public Observable<T> SetGuard(string key, Func<T, T> guard)
        {
            if (key.IsNullOrEmpty() || guard.IsNull()) 
                return this;

            if (!_guardMap.ContainsKey(key))
                _guardKeys.Add(key);

            _guardMap[key] = guard;

            return this;
        }
        public void RemoveGuard(string key)
        {
            if (_guardMap.Remove(key))
                _guardKeys.Remove(key);
        }
        public void ClearGuard()
        {
            _guardMap.Clear();
            _guardKeys.Clear();
        }

        public Observable<T> BindEvent(Action<T> notifyAction)
        {
            _externalNotifyAction = notifyAction;
            return this;
        }

        public static implicit operator T(Observable<T> observer)
        {
            if (observer == null)
                return default;

            return observer._value;
        }
        public Observable()
            => _value = default;

        public Observable(T initialValue = default)
        {
            _value = initialValue;
            _lastSnapshot = initialValue;
        }

        public Observable<T> Set(T value)
        {
            Value = value;
            return this;
        }
        public Observable<T> Add(T value)
        {
            if (typeof(T) == typeof(int))
                Value = (T)(object)((int)(object)_value + (int)(object)value);
            
            else if (typeof(T) == typeof(long))
                Value = (T)(object)((long)(object)_value + (long)(object)value);
            
            else if (typeof(T) == typeof(float))
                Value = (T)(object)((float)(object)_value + (float)(object)value);
            
            else if (typeof(T) == typeof(double))
                Value = (T)(object)((double)(object)_value + (double)(object)value);

            else if (typeof(T) == typeof(Vector2))
                Value = (T)(object)((Vector2)(object)_value + (Vector2)(object)value);

            else if (typeof(T) == typeof(Vector3))
                Value = (T)(object)((Vector3)(object)_value + (Vector3)(object)value);

            else if (typeof(T) == typeof(Vector4))
                Value = (T)(object)((Vector4)(object)_value + (Vector4)(object)value);

            return this;
        }
        public Observable<T> Subtract(T value)
        {
            if (typeof(T) == typeof(int))
                Value = (T)(object)((int)(object)_value - (int)(object)value);

            else if (typeof(T) == typeof(long))
                Value = (T)(object)((long)(object)_value - (long)(object)value);

            else if (typeof(T) == typeof(float))
                Value = (T)(object)((float)(object)_value - (float)(object)value);

            else if (typeof(T) == typeof(double)) 
                Value = (T)(object)((double)(object)_value - (double)(object)value);

            else if (typeof(T) == typeof(Vector2)) 
                Value = (T)(object)((Vector2)(object)_value - (Vector2)(object)value);

            else if (typeof(T) == typeof(Vector3)) 
                Value = (T)(object)((Vector3)(object)_value - (Vector3)(object)value);

            else if (typeof(T) == typeof(Vector4))
                Value = (T)(object)((Vector4)(object)_value - (Vector4)(object)value);

            return this;
        }
        public Observable<T> Multiply(T value)
        {
            if (typeof(T) == typeof(int))
                Value = (T)(object)((int)(object)_value * (int)(object)value);

            else if (typeof(T) == typeof(long))
                Value = (T)(object)((long)(object)_value * (long)(object)value);

            else if (typeof(T) == typeof(float)) 
                Value = (T)(object)((float)(object)_value * (float)(object)value);

            else if (typeof(T) == typeof(double)) 
                Value = (T)(object)((double)(object)_value * (double)(object)value);

            return this;
        }
        public Observable<T> Multiply(float scalar)
        {
            if (typeof(T) == typeof(int))
                Value = (T)(object)(int)((int)(object)_value * scalar);

            else if (typeof(T) == typeof(long))
                Value = (T)(object)(long)((long)(object)_value * scalar);

            else if (typeof(T) == typeof(float))
                Value = (T)(object)((float)(object)_value * scalar);

            else if (typeof(T) == typeof(double))
                Value = (T)(object)((double)(object)_value * scalar);

            else if (typeof(T) == typeof(Vector2))
                Value = (T)(object)((Vector2)(object)_value * scalar);

            else if (typeof(T) == typeof(Vector3))
                Value = (T)(object)((Vector3)(object)_value * scalar);

            else if (typeof(T) == typeof(Vector4))
                Value = (T)(object)((Vector4)(object)_value * scalar);

            return this;            
        }

        public Observable<T> Divide(T value)
        {
            if (typeof(T) == typeof(int) && (int)(object)value == 0)
                return this;

            if (typeof(T) == typeof(long) && (long)(object)value == 0)
                return this;

            if (typeof(T) == typeof(float) && (float)(object)value == 0f)
                return this;

            if (typeof(T) == typeof(double) && (double)(object)value == 0)
                return this;


            if (typeof(T) == typeof(int))
                Value = (T)(object)((int)(object)_value / (int)(object)value);

            else if (typeof(T) == typeof(long))
                Value = (T)(object)((long)(object)_value / (long)(object)value);

            else if (typeof(T) == typeof(float))
                Value = (T)(object)((float)(object)_value / (float)(object)value);

            else if (typeof(T) == typeof(double))
                Value = (T)(object)((double)(object)_value / (double)(object)value);

            return this;
        }

        public Observable<T> Divide(float scalar)
        {
            if (scalar == 0) 
                return this;

            if (typeof(T) == typeof(int))
                Value = (T)(object)(int)((int)(object)_value / scalar);

            else if (typeof(T) == typeof(long))
                Value = (T)(object)(long)((long)(object)_value / scalar);

            else if (typeof(T) == typeof(float)) 
                Value = (T)(object)((float)(object)_value / scalar);

            else if (typeof(T) == typeof(double))
                Value = (T)(object)((double)(object)_value / scalar);

            else if (typeof(T) == typeof(Vector2))
                Value = (T)(object)((Vector2)(object)_value / scalar);

            else if (typeof(T) == typeof(Vector3))
                Value = (T)(object)((Vector3)(object)_value / scalar);

            else if (typeof(T) == typeof(Vector4))
                Value = (T)(object)((Vector4)(object)_value / scalar);

            return this;
        }

        private T CalculateMin(T cur, T min)
        {
            if (typeof(T) == typeof(int)) 
                return (T)(object)Mathf.Max((int)(object)cur, (int)(object)min);

            if (typeof(T) == typeof(float))
                return (T)(object)Mathf.Max((float)(object)cur, (float)(object)min);

            if (typeof(T) == typeof(double)) 
            {
                double v = (double)(object)cur;
                double m = (double)(object)min; 

                return (T)(object)(v < m ? m : v); 
            }

            if (typeof(T) == typeof(long)) 
            {
                long v = (long)(object)cur; 
                long m = (long)(object)min;

                return (T)(object)(v < m ? m : v); 
            }

            if (typeof(T) == typeof(Vector2))
                return (T)(object)Vector2.Max((Vector2)(object)cur, (Vector2)(object)min);

            if (typeof(T) == typeof(Vector3)) 
                return (T)(object)Vector3.Max((Vector3)(object)cur, (Vector3)(object)min);

            if (typeof(T) == typeof(Vector4)) 
                return (T)(object)Vector4.Max((Vector4)(object)cur, (Vector4)(object)min);

            return cur;
        }

        private T CalculateMax(T cur, T max)
        {
            if (typeof(T) == typeof(int)) 
                return (T)(object)Mathf.Min((int)(object)cur, (int)(object)max);

            if (typeof(T) == typeof(float)) 
                return (T)(object)Mathf.Min((float)(object)cur, (float)(object)max);

            if (typeof(T) == typeof(double)) 
            { 
                double v = (double)(object)cur; 
                double m = (double)(object)max;

                return (T)(object)(v > m ? m : v);
            }

            if (typeof(T) == typeof(long)) 
            { 
                long v = (long)(object)cur; 
                long m = (long)(object)max;

                return (T)(object)(v > m ? m : v);
            }

            if (typeof(T) == typeof(Vector2)) 
                return (T)(object)Vector2.Min((Vector2)(object)cur, (Vector2)(object)max);

            if (typeof(T) == typeof(Vector3)) 
                return (T)(object)Vector3.Min((Vector3)(object)cur, (Vector3)(object)max);

            if (typeof(T) == typeof(Vector4))
                return (T)(object)Vector4.Min((Vector4)(object)cur, (Vector4)(object)max);

            return cur;
        }

        private T CalculateClamp(T cur, T min, T max)
        {
            if (typeof(T) == typeof(int)) 
                return (T)(object)Mathf.Clamp((int)(object)cur, (int)(object)min, (int)(object)max);

            if (typeof(T) == typeof(float)) 
                return (T)(object)Mathf.Clamp((float)(object)cur, (float)(object)min, (float)(object)max);

            if (typeof(T) == typeof(double))
            {
                double v = (double)(object)cur;
                double mn = (double)(object)min; 
                double mx = (double)(object)max;

                return (T)(object)(v < mn ? mn : (v > mx ? mx : v));
            }

            if (typeof(T) == typeof(long))
            {
                long v = (long)(object)cur; 
                long mn = (long)(object)min; 
                long mx = (long)(object)max;

                return (T)(object)(v < mn ? mn : (v > mx ? mx : v));
            }

            if (typeof(T) == typeof(Vector2))
            {
                Vector2 v = (Vector2)(object)cur; 
                Vector2 mn = (Vector2)(object)min;
                Vector2 mx = (Vector2)(object)max;

                return (T)(object)new Vector2(
                    Mathf.Clamp(v.x, mn.x, mx.x), 
                    Mathf.Clamp(v.y, mn.y, mx.y));
            }

            if (typeof(T) == typeof(Vector3))
            {
                Vector3 v = (Vector3)(object)cur; 
                Vector3 mn = (Vector3)(object)min;
                Vector3 mx = (Vector3)(object)max;

                return (T)(object)new Vector3(
                    Mathf.Clamp(v.x, mn.x, mx.x), 
                    Mathf.Clamp(v.y, mn.y, mx.y), 
                    Mathf.Clamp(v.z, mn.z, mx.z));
            }

            if (typeof(T) == typeof(Vector4))
            {
                Vector4 v = (Vector4)(object)cur; 
                Vector4 mn = (Vector4)(object)min;
                Vector4 mx = (Vector4)(object)max;

                return (T)(object)new Vector4(
                    Mathf.Clamp(v.x, mn.x, mx.x),
                    Mathf.Clamp(v.y, mn.y, mx.y),
                    Mathf.Clamp(v.z, mn.z, mx.z),
                    Mathf.Clamp(v.w, mn.w, mx.w));
            }

            return cur;
        }
        public Observable<T> Min(T min, bool saveAsGuard = false)
        {
            Value = CalculateMin(_value, min);

            if (saveAsGuard)
                SetGuard("MinRule", val => CalculateMin(val, min));

            return this;
        }
        public Observable<T> Max(T max, bool saveAsGuard = false)
        {
            Value = CalculateMax(_value, max);

            if (saveAsGuard)
                SetGuard("MaxRule", val => CalculateMax(val, max));

            return this;
        }
        public Observable<T> Clamp(T min, T max, bool saveAsGuard = false)
        {
            Value = CalculateClamp(_value, min, max);

            if (saveAsGuard)
                SetGuard("ClampRule", val => CalculateClamp(val, min, max));

            return this;
        }

        public void Notify()
        {
            for (int i = _listeners.Count - 1; i >= 0; i--)
            {
                if (i >= _listeners.Count) 
                    continue;

                var listener = _listeners[i];

                if (listener == null || !_listenerSet.Contains(listener))
                    continue;

                try
                {
                    listener.Invoke(_value);
                }
                catch (Exception ex)
                {
                    RapidFramework.Log.LogError($"Listener exception: {ex.Message}");
                }
            }
        }
        public void AddListener(Action<T> listener , bool immediateNotify = true)
        {
            if (listener == null)
                return;

            if (_listenerSet.Add(listener))
            {
                _listeners.Add(listener);

                if (immediateNotify)
                {
                    try
                    {
                        listener.Invoke(_value);
                    }
                    catch (Exception ex)
                    {
                        RapidFramework.Log.LogError($"Immediate Notify exception: {ex.Message}");
                    }
                }
            }                       
        }
        public void RemoveListener(Action<T> listener)
        {
            if (listener == null)
                return;

            if(_listenerSet.Remove(listener))
            {
                _listeners.Remove(listener);

                if (_reverseBridgeMap.Count > 0 && _reverseBridgeMap.TryGetValue(listener, out var objListener))
                {
                    _bridgeMap.Remove(objListener);
                    _reverseBridgeMap.Remove(listener);
                }
            }                
        }
        public void ClearListeners()
        {
            _listeners.Clear();
            _listenerSet.Clear();
            _bridgeMap.Clear();
            _reverseBridgeMap.Clear();
        }

        public object GetValue()
            => _value;

        public void AddListenerObject(Action<object> listener, bool immediateNotify = true)
        {
            if (listener == null)
                return;

            if(!_bridgeMap.TryGetValue(listener, out var bridge))
            {
                bridge = (val) => listener.Invoke(val);
                _bridgeMap[listener] = bridge;
                _reverseBridgeMap[bridge] = listener;

            }
            
            AddListener(bridge, immediateNotify);
        }

        public void RemoveListenerObject(Action<object> listener)
        {
            if (listener == null) 
                return;

            if (_bridgeMap.TryGetValue(listener, out var bridge))
                RemoveListener(bridge);
        }

        public override string ToString()
        {
            if(IsUnityObject)
            {
                var obj = _value as UnityEngine.Object;

                return obj == null 
                    ? "null" 
                    : obj.name;
            }

            return _value != null 
                ? _value.ToString() 
                : "null";
        }

        public class SubscriptionHandler : IDisposable
        {
            private Observable<T> _parent;
            private Action<T> _listener;

            public SubscriptionHandler(Observable<T> parent, Action<T> listener)
            {
                _parent = parent;
                _listener = listener;
            }
            public void Dispose()
            {
                if (_parent == null || _listener == null)
                    return;

                _parent.RemoveListener(_listener);
                _parent = null;
                _listener = null;
            }
        }

        public SubscriptionHandler AddListenerWithDisposable(Action<T> listener, bool immediateNotify = true)
        {
            AddListener(listener, immediateNotify);
            return new SubscriptionHandler(this, listener);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (!EqualsCheck(_lastSnapshot, _value))
            {
                _lastSnapshot = _value;
                Notify();
            }
        }
    }
}
