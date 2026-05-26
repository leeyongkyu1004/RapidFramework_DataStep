/*
 *  Comment     :
 */

using System;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace RapidFramework
{
    //Data    
    public partial class SceneManager : ManagerBase
    {
    
    }
    //Life Cycle
    public partial class SceneManager 
    {

    }
    //Logic
    public partial class SceneManager 
    {
       
    }
    //Load
    public partial class SceneManager 
    {
        public void Load(string sceneName)
            => UnitySceneManager.LoadScene(sceneName);
    }
}
