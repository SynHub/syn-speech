using System;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Parser
{
    /// <summary>
    /// An implementation of interface CharStream, 
    /// where the stream is assumed to contain only ASCII characters (with java-like unicode escape processing).
    /// </summary>
    public class JavaCharStream
    {
        /// <summary>
        /// Whether parser is static.
        /// </summary>
        public const bool StaticFlag = false;

        static int Hexval(char c)
        {
            switch (c)
            {
                case '0':
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;

                case 'a':
                case 'A':
                    return 10;
                case 'b':
                case 'B':
                    return 11;
                case 'c':
                case 'C':
                    return 12;
                case 'd':
                case 'D':
                    return 13;
                case 'e':
                case 'E':
                    return 14;
                case 'f':
                case 'F':
                    return 15;
            }

            throw new IOException(); // Should never come here
        }

        /** Position in buffer. */
        public int Bufpos = -1;
        int _bufsize;
        int _available;
        int _tokenBegin;
        protected int[] Bufline;
        protected int[] Bufcolumn;

        protected int Column = 0;
        protected int Line = 1;

        protected bool PrevCharIsCr = false;
        protected bool PrevCharIsLf = false;

        protected TextReader InputStream;

        protected char[] NextCharBuf;
        protected char[] Buffer;
        protected int MaxNextCharInd = 0;
        protected int NextCharInd = -1;
        protected int InBuf = 0;
        protected int TabSize = 8;

        protected void SetTabSize(int i) { TabSize = i; }
        protected int GetTabSize(int i) { return TabSize; }

        protected void ExpandBuff(bool wrapAround)
        {
            char[] newbuffer = new char[_bufsize + 2048];
            int[] newbufline = new int[_bufsize + 2048];
            int[] newbufcolumn = new int[_bufsize + 2048];

            try
            {
                if (wrapAround)
                {
                    Array.Copy(Buffer, _tokenBegin, newbuffer, 0, _bufsize - _tokenBegin);
                    Array.Copy(Buffer, 0, newbuffer, _bufsize - _tokenBegin, Bufpos);
                    Buffer = newbuffer;

                    Array.Copy(Bufline, _tokenBegin, newbufline, 0, _bufsize - _tokenBegin);
                    Array.Copy(Bufline, 0, newbufline, _bufsize - _tokenBegin, Bufpos);
                    Bufline = newbufline;

                    Array.Copy(Bufcolumn, _tokenBegin, newbufcolumn, 0, _bufsize - _tokenBegin);
                    Array.Copy(Bufcolumn, 0, newbufcolumn, _bufsize - _tokenBegin, Bufpos);
                    Bufcolumn = newbufcolumn;

                    Bufpos += (_bufsize - _tokenBegin);
                }
                else
                {
                    Array.Copy(Buffer, _tokenBegin, newbuffer, 0, _bufsize - _tokenBegin);
                    Buffer = newbuffer;

                    Array.Copy(Bufline, _tokenBegin, newbufline, 0, _bufsize - _tokenBegin);
                    Bufline = newbufline;

                    Array.Copy(Bufcolumn, _tokenBegin, newbufcolumn, 0, _bufsize - _tokenBegin);
                    Bufcolumn = newbufcolumn;

                    Bufpos -= _tokenBegin;
                }
            }
            catch (Exception t)
            {
                throw new Error(t.Message);
            }

            _available = (_bufsize += 2048);
            _tokenBegin = 0;
        }

        protected void FillBuff()
        {
            int i;
            if (MaxNextCharInd == 4096)
                MaxNextCharInd = NextCharInd = 0;

            try
            {
                if ((i = InputStream.Read(NextCharBuf, MaxNextCharInd, 4096 - MaxNextCharInd)) == 0)//Java returns -1 whereas C# returns 0 on stream end.
                {
                    InputStream.Close();
                    throw new IOException();
                }
                else
                    MaxNextCharInd += i;
                return;
            }
            catch (IOException e)
            {
                if (Bufpos != 0)
                {
                    --Bufpos;
                    Backup(0);
                }
                else
                {
                    Bufline[Bufpos] = Line;
                    Bufcolumn[Bufpos] = Column;
                }
                throw e;
            }
        }

        protected char ReadByte()
        {
            if (++NextCharInd >= MaxNextCharInd)
                FillBuff();

            return NextCharBuf[NextCharInd];
        }

        /** @return starting character for token. */
        public char BeginToken()
        {
            if (InBuf > 0)
            {
                --InBuf;

                if (++Bufpos == _bufsize)
                    Bufpos = 0;

                _tokenBegin = Bufpos;
                return Buffer[Bufpos];
            }

            _tokenBegin = 0;
            Bufpos = -1;

            return ReadChar();
        }

        protected void AdjustBuffSize()
        {
            if (_available == _bufsize)
            {
                if (_tokenBegin > 2048)
                {
                    Bufpos = 0;
                    _available = _tokenBegin;
                }
                else
                    ExpandBuff(false);
            }
            else if (_available > _tokenBegin)
                _available = _bufsize;
            else if ((_tokenBegin - _available) < 2048)
                ExpandBuff(true);
            else
                _available = _tokenBegin;
        }

        protected void UpdateLineColumn(char c)
        {
            Column++;

            if (PrevCharIsLf)
            {
                PrevCharIsLf = false;
                Line += (Column = 1);
            }
            else if (PrevCharIsCr)
            {
                PrevCharIsCr = false;
                if (c == '\n')
                {
                    PrevCharIsLf = true;
                }
                else
                    Line += (Column = 1);
            }

            switch (c)
            {
                case '\r':
                    PrevCharIsCr = true;
                    break;
                case '\n':
                    PrevCharIsLf = true;
                    break;
                case '\t':
                    Column--;
                    Column += (TabSize - (Column % TabSize));
                    break;
                default:
                    break;
            }

            Bufline[Bufpos] = Line;
            Bufcolumn[Bufpos] = Column;
        }

        /** Read a character. */
        public char ReadChar()
        {
            if (InBuf > 0)
            {
                --InBuf;

                if (++Bufpos == _bufsize)
                    Bufpos = 0;

                return Buffer[Bufpos];
            }

            char c;

            if (++Bufpos == _available)
                AdjustBuffSize();

            if ((Buffer[Bufpos] = c = ReadByte()) == '\\')
            {
                UpdateLineColumn(c);

                int backSlashCnt = 1;

                for (; ; ) // Read all the backslashes
                {
                    if (++Bufpos == _available)
                        AdjustBuffSize();

                    try
                    {
                        if ((Buffer[Bufpos] = c = ReadByte()) != '\\')
                        {
                            UpdateLineColumn(c);
                            // found a non-backslash char.
                            if ((c == 'u') && ((backSlashCnt & 1) == 1))
                            {
                                if (--Bufpos < 0)
                                    Bufpos = _bufsize - 1;

                                break;
                            }

                            Backup(backSlashCnt);
                            return '\\';
                        }
                    }
                    catch (IOException)
                    {
                        // We are returning one backslash so we should only backup (count-1)
                        if (backSlashCnt > 1)
                            Backup(backSlashCnt - 1);

                        return '\\';
                    }

                    UpdateLineColumn(c);
                    backSlashCnt++;
                }

                // Here, we have seen an odd number of backslash's followed by a 'u'
                try
                {
                    while ((c = ReadByte()) == 'u')
                        ++Column;

                    Buffer[Bufpos] = c = (char)(Hexval(c) << 12 |
                                                Hexval(ReadByte()) << 8 |
                                                Hexval(ReadByte()) << 4 |
                                                Hexval(ReadByte()));

                    Column += 4;
                }
                catch (IOException e)
                {
                    throw new Error("Invalid escape character at line " + Line +
                                                     " column " + Column + ".");
                }

                if (backSlashCnt == 1)
                    return c;
                else
                {
                    Backup(backSlashCnt - 1);
                    return '\\';
                }
            }
            else
            {
                UpdateLineColumn(c);
                return c;
            }
        }

        [Obsolete("use GetEndColumn()")]
        public int GetColumn()
        {
            return Bufcolumn[Bufpos];
        }

        [Obsolete("use GetEndLine()")]
        public int GetLine()
        {
            return Bufline[Bufpos];
        }

        /** Get end column. */
        public int GetEndColumn()
        {
            return Bufcolumn[Bufpos];
        }

        /** Get end line. */
        public int GetEndLine()
        {
            return Bufline[Bufpos];
        }

        /** @return column of token start */
        public int GetBeginColumn()
        {
            return Bufcolumn[_tokenBegin];
        }

        /** @return line number of token start */
        public int GetBeginLine()
        {
            return Bufline[_tokenBegin];
        }

        /** Retreat. */
        public void Backup(int amount)
        {

            InBuf += amount;
            if ((Bufpos -= amount) < 0)
                Bufpos += _bufsize;
        }

        /** Constructor. */
        public JavaCharStream(TextReader dstream, int startline, int startcolumn, int buffersize)
        {
            InputStream = dstream;
            Line = startline;
            Column = startcolumn - 1;

            _available = _bufsize = buffersize;
            Buffer = new char[buffersize];
            Bufline = new int[buffersize];
            Bufcolumn = new int[buffersize];
            NextCharBuf = new char[4096];
        }

        /** Constructor. */
        public JavaCharStream(TextReader dstream, int startline, int startcolumn)
            : this(dstream, startline, startcolumn, 4096)
        {

        }

        /** Constructor. */
        public JavaCharStream(TextReader dstream)
            : this(dstream, 1, 1, 4096)
        {

        }
        /** Reinitialise. */
        public void ReInit(TextReader dstream, int startline, int startcolumn, int buffersize)
        {
            InputStream = dstream;
            Line = startline;
            Column = startcolumn - 1;

            if (Buffer == null || buffersize != Buffer.Length)
            {
                _available = _bufsize = buffersize;
                Buffer = new char[buffersize];
                Bufline = new int[buffersize];
                Bufcolumn = new int[buffersize];
                NextCharBuf = new char[4096];
            }
            PrevCharIsLf = PrevCharIsCr = false;
            _tokenBegin = InBuf = MaxNextCharInd = 0;
            NextCharInd = Bufpos = -1;
        }

        /** Reinitialise. */
        public void ReInit(TextReader dstream, int startline, int startcolumn)
        {
            ReInit(dstream, startline, startcolumn, 4096);
        }

        /** Reinitialise. */
        public void ReInit(TextReader dstream)
        {
            ReInit(dstream, 1, 1, 4096);
        }
        /** Constructor. */
        public JavaCharStream(Stream dstream, String encoding, int startline, int startcolumn, int buffersize) :
            this(encoding == null ? new StreamReader(dstream) : new StreamReader(dstream, Encoding.GetEncoding(encoding)/*//TODO: Check behaviour*/), startline, startcolumn, buffersize)
        {

        }

        /** Constructor. */
        public JavaCharStream(Stream dstream, int startline, int startcolumn, int buffersize)
            : this(new StreamReader(dstream), startline, startcolumn, 4096)
        {

        }

        /** Constructor. */
        public JavaCharStream(Stream dstream, String encoding, int startline, int startcolumn)
            : this(dstream, encoding, startline, startcolumn, 4096)
        {

        }

        /** Constructor. */
        public JavaCharStream(Stream dstream, int startline, int startcolumn)
            : this(dstream, startline, startcolumn, 4096)
        {

        }

        /** Constructor. */
        public JavaCharStream(Stream dstream, String encoding)
            : this(dstream, encoding, 1, 1, 4096)
        {

        }

        /** Constructor. */
        public JavaCharStream(Stream dstream)
            : this(dstream, 1, 1, 4096)
        {

        }

        /** Reinitialise. */
        public void ReInit(Stream dstream, String encoding, int startline,
        int startcolumn, int buffersize)
        {

            ReInit(encoding == null ? new StreamReader(dstream) : new StreamReader(dstream, Encoding.GetEncoding(encoding)/*//TODO: Check behaviour*/), startline, startcolumn, buffersize);
        }

        /** Reinitialise. */
        public void ReInit(Stream dstream, int startline,
        int startcolumn, int buffersize)
        {
            ReInit(new StreamReader(dstream), startline, startcolumn, buffersize);
        }
        /** Reinitialise. */
        public void ReInit(Stream dstream, String encoding, int startline,
                           int startcolumn)
        {
            ReInit(dstream, encoding, startline, startcolumn, 4096);
        }
        /** Reinitialise. */
        public void ReInit(Stream dstream, int startline,
                           int startcolumn)
        {
            ReInit(dstream, startline, startcolumn, 4096);
        }
        /** Reinitialise. */
        public void ReInit(Stream dstream, String encoding)
        {
            ReInit(dstream, encoding, 1, 1, 4096);
        }

        /** Reinitialise. */
        public void ReInit(Stream dstream)
        {
            ReInit(dstream, 1, 1, 4096);
        }

        /** @return token image as String */
        public String GetImage()
        {
            if (Bufpos >= _tokenBegin)
                return new String(Buffer, _tokenBegin, Bufpos - _tokenBegin + 1);
            else
                return new String(Buffer, _tokenBegin, _bufsize - _tokenBegin) +
                                        new String(Buffer, 0, Bufpos + 1);
        }

        /** @return suffix */
        public char[] GetSuffix(int len)
        {
            char[] ret = new char[len];

            if ((Bufpos + 1) >= len)
                Array.Copy(Buffer, Bufpos - len + 1, ret, 0, len);
            else
            {
                Array.Copy(Buffer, _bufsize - (len - Bufpos - 1), ret, 0,
                                                                  len - Bufpos - 1);
                Array.Copy(Buffer, 0, ret, len - Bufpos - 1, Bufpos + 1);
            }

            return ret;
        }

        /** Set buffers back to null when finished. */
        public void Done()
        {
            NextCharBuf = null;
            Buffer = null;
            Bufline = null;
            Bufcolumn = null;
        }

        /// <summary>
        /// Method to adjust line and column numbers for the start of a token.
        /// </summary>
        /// <param name="newLine">The new line.</param>
        /// <param name="newCol">The new col.</param>
        public void AdjustBeginLineColumn(int newLine, int newCol)
        {
            int start = _tokenBegin;
            int len;

            if (Bufpos >= _tokenBegin)
            {
                len = Bufpos - _tokenBegin + InBuf + 1;
            }
            else
            {
                len = _bufsize - _tokenBegin + Bufpos + 1 + InBuf;
            }

            int i = 0, j = 0, k = 0;
            int nextColDiff = 0, columnDiff = 0;

            while (i < len && Bufline[j = start % _bufsize] == Bufline[k = ++start % _bufsize])
            {
                Bufline[j] = newLine;
                nextColDiff = columnDiff + Bufcolumn[k] - Bufcolumn[j];
                Bufcolumn[j] = newCol + columnDiff;
                columnDiff = nextColDiff;
                i++;
            }

            if (i < len)
            {
                Bufline[j] = newLine++;
                Bufcolumn[j] = newCol + columnDiff;

                while (i++ < len)
                {
                    if (Bufline[j = start % _bufsize] != Bufline[++start % _bufsize])
                        Bufline[j] = newLine++;
                    else
                        Bufline[j] = newLine;
                }
            }

            Line = Bufline[j];
            Column = Bufcolumn[j];
        }

    }
}
