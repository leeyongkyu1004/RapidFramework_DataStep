/*
 *  Comment     :
 */

using System;
using UnityEngine;

namespace RapidFramework
{
    public static class AwaitableExtensions
    {
        public static async void Forget(this Awaitable awaitable)
        {
            try
            {
                await awaitable;
            }
            catch (OperationCanceledException){}
            catch (Exception ex)
            {
                RapidFramework.Log.LogError(ex.Message);                
            }
        }
    }
}
