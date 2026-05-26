/*
 *  Comment     :
 */

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace RapidFramework
{
    // Enum
    public partial class LogManager : ManagerBase
    {
        public enum LogLevel
        {
            Info,
            Warning,
            Error
        }
    }

    // Data
    public partial class LogManager
    {
        private string _logDirectory;
        private string _currentLogFile;

        public bool IsLogEnabled { get; set; } = true;
        public bool IsSaveToFile { get; set; } = false;
    }

    // Register
    public partial class LogManager
    {
        public override void Register()
        {
            base.Register();
            _logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
        }
    }

    // Life Cycle
    public partial class LogManager
    {
        public override void Initialize()
        {
            base.Initialize();

            if (IsSaveToFile)
                PrepareLogFile();
        }

        private void PrepareLogFile()
        {
            try
            {
                if (!Directory.Exists(_logDirectory))
                    Directory.CreateDirectory(_logDirectory);

                _currentLogFile = Path.Combine(_logDirectory, $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }

    // Logic
    public partial class LogManager
    {
        public string LogInfo(
            string message, 
            [CallerFilePath] string filePath = "", 
            [CallerMemberName] string memberName = ""
        )
          => WriteLog(LogLevel.Info, message, filePath, memberName);

        public string LogWarning(
            string message, 
            [CallerFilePath] string filePath = "", 
            [CallerMemberName] string memberName = ""
        )
            => WriteLog(LogLevel.Warning, message, filePath, memberName);

        public string LogError(
            string message, 
            [CallerFilePath] string filePath = "", 
            [CallerMemberName] string memberName = ""
        )
            => WriteLog(LogLevel.Error, message, filePath, memberName);

        public string LogInfo<T>(
            string message, 
            [CallerFilePath] string filePath = "", 
            [CallerMemberName] string memberName = ""
        )
          => WriteLog(LogLevel.Info, message, filePath, memberName, typeof(T).Name);

        public string LogWarning<T>(
            string message, 
            [CallerFilePath] string filePath = "",
            [CallerMemberName] string memberName = ""
        )
            => WriteLog(LogLevel.Warning, message, filePath, memberName, typeof(T).Name);

        public string LogError<T>(
            string message, 
            [CallerFilePath] string filePath = "", 
            [CallerMemberName] string memberName = ""
        )
            => WriteLog(LogLevel.Error, message, filePath, memberName, typeof(T).Name);

        private string WriteLog(
            LogLevel level,
            string message,
            string filePath,
            string memberName, 
            string typeName = ""
        )
        {
            if (!IsLogEnabled)
                return string.Empty;

            string className = Path.GetFileNameWithoutExtension(filePath);
            string targetName = string.IsNullOrEmpty(typeName) 
                ? className 
                : typeName;

            string formattedMsg = $"[{targetName}::{memberName}] > {message}";

            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log(formattedMsg); 
                    break;
                case LogLevel.Warning: 
                    Debug.LogWarning(formattedMsg); 
                    break;
                case LogLevel.Error: 
                    Debug.LogError(formattedMsg); 
                    break;
            }

            if (IsSaveToFile && !string.IsNullOrEmpty(_currentLogFile))
                SaveToFile(level, formattedMsg);

            return formattedMsg;
        }

        private void SaveToFile(LogLevel level, string message)
        {
            try
            {
                string logLine = $"[{DateTime.Now:HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(_currentLogFile, logLine, Encoding.UTF8);
            }
            catch { }
        }
    }
}