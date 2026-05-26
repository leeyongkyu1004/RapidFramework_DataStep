/*
 *  Comment     : 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace RapidFramework
{
    // Internal Helpers
    public static partial class Guard
    {
        public static bool IsActuallyNull<T>(T obj)
        {
            if (obj == null)
                return true;

            if (obj is UnityEngine.Object unityObj)
                return unityObj == null;

            return false;
        }
        private static string Report<T>(
            LogManager.LogLevel level,
            string userMessage,
            string filePath,
            string memberName,
            string defaultGuardTag
        )
        {
            LogManager log = RapidFramework.Log;

            if (log != null)
            {
                string msg = string.IsNullOrEmpty(userMessage)
                    ? defaultGuardTag 
                    : userMessage;

                return level switch
                {
                    LogManager.LogLevel.Info => log.LogInfo<T>(msg, filePath, memberName),
                    LogManager.LogLevel.Warning => log.LogWarning<T>(msg, filePath, memberName),
                    LogManager.LogLevel.Error => log.LogError<T>(msg, filePath, memberName),
                    _ => string.Empty,
                };
            }

            string typeName = typeof(T).Name;
            string className = System.IO.Path.GetFileNameWithoutExtension(filePath);

            string body = string.IsNullOrEmpty(userMessage)
                                    ? $"{typeName} {defaultGuardTag}"
                                    : userMessage;

            string formatted = $"[{className}::{memberName}] {body}";

            switch (level)
            {
                case LogManager.LogLevel.Warning: Debug.LogWarning(formatted); break;
                case LogManager.LogLevel.Error: Debug.LogError(formatted); break;
                default: Debug.Log(formatted); break;
            }
            return formatted;
        }
    }

    //Null Guards
    public static partial class Guard
    {
        public static bool IsNull<T>(
            this T obj,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class
        {
            if (!IsActuallyNull(obj))
                return false;

            Report<T>(LogManager.LogLevel.Error, log, filePath, memberName, "is null");
            return true;
        }

        public static T AgainstNull<T>(
            this T obj,
            bool autoCreate = false,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class, new()
        {
            if (!IsActuallyNull(obj))
                return obj;

            if (autoCreate)
            {
                obj = new T();
                Report<T>(LogManager.LogLevel.Warning,
                    log ?? $"{typeof(T).Name} auto-created",
                    filePath, memberName, "auto-create");
                return obj;
            }

            Report<T>(LogManager.LogLevel.Error, log, filePath, memberName, "against null");
            return null;
        }

        public static T AgainstNull<T>(
            this T obj,
            bool autoCreate,
            Action<T> action,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class, new()
        {
            var validated = obj.AgainstNull(autoCreate, log, filePath, memberName);

            if (!IsActuallyNull(validated))
                action?.Invoke(validated);

            return validated;
        }

        public static T AgainstNull<T>(
            this T component,
            GameObject target,
            bool autoCreate = false,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : Component
        {
            if (!IsActuallyNull(component))
                return component;

            if (autoCreate && target != null)
            {
                component = target.GetComponent<T>() ?? target.AddComponent<T>();

                Report<T>(
                    LogManager.LogLevel.Warning,
                    log ?? $"{typeof(T).Name} auto-added",
                    filePath,
                    memberName, 
                    "auto-add");

                return component;
            }
            Report<T>(LogManager.LogLevel.Error, log, filePath, memberName, "against null (Component)");
            return null;
        }

        public static T AgainstNull<T>(
            this T component,
            GameObject target,
            bool autoCreate,
            Action<T> action,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : Component
        {
            var validated = component.AgainstNull(target, autoCreate, log, filePath, memberName);

            if (!IsActuallyNull(validated))
                action?.Invoke(validated);

            return validated;
        }

        public static T Ensure<T>(
            this T obj,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class
        {
            if (!IsActuallyNull(obj))
                return obj;

            string message = Report<T>(
                LogManager.LogLevel.Error, log, filePath, memberName, "ensure failed");

            throw new NullReferenceException(message);
        }
    }
    //Logic Array
    public static partial class Guard
    {
        public static bool IsNull<T>(
            this T[] array,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (array != null)
                return false;

            Report<T[]>(LogManager.LogLevel.Error,log,filePath,memberName,"array is null");
            return true;
        }
        public static bool IsNullOrEmpty<T>(
            this T[] array,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (array != null && array.Length > 0)
                return false;

            if (array == null)
            {
                Report<T[]>(LogManager.LogLevel.Error,log,filePath,memberName,"array is null");
                return true;
            }
            Report<T[]>(LogManager.LogLevel.Warning,log,filePath,memberName,"array is empty");
            return true;
        }
        public static bool IsNullElement<T>(
            this T[] array,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (array == null)
            {
                Report<T[]>(LogManager.LogLevel.Error,log,filePath,memberName,"array is null");
                return true;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (IsActuallyNull(array[i]))
                {
                    Report<T[]>(
                        LogManager.LogLevel.Warning, 
                        log ?? $"Null element at index {i}",
                        filePath,
                        memberName,
                        "array contains null");

                    return true;
                }
            }
            return false;
        }

        public static T[] AgainstNullOrEmpty<T>(
            this T[] array,
            bool autoCreate = false,
            int autoSize = 0,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (array != null && array.Length > 0)
                return array;

            if (autoCreate)
            {
                array = new T[autoSize];

                Report<T[]>(
                    LogManager.LogLevel.Warning,
                    log ?? $"Array<{typeof(T).Name}> auto-created",
                    filePath,
                    memberName,
                    "array auto-create");

                return array;
            }

            if (array == null)
                Report<T[]>(LogManager.LogLevel.Error, log, filePath, memberName, "array is null");
            else
                Report<T[]>(LogManager.LogLevel.Warning, log, filePath, memberName, "array is empty");

            return array;
        }

        public static T[] AgainstNullOrEmpty<T>(
            this T[] array,
            bool autoCreate,
            int autoSize,
            Action<T[]> action,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            var validated = array.AgainstNullOrEmpty(
                autoCreate,
                autoSize,
                log,
                filePath,
                memberName);

            if (validated != null && validated.Length > 0)
                action?.Invoke(validated);

            return validated;
        }

        public static T[] Ensure<T>(
            this T[] array,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (array != null && array.Length > 0)
                return array;

            string message = Report<T[]>(LogManager.LogLevel.Error,log,filePath,memberName,"array ensure failed");

            throw new ArgumentException(message);
        }

        public static bool HasNullElement<T>(
            this T[] array,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (array == null)
            {
                Report<T[]>(LogManager.LogLevel.Error,log,filePath,memberName,"array is null");
                return true;
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (IsActuallyNull(array[i]))
                {
                    Report<T[]>(LogManager.LogLevel.Warning,
                        log ?? $"Null element at index {i}",
                        filePath,
                        memberName,
                        "array contains null");

                    return true;
                }
            }
            return false;
        }
    }
    //Logic Collection
    public static partial class Guard
    {
        public static bool IsEmpty<T>(
            this T collection,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class, ICollection
        {
            if (IsActuallyNull(collection))
            {
                Report<T>(LogManager.LogLevel.Error, log, filePath, memberName, "is null (Collection)");
                return true;
            }

            if (collection.Count == 0)
            {
                Report<T>(LogManager.LogLevel.Warning, log, filePath, memberName, "is empty");
                return true;
            }
            return false;
        }

        public static bool IsNullOrEmpty<T>(
          this T collection,
          string log = null,
          [CallerFilePath] string filePath = "",
          [CallerMemberName] string memberName = ""
        ) where T : class, ICollection
        {
            if (IsActuallyNull(collection))
            {
                Report<T>(LogManager.LogLevel.Error, log, filePath, memberName, "collection is null");
                return true;
            }

            if (collection.Count == 0)
            {
                Report<T>(LogManager.LogLevel.Warning, log, filePath, memberName, "collection is empty");
                return true;
            }
            return false;
        }

        public static bool IsNullElement<T>(
            this IEnumerable<T> collection,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (collection == null)
            {
                Report<IEnumerable<T>>(LogManager.LogLevel.Error, log, filePath, memberName, "collection is null");
                return true;
            }

            int index = 0;

            foreach (var element in collection)
            {
                if (IsActuallyNull(element))
                {
                    Report<IEnumerable<T>>(
                        LogManager.LogLevel.Warning,
                        log ?? $"Null element at index {index}",
                        filePath,
                        memberName,
                        "collection contains null");

                    return true;
                }
                index++;
            }
            return false;
        }

        public static T AgainstEmpty<T>(
            this T collection,
            bool autoCreate = false,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class, ICollection, new()
        {
            if (IsActuallyNull(collection))
            {
                if (autoCreate)
                {
                    collection = new T();
                    Report<T>(LogManager.LogLevel.Warning,
                        log ?? $"{typeof(T).Name} auto-created",
                        filePath,
                        memberName, 
                        "auto-create");

                    return collection;
                }
                Report<T>(LogManager.LogLevel.Error, log, filePath, memberName, "is null (Collection)");
                return null;
            }

            if (collection.Count == 0)
                Report<T>(LogManager.LogLevel.Warning, log, filePath, memberName, "is empty");

            return collection;
        }

        public static T AgainstEmpty<T>(
            this T collection,
            bool autoCreate,
            Action<T> action,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        ) where T : class, ICollection, new()
        {
            var validated = collection.AgainstEmpty(autoCreate, log, filePath, memberName);

            if (!IsActuallyNull(validated) && validated.Count > 0)
                action?.Invoke(validated);

            return validated;
        }
    }
    //Logic String
    public static partial class Guard
    {
        public static bool IsNullOrEmpty(
           this string str,
           string log = null,
           [CallerFilePath] string filePath = "",
           [CallerMemberName] string memberName = ""
        )
        {
            if (!string.IsNullOrEmpty(str))
                return false;

            Report<string>(LogManager.LogLevel.Error, log, filePath, memberName, "is null or empty");
            return true;
        }

        public static string AgainstNullOrEmpty(
            this string str,
            string fallback = null,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (!string.IsNullOrEmpty(str))
                return str;

            if (fallback != null)
            {
                Report<string>(LogManager.LogLevel.Warning,
                    log ?? $"string fallback: \"{fallback}\"",
                    filePath,
                    memberName, 
                    "fallback used");

                return fallback;
            }
            Report<string>(LogManager.LogLevel.Error, log, filePath, memberName, "against null or empty");
            return null;
        }

        public static string Ensure(
            this string str,
            string log = null,
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
        {
            if (!string.IsNullOrEmpty(str))
                return str;

            string message = Report<string>(
                LogManager.LogLevel.Error, log, filePath, memberName, "ensure failed");

            throw new ArgumentException(message);
        }
    }
}