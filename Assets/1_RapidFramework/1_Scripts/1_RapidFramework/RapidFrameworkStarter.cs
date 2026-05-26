/*
 *  Comment     :
 */

using UnityEngine;

namespace RapidFramework
{
    //Life Cycle
    [DefaultExecutionOrder(-1000)]
    public class RapidFrameworkStarter : MonoBehaviour
    {
        private void Awake()
        {
            var rapidFramework = RapidFramework.Instance;

            if(!rapidFramework.IsNull())
                rapidFramework.Activate();

            Destroy(gameObject);
        }            
    }

}
