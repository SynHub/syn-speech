using System;
using System.IO;

//EXTERNAL
namespace Syn.Speech.Helper
{
    public class LineNumberReader: TextReader
    {
        private readonly TextReader _reader;
        private int _cur;

        public LineNumberReader(TextReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            _reader = reader;
        }

        public int Line { get; private set; }

        public override int Peek()
        {
            return _reader.Peek();
        }

        public override int Read()
        {
            var b = _reader.Read();
            if ((_cur == '\n') || (_cur == '\r' && b != '\n')) Line++;
            return _cur = b;
        }

        public override string ReadLine()
        {
            var retLine = _reader.ReadLine();
            Line++;
            return retLine;
        }

        public int LineNumber
        {
            get { return Line; }
        }

        public override void Close()
        {
            if (_reader != null)
                _reader.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _reader.Dispose();
        }
    }
}
