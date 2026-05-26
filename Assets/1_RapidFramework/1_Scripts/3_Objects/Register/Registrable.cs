/*
 *  Comment     :
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace RapidFramework
{   
    // Static Helper 
    internal static class RegistrableQuittingChecker
    {
        public static bool IsAppQuitting { get; private set; } = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitQuittingFlag()
        {
            IsAppQuitting = false;
            Application.quitting += () => IsAppQuitting = true;
        }
    }

    //Data
    public abstract partial class Registrable<T> : MonoBehaviour where T : Registrable<T>
    {
        protected bool _isRegistered = false;
        public bool IsRegistered => _isRegistered;

        private Action _onUnbind;

        protected static bool IsAppQuitting => RegistrableQuittingChecker.IsAppQuitting;

    }
    //Life Cycle
    public abstract partial class Registrable<T>
    {
        private void FinalizeRegistration()
        {
            _isRegistered = true;
            Initialize();
            OnRegistered();
        }
        public virtual void Initialize() { }

        protected virtual void OnRegistered() { }

        private void ExecuteUnbind()
        {
            if (!_isRegistered)
                return;

            _isRegistered = false;

            if (IsAppQuitting)
            {
                _onUnbind = null;
                return;
            }

            try
            {
                _onUnbind?.Invoke();
            }
            finally
            {

                _onUnbind = null;
                OnUnregistered();
            }
        }

        protected virtual void OnDestroy()
            => ExecuteUnbind();
    }
    //Logic
    public abstract partial class Registrable<T>
    {
        public void Register(T target, Action<T> binder)
        {
            if (IsAppQuitting || binder == null || _isRegistered)
                return;

            if (ReferenceEquals(target, this))
                return;

            if (target != null)
                target.OnReplaced();

            binder((T)this);
            _onUnbind = () => binder(null);

            FinalizeRegistration();

        }
        public void Register(List<T> container)
        {
            if (IsAppQuitting || container == null || _isRegistered)
                return;

            if (container.Contains((T)this))
                return;

            container.Add((T)this);
            _onUnbind = () => container.Remove((T)this);

            FinalizeRegistration();

        }
        public void Register<TKey>(Dictionary<TKey, T> container, TKey key)
        {
            if (IsAppQuitting || container == null || _isRegistered)
                return;

            if (container.TryGetValue(key, out var existing))
            {
                if (ReferenceEquals(existing, this))
                    return;

                if (existing != null)
                    existing.OnReplaced();
            }

            container[key] = (T)this;
            _onUnbind = () => container.Remove(key);

            FinalizeRegistration();

        }

        public void Unregister()
            => ExecuteUnbind();

        protected virtual void OnReplaced()
            => ExecuteUnbind();

        protected virtual void OnUnregistered() { }
    }
}
