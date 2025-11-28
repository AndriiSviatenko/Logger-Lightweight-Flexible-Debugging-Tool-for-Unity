using UnityEngine;
using System.Runtime.CompilerServices;
using System.IO;
using System;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Azen.Logger
{
    public static class CustomLogger
    {
        private const string DefaultEmoji = "📝";
        private const string DefaultColor = "#FFFFFF";
        private const int StringBuilderInitialCapacity = 256;

        private static readonly object LockObject = new object();
        private static readonly Regex RichTextRegex = new Regex("<.*?>", RegexOptions.Compiled);

        private static readonly Dictionary<string, string> FileNameCache = new Dictionary<string, string>(32);
        private static readonly StringBuilder SharedStringBuilder = new StringBuilder(StringBuilderInitialCapacity);

        private static string logFilePath;
        private static string lastMessage = "";
        private static int repeatCount = 0;
        private static bool initialized = false;

        public static LoggerConfig Config { get; set; }

        private static void Initialize()
        {
            if (initialized) return;

            lock (LockObject)
            {
                if (initialized) return;

                var directory = Path.Combine(Application.persistentDataPath, "Logs");
                Directory.CreateDirectory(directory);

                var fileName = $"DebugLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
                logFilePath = Path.Combine(directory, fileName);

                initialized = true;
            }
        }

        #region Public Logging API
        public static void Log(string message, LogCategory category = LogCategory.None,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(message, category, LogType.Log, filePath, memberName, lineNumber);
        }

        public static void LogWarning(string message, LogCategory category = LogCategory.Warning,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(message, category, LogType.Warning, filePath, memberName, lineNumber);
        }

        public static void LogError(string message, LogCategory category = LogCategory.Error,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(message, category, LogType.Error, filePath, memberName, lineNumber);
        }
        #endregion

        #region Object Overloads
        public static void Log(object obj, LogCategory category = LogCategory.Other,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(obj?.ToString(), category, LogType.Log, filePath, memberName, lineNumber);
        }

        public static void LogWarning(object obj, LogCategory category = LogCategory.Warning,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(obj?.ToString(), category, LogType.Warning, filePath, memberName, lineNumber);
        }

        public static void LogError(object obj, LogCategory category = LogCategory.Error,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal(obj?.ToString(), category, LogType.Error, filePath, memberName, lineNumber);
        }
        #endregion

        #region Markers
        public static void Mark(string label = "MARK", LogCategory category = LogCategory.None,
            [CallerFilePath] string filePath = "", [CallerMemberName] string memberName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            LogInternal($"=== {label} ===", category, LogType.Log, filePath, memberName, lineNumber);
        }
        #endregion

        #region Core Implementation
        private static void LogInternal(string message, LogCategory category, LogType type,
            string filePath, string memberName, int lineNumber)
        {
            if (ShouldSkipLog(category)) return;

            Initialize();

            var (emoji, color) = GetLogDecorators(category);
            var fileName = GetCachedFileName(filePath);

            var finalMessage = FormatMessage(message, emoji, color, category, fileName, memberName, lineNumber);
            HandleMessageRepeats(finalMessage, type);
        }

        private static bool ShouldSkipLog(LogCategory category)
        {
            if (Config == null) return false;

            return !Config.EnableAllLogs ||
                   !Config.IsCategoryEnabled(category) ||
                   (!Application.isEditor && Config.EditorOnly);
        }

        private static (string emoji, string color) GetLogDecorators(LogCategory category)
        {
            if (Config == null) return (DefaultEmoji, DefaultColor);

            return (
                Config.GetEmoji(category) ?? DefaultEmoji,
                Config.GetHexColor(category) ?? DefaultColor
            );
        }

        private static string GetCachedFileName(string filePath)
        {
            if (FileNameCache.TryGetValue(filePath, out var cached))
                return cached;

            var fileName = Path.GetFileNameWithoutExtension(filePath);

            if (FileNameCache.Count > 100)
                FileNameCache.Clear();

            FileNameCache[filePath] = fileName;
            return fileName;
        }

        private static string FormatMessage(string message, string emoji, string color,
            LogCategory category, string fileName, string memberName, int lineNumber)
        {
            lock (SharedStringBuilder)
            {
                SharedStringBuilder.Clear();

                SharedStringBuilder.Append(emoji);
                SharedStringBuilder.Append(" <color=");
                SharedStringBuilder.Append(color);
                SharedStringBuilder.Append(">[");
                SharedStringBuilder.Append(category.ToString());
                SharedStringBuilder.Append("]</color> ");
                SharedStringBuilder.Append(message);
                SharedStringBuilder.Append("\n<color=#888888><i>");
                SharedStringBuilder.Append(fileName);
                SharedStringBuilder.Append('.');
                SharedStringBuilder.Append(memberName);
                SharedStringBuilder.Append(':');
                SharedStringBuilder.Append(lineNumber);
                SharedStringBuilder.Append("</i></color>");

                return SharedStringBuilder.ToString();
            }
        }

        private static void HandleMessageRepeats(string finalMessage, LogType type)
        {
            lock (LockObject)
            {
                if (finalMessage == lastMessage)
                {
                    repeatCount++;
                    return;
                }

                FlushRepeatedMessages(type);
                OutputMessage(finalMessage, type);
                WriteToFile(finalMessage);

                lastMessage = finalMessage;
            }
        }

        private static void FlushRepeatedMessages(LogType type)
        {
            if (repeatCount > 0)
            {
                var repeatMessage = $"🔁 (repeated {repeatCount}x) {lastMessage}";
                OutputMessage(repeatMessage, type);
                WriteToFile(repeatMessage);
                repeatCount = 0;
            }
        }

        private static void OutputMessage(string message, LogType type)
        {
            switch (type)
            {
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }

        private static void WriteToFile(string message)
        {
            try
            {
                var plainText = StripRichTextTags(message);
                File.AppendAllText(logFilePath, $"{plainText}\n");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Logger] File write failed: {ex.Message}");
            }
        }

        private static string StripRichTextTags(string input)
        {
            return RichTextRegex.Replace(input, string.Empty);
        }

        /// <summary>
        /// Очищає кеш файлових імен (для тестування або оптимізації пам'яті)
        /// </summary>
        public static void ClearCache()
        {
            lock (LockObject)
            {
                FileNameCache.Clear();
            }
        }
        #endregion
    }
}