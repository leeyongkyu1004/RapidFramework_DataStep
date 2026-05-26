/*
 *  Comment     :
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static RapidFramework.TextDataProvider;

namespace RapidFramework
{
    [RequireComponent(typeof(TMP_Text))]
    [AddComponentMenu("RapidFramework/Text Data Binder")]    
    //Data
    public partial class TextDataBinder : Registrable<TextDataBinder> , ITextRefreshable
    {
        [Header("Data Configuration")]
        [SerializeField] private string _textId;

        [Header("Initial Format Args")]
        [SerializeField] private string[] _InitialFormatArgs;

        private TMP_Text _textMesh;
        private static TextDataProvider _provider;

        private object[] _formatArgs;
        private string _lastRawValue;
        private readonly List<Action> _unsubscribers = new(4);

        private static readonly StringBuilder _sb = new StringBuilder(256);
    }
    //Life Cycle
    public partial class TextDataBinder
    {
        private void Start()
        {
            if (!GetProvider().IsNull($" id {_textId} / {gameObject.name}"))
            {
                this.Register(null, (binder) =>
                {
                    _provider.Bind(this);
                });
            }
        }
        public override void Initialize()
        {
            base.Initialize();

            _textMesh = GetComponent<TMP_Text>();

            if(_InitialFormatArgs != null && _InitialFormatArgs.Length > 0)
            {
                if (_formatArgs == null || _formatArgs.Length != _InitialFormatArgs.Length)
                    _formatArgs = new object[_InitialFormatArgs.Length];

                Array.Copy(_InitialFormatArgs, _formatArgs, _InitialFormatArgs.Length);
            }
           
            Refresh();
        }
        protected override void OnUnregistered()
        {
            base.OnUnregistered();
            Unbind();

            if(_provider != null)
                _provider.Unbind(this);
        }
    }
    //Logic
    public partial class TextDataBinder
    {
        public TextDataProvider GetProvider()
        {
            if (_provider == null)
                _provider = RapidFramework.Data.Local.Text;

            return _provider;
        }


        public void Bind(params object[] targets)
        {
            Unbind();

            if (targets.IsNullOrEmpty())
                return;

            if (_formatArgs == null || _formatArgs.Length != targets.Length)
                _formatArgs = new object[targets.Length];

            for (int i = 0; i < targets.Length; i++)
            {
                int index = i;
                var target = targets[i];

                if (target is Observable.IField observable)
                {
                    _formatArgs[index] = observable.GetValue();

                    Action<object> action = (val) =>
                    {
                        if (object.Equals(_formatArgs[index], val))
                            return;

                        _formatArgs[index] = val;
                        Refresh();
                    };

                    observable.AddListenerObject(action, true);
                    _unsubscribers.Add
                        (
                            () => observable.RemoveListenerObject(action)
                        );

                }
                else
                {
                    _formatArgs[index] = target;
                }

            }
            Refresh();

        }

        public void Unbind()
        {
            for (int i = 0; i < _unsubscribers.Count; i++)
            {
                _unsubscribers[i]?.Invoke();
            }

            _unsubscribers.Clear();
        }
        public void Refresh()
            => UpdateText(_formatArgs);

        public void Refresh(params object[] args)
        {
            _formatArgs = args;
            Refresh();
        }
        private void UpdateText(object[] args)
        {
            if (_provider.IsNull() || _textMesh.IsNull() || _textId.IsNullOrEmpty())
                return;

            var row = _provider.TextTable.Get(_textId);

            if (row.IsNull())
            {
#if UNITY_EDITOR
                _textMesh.text = $"<{_textId}> is error";
#endif
                RapidFramework.Log.LogWarning($"Missing Text ID: {_textId} on {gameObject.name}");
                return;
            }

            if (args == null && _lastRawValue == row.Value)
                return;

            _lastRawValue = row.Value;

            if (args != null && args.Length != 0)
            {
                object[] processed = ArrayPool<object>.Shared.Rent(args.Length);

                try
                {
                    ProcessFormatArgs(args,processed);

                    _sb.Clear();
                    _sb.AppendFormat(row.Value, processed);

                    _textMesh.SetText(_sb);
                }
                catch (FormatException fex)
                {
                    _textMesh.text = row.Value;
                    RapidFramework.Log.LogError($"Format Error : {_textId} / {fex.Message}");
                }
                finally
                {
                    ArrayPool<object>.Shared.Return(processed, true);
                }
            }
            else
            {
                _textMesh.text = row.Value;
            }
        }
        private void ProcessFormatArgs(object[] args, object[] dest)
        {            
            ReadOnlySpan<object> span = args;
            
            for(int i = 0; i < span.Length; i++)
            {
                if (span[i] is string id && _provider.TextTable.Contains(id))
                    dest[i] = _provider.TextTable.Get(id).Value;
                else
                    dest[i] = span[i];
            }
        }
        public void SetId(string newId)
        {
            if (_textId == newId)
                return;

            _textId = newId;
            _lastRawValue = null;

            Refresh();
        }
    }
}
