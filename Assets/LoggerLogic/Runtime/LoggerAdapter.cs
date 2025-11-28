using System.Runtime.CompilerServices;

namespace Azen.Logger
{
    namespace Azen.Logger
    {
        /// <summary>
        /// Адаптер для CustomLogger, що реалізує інтерфейс ILogger
        /// Дозволяє використовувати dependency injection та заміну реалізації
        /// </summary>
        public class LoggerAdapter : ILogger
        {
            private readonly LoggerConfig config;

            public LoggerAdapter(LoggerConfig config)
            {
                this.config = config;
                CustomLogger.Config = config;
            }

            public void Log(string message, LogCategory category = LogCategory.None,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                CustomLogger.Log(message, category, filePath, memberName, lineNumber);
            }

            public void LogWarning(string message, LogCategory category = LogCategory.Warning,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                CustomLogger.LogWarning(message, category, filePath, memberName, lineNumber);
            }

            public void LogError(string message, LogCategory category = LogCategory.Error,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                CustomLogger.LogError(message, category, filePath, memberName, lineNumber);
            }

            public void Mark(string label = "MARK", LogCategory category = LogCategory.None,
                [CallerFilePath] string filePath = "",
                [CallerMemberName] string memberName = "",
                [CallerLineNumber] int lineNumber = 0)
            {
                CustomLogger.Mark(label, category, filePath, memberName, lineNumber);
            }
        }
    }
}

