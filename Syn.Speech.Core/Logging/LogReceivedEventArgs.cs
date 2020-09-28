using System;

namespace Syn.Speech.Logging
{
    public class LogReceivedEventArgs : EventArgs
    {
        public string Message { get; set; }

        public LogType Type { get; set; }

        public LogReceivedEventArgs(string message, LogType type)
        {
            this.Message = message;
            this.Type = type;
        }
    }
}
