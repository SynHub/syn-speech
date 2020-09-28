using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syn.Speech.Logging
{
    /// <summary>Static Log Helper Class.</summary>
    public static class Logger
    {
        public static bool IsEnabled { get; set; }

        public static LogLevel Level { get; set; }

        public static event EventHandler<LogReceivedEventArgs> LogReceived;

        static Logger()
        {
            Logger.Level = LogLevel.Info;
            Logger.IsEnabled = true;
        }

        public static void LogError<T>(this T source, object message, params object[] values)
        {
            string str = string.Format((string)message, values);
            Logger.LogMessage<string>(source.GetType().Name, (object)str, LogType.Error);
        }

        public static void LogError<T>(this T source, object message)
        {
            Logger.LogMessage<string>(source.GetType().Name, message, LogType.Error);
        }

        public static void LogError<T>(object message)
        {
            Logger.LogMessage<string>(typeof(T).Name, message, LogType.Error);
        }

        [Conditional("DEBUG")]
        public static void LogDebug<T>(this T source, object message, params object[] values)
        {
            string str = string.Format((string)message, values);
            Logger.LogMessage<string>(source.GetType().Name, (object)str, LogType.Debug);
        }

        [Conditional("DEBUG")]
        public static void LogDebug<T>(this T source, object message)
        {
            Logger.LogMessage<string>(source.GetType().Name, message, LogType.Debug);
        }

        [Conditional("DEBUG")]
        public static void LogDebug<T>(object message)
        {
            Logger.LogMessage<string>(typeof(T).Name, message, LogType.Debug);
        }

        public static void LogWarning<T>(this T source, object message, params object[] values)
        {
            string str = string.Format((string)message, values);
            Logger.LogMessage<string>(source.GetType().Name, (object)str, LogType.Warning);
        }

        public static void LogWarning<T>(this T source, object message)
        {
            Logger.LogMessage<string>(source.GetType().Name, message, LogType.Warning);
        }

        public static void LogWarning<T>(object message)
        {
            Logger.LogMessage<string>(typeof(T).Name, message, LogType.Warning);
        }

        public static void LogInfo<T>(this T source, object message)
        {
            Logger.LogMessage<string>(source.GetType().Name, message, LogType.Info);
        }

        public static void LogInfo<T>(object message)
        {
            Logger.LogMessage<string>(typeof(T).Name, message, LogType.Info);
        }

        private static void LogMessage<T>(T source, object message, LogType type)
        {
            if (!Logger.IsEnabled)
                return;
            string message1 = DateTime.Now.ToString() + " " + (object)type + " " + (object)source + " " + message;
            if (Logger.LogReceived == null)
                return;
            Logger.LogReceived((object)null, new LogReceivedEventArgs(message1, type));
        }
    }
}
