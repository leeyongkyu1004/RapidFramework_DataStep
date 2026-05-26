/*
 *  Comment     : 이 코드는 LocalDataServiceFacade에 의해 자동 생성되었습니다.
 */

namespace RapidFramework
{
    public partial class LocalDataService
    {
        private SettingsDataProvider _settings;
        public SettingsDataProvider Settings => _settings ??= GetDataProvider<SettingsDataProvider>();

        private TextDataProvider _text;
        public TextDataProvider Text => _text ??= GetDataProvider<TextDataProvider>();


        public void ClearCache()
        {
            _settings = null;
            _text = null;
        }
    }
}