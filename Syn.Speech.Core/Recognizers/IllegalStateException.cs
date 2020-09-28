using System;

namespace Syn.Speech.Recognizers
{
    public class IllegalStateException: Exception
    {
        public IllegalStateException(string message)
            : base(message)
        { }
        public IllegalStateException(Exception ex)
            : base(String.Empty,ex)
        { }
    }
}
