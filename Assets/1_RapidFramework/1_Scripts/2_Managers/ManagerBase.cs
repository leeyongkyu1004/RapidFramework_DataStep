/*
 *  Comment     : RF Base Manager
 */

using UnityEngine;

namespace RapidFramework
{
    //Register        
    public abstract partial class ManagerBase : MonoBehaviour
    {
        public virtual void Register() { }
        public virtual void Unregister() { }
    }
    //Life Cycle
    public abstract partial class ManagerBase
    {
        public virtual void Initialize() {}       
    }
}
