using System;
using System.IO;

namespace WWMBoberRotations.Services
{
    /// <summary>
    /// Simple file-based logger for application events and errors
    /// </summary>
    public static class Logger
    {
        private static readonly string _logFilePath;
        private static readonly object _lock = new();

        static Logger()
        {
            var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var logFileName = $"log_{DateTime.Now:yyyyMMdd}.txt";
            _logFilePath = Path.Combine(logDir, logFileName);
        }

        public static void Info(string message)
        {
            Log("INFO", message);
        }

        public static void Warning(string message)
        {
            Log("WARN", message);
        }

        public static void Error(string message, Exception? ex = null)
        {
            var fullMessage = ex != null 
                ? $"{message}\nException: {ex.GetType().Name}\nMessage: {ex.Message}\nStackTrace: {ex.StackTrace}"
                : message;
            Log("ERROR", fullMessage);
        }

        private static void Log(string level, string message)
        {
            try
            {
                lock (_lock)
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                    var logEntry = $"[{timestamp}] [{level}] {message}{Environment.NewLine}";
                    File.AppendAllText(_logFilePath, logEntry);
                }
            }
            catch
            {
            }
        }
    }
}
