/*
 *  Comment     :
 */

using UnityEngine;
using TMPro;
using Unity.VisualScripting;

namespace RapidFramework
{
    //Data
    public partial class SampleCode : MonoBehaviour
    {        
        [SerializeField] private TextDataBinder _infoValue;
        [SerializeField] private TMP_InputField _inputField;

        //Observable
        private Observable<string> _input = new(string.Empty);
        [SerializeField] private TMP_Text _observableText;
    }
    //Life Cycle
    public partial class SampleCode
    {
        private void Start()
            => Initialize();

        private void Initialize()
            => AddListeners();

        private void AddListeners()
        {
            _input.AddListener(RefreshObservableText);
        }
        private void RemoveListeners()
        {
            _input.RemoveListener(RefreshObservableText);
        }
        private void OnDestroy()
            => RemoveListeners();
    }
    //Logic
    public partial class SampleCode
    {
        public void OnRefreshInfoValue()
        {
            //Early return (Guard Clause)
            //Pure Code
            //if (_inputField == null || string.IsNullOrEmpty(_inputField.text))
            //    return;
            
            //Guard Code
            if (_inputField.IsNull() || _inputField.text.IsNullOrEmpty())
                return;

            //InputField
            _infoValue.Refresh(_inputField.text);

            //Observable
            _input.Set(_inputField.text);
        }

        public void OnChangeLanguageEng()
            => RapidFramework.Data.Local.Settings.SetLanguage("en").Forget();

        public void OnChangeLanguageKor()
            => RapidFramework.Data.Local.Settings.SetLanguage("ko").Forget();

        //Observable
        private void RefreshObservableText(string text)
            => _observableText.text = text;
    }

}
