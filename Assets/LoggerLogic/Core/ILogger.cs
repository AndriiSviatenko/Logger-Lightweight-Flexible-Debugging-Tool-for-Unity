using System.Runtime.CompilerServices;

namespace Azen.Logger
{
    namespace Azen.Logger
    {
        /// <summary>
        /// Базовий інтерфейс для всіх логерів
        /// </summary>
        public interface ILogger
        {
            void Log(string message, LogCategory category = LogCategory.None,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0);

            void LogWarning(string message, LogCategory category = LogCategory.Warning,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0);

            void LogError(string message, LogCategory category = LogCategory.Error,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0);

            void Mark(string label = "MARK", LogCategory category = LogCategory.None,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0);
        }
    }
}

