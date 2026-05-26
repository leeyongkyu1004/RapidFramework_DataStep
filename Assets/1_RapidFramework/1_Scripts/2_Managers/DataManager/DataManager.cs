/*
 *  Comment     :
 */

using UnityEngine;

namespace RapidFramework
{
    //Data
    public partial class DataManager : ManagerBase
    {
        public LocalDataService Local { get; private set; } = new();
    }
    //Register
    public partial class DataManager
    {
        public override void Register()
        {
            base.Register();
            Local.Register();
        }
        public override void Unregister()
        {
            Local.ClearCache();
            base.Unregister();
        }
    }
    //Life Cycle
    public partial class DataManager 
    {
        public override void Initialize()
        {
            base.Initialize();
            Local.Initialize();        
        }        
    }
}
