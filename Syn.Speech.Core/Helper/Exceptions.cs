using System;

namespace Syn.Speech.Helper
{

    public class SAXParseException : Exception { }

    public class IllegalStateException : Exception
    {
        public IllegalStateException(string message) : base(message) { }
        public IllegalStateException(Exception exception) : base(String.Empty, exception) { }
    }

    public class VirtualMachineError : Error
    {
    }

    public class StackOverflowError : VirtualMachineError
    {
        public StackOverflowError()
        {
        }
    }

    public class BrokenBarrierException : Exception
    {
    }

    internal class BufferUnderflowException : Exception
    {
    }

    public class CharacterCodingException : Exception
    {
    }

    public class DataFormatException : Exception
    {
    }

    public class EOFException : Exception
    {
        public EOFException()
        {
        }

        public EOFException(string msg)
            : base(msg)
        {
        }
    }

    public class Error : Exception
    {
        public Error()
        {
        }

        public Error(Exception ex)
            : base("Runtime Exception", ex)
        {
        }

        public Error(string msg)
            : base(msg)
        {
        }

        public Error(string msg, Exception ex)
            : base(msg, ex)
        {
        }
    }

    public class ExecutionException : Exception
    {
        public ExecutionException(Exception inner)
            : base("Execution failed", inner)
        {
        }
    }

    public class InstantiationException : Exception
    {
    }

    public class InterruptedIOException : Exception
    {
        public InterruptedIOException(string msg)
            : base(msg)
        {
        }
    }

    public class MissingResourceException : Exception
    {
    }

    public class NoSuchAlgorithmException : Exception
    {
    }

    public class NoSuchElementException : Exception
    {
    }

    internal class NoSuchMethodException : Exception
    {
    }

    internal class OverlappingFileLockException : Exception
    {
    }

    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string msg, int errorOffset)
            : base(string.Format("Msg: {0}. Error Offset: {1}", msg, errorOffset))
        {
        }
    }

    public class RuntimeException : Exception
    {
        public RuntimeException()
        {
        }

        public RuntimeException(Exception ex)
            : base("Runtime Exception", ex)
        {
        }

        public RuntimeException(string msg)
            : base(msg)
        {
        }

        public RuntimeException(string msg, Exception ex)
            : base(msg, ex)
        {
        }
    }

    internal class StringIndexOutOfBoundsException : Exception
    {
    }

    internal class UnknownHostException : Exception
    {
        public UnknownHostException()
        {
        }

        public UnknownHostException(Exception ex)
            : base("Host not found", ex)
        {
        }
    }

    internal class UnsupportedEncodingException : Exception
    {
    }

    internal class URISyntaxException : Exception
    {
        public URISyntaxException(string s, string msg)
            : base(s + " " + msg)
        {
        }
    }

    internal class ZipException : Exception
    {
    }

    public class GitException : Exception
    {
    }

    class ConnectException : Exception
    {
        public ConnectException(string msg)
            : base(msg)
        {
        }
    }

    class KeyManagementException : Exception
    {
    }

    class IllegalCharsetNameException : Exception
    {
        public IllegalCharsetNameException(string msg)
            : base(msg)
        {
        }
    }

    class UnsupportedCharsetException : Exception
    {
        public UnsupportedCharsetException(string msg)
            : base(msg)
        {
        }
    }
}
