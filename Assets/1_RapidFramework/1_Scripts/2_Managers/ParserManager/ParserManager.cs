/*
 *  Comment     :
 */

using UnityEngine;

namespace RapidFramework
{
    //Data
    public partial class ParserManager : ManagerBase
    {
        public JsonService Json { get; private set; } = new();
    }
    //LifeCycle
    public partial class ParserManager
    {        
        public override void Initialize()
        {
            base.Initialize();

            Json.Initialize();
        }
    }
}
