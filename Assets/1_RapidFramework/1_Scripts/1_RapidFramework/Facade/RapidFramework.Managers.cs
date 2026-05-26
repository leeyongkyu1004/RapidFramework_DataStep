/*
 *  Comment     : 이 코드는 RapidFrameworkFacade에 의해 자동 생성되었습니다. 
 *                수동으로 수정하지 마십시오.
 */

namespace RapidFramework
{
    public partial class RapidFramework
    {
        private LogManager _log;
        private LogManager LogInternal => _log ??= GetManager<LogManager>();
        public static LogManager Log => Instance.LogInternal;

        private ParserManager _parser;
        private ParserManager ParserInternal => _parser ??= GetManager<ParserManager>();
        public static ParserManager Parser => Instance.ParserInternal;

        private DataManager _data;
        private DataManager DataInternal => _data ??= GetManager<DataManager>();
        public static DataManager Data => Instance.DataInternal;

        private SceneManager _scene;
        private SceneManager SceneInternal => _scene ??= GetManager<SceneManager>();
        public static SceneManager Scene => Instance.SceneInternal;


        private void ClearCache()
        {
            _log = null;
            _parser = null;
            _data = null;
            _scene = null;
        }
    }
}