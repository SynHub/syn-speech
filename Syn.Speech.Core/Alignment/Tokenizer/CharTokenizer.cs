using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public sealed class CharTokenizer : IEnumerator<Token>
    {
        public const int Eof = -1;
        public const string DefaultWhitespaceSymbols = " \t\n\r";
        public const string DefaultSingleCharSymbols = "(){}[]";
        public const string DefaultPrepunctuationSymbols = "\"'`({[";
        public const string DefaultPostpunctuationSymbols = "\"'`.,:;!?(){}[]";
        private int _lineNumber;
        private string _inputText;
        private StringReader _reader;
        private int _currentChar;
        private int _currentPosition;

        private Token _token;
        private Token _lastToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharTokenizer"/> class.
        /// </summary>
        public CharTokenizer()
        {
            PostpunctuationSymbols = DefaultPostpunctuationSymbols;
            PrepunctuationSymbols = DefaultPrepunctuationSymbols;
            SingleCharSymbols = DefaultSingleCharSymbols;
            WhitespaceSymbols = DefaultWhitespaceSymbols;
        }

        public CharTokenizer(string _string)
        {
            PostpunctuationSymbols = DefaultPostpunctuationSymbols;
            PrepunctuationSymbols = DefaultPrepunctuationSymbols;
            SingleCharSymbols = DefaultSingleCharSymbols;
            WhitespaceSymbols = DefaultWhitespaceSymbols;
            SetInputText(_string);
        }

        public CharTokenizer(StringReader file)
        {
            PostpunctuationSymbols = DefaultPostpunctuationSymbols;
            PrepunctuationSymbols = DefaultPrepunctuationSymbols;
            SingleCharSymbols = DefaultSingleCharSymbols;
            WhitespaceSymbols = DefaultWhitespaceSymbols;
            SetInputReader(file);
        }

        public string WhitespaceSymbols { private get; set; }

        public string SingleCharSymbols { private get; set; }

        public string PrepunctuationSymbols { private get; set; }

        public string PostpunctuationSymbols { private get; set; }

        public void SetInputText(string inputString)
        {
            _inputText = inputString;
            _currentPosition = 0;
            if (_inputText == null)
                return;
            GetNextChar();
        }

        public void SetInputReader(StringReader reader)
        {
            _reader = reader;
            GetNextChar();
        }


        private int GetNextChar()
        {
            if (_reader != null)
            {
                try
                {
                    var readVal = _reader.Read();
                    if (readVal == -1)
                    {
                        _currentChar = Eof;
                    }
                    else
                    {
                        _currentChar = (char)readVal;
                    }
                }
                catch (IOException ioe)
                {
                    _currentChar = Eof;
                    ErrorDescription = ioe.Message;
                }
            }
            else if (_inputText != null)
            {
                if (_currentPosition < _inputText.Length)
                {
                    _currentChar = _inputText[_currentPosition];
                }
                else
                {
                    _currentChar = Eof;
                }
            }
            if (_currentChar != Eof)
            {
                _currentPosition++;
            }
            if (_currentChar == '\n')
            {
                _lineNumber++;
            }
            return _currentChar;
        }

        private string GetTokenOfCharClass(string charClass)
        {
            return GetTokenByCharClass(charClass, true);
        }

        private string GetTokenNotOfCharClass(string endingCharClass)
        {
            return GetTokenByCharClass(endingCharClass, false);
        }

        private void RemoveTokenPostpunctuation()
        {
            if (_token == null)
            {
                return;
            }
            var tokenWord = _token.Word;

            var tokenLength = tokenWord.Length;
            var position = tokenLength - 1;

            //while (position > 0 && postpunctuationSymbols.IndexOf((int)tokenWord.charAt(position)) != -1)
            while (position > 0 && PostpunctuationSymbols.IndexOf(tokenWord[position]) != -1)
            {
                position--;
            }

            if (tokenLength - 1 != position)
            {
                // Copy postpunctuation from token
                _token.PostPunctuation = tokenWord.Substring(position + 1);

                // truncate token at postpunctuation
                _token.Word = tokenWord.Substring(0, position + 1);
            }
            else
            {
                _token.PostPunctuation = "";
            }
        }

        private string GetTokenByCharClass(string charClass, bool containThisCharClass)
        {
            var buffer = new StringBuilder();

            // if we want the returned string to contain chars in charClass, then
            // containThisCharClass is TRUE and
            // (charClass.indexOf(currentChar) != 1) == containThisCharClass)
            // returns true; if we want it to stop at characters of charClass,
            // then containThisCharClass is FALSE, and the condition returns
            // false.
            while ((charClass.IndexOf((char)_currentChar) != -1) == containThisCharClass
                    && SingleCharSymbols.IndexOf((char)_currentChar) == -1
                    && _currentChar != Eof)
            {
                buffer.Append((char)_currentChar);
                GetNextChar();
            }
            return buffer.ToString();
        }

        public void Remove()
        {
            throw new InvalidOperationException();
        }

        public bool HasErrors
        {
            get { return ErrorDescription != null; }
        }

        public string ErrorDescription { get; private set; }

        public bool IsSentenceSeparator()
        {
            var tokenWhiteSpace = _token.Whitespace;
            string lastTokenPostpunctuation = null;
            if (_lastToken != null)
            {
                lastTokenPostpunctuation = _lastToken.PostPunctuation;
            }

            if (_lastToken == null || _token == null)
            {
                return false;
            }
            if (tokenWhiteSpace.IndexOf('\n') != tokenWhiteSpace
                .LastIndexOf('\n'))
            {
                return true;
            }
            if (lastTokenPostpunctuation != null && (lastTokenPostpunctuation.IndexOf(':') != -1 || lastTokenPostpunctuation.IndexOf('?') != -1 || lastTokenPostpunctuation.IndexOf('!') != -1))
            {
                return true;
            }
            if (lastTokenPostpunctuation != null && (lastTokenPostpunctuation.IndexOf('.') != -1 && tokenWhiteSpace.Length > 1 && char.IsUpper(_token.Word[0])))
            {
                return true;
            }

            var lastWord = _lastToken.Word;
            var lastWordLength = lastWord.Length;

            if (lastTokenPostpunctuation.IndexOf('.') != -1 &&
                /* next word starts with a capital */
                char.IsUpper(_token.Word[0])
                &&
                /* last word isn't an abbreviation */
                !(char.IsUpper(lastWord
                    [lastWordLength - 1]) || (lastWordLength < 4 && char
                        .IsUpper(lastWord[0]))))
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            return _currentChar != Eof;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public Token Current
        {
            get
            {
                _lastToken = _token;
                _token = new Token();
                _token.Whitespace = GetTokenOfCharClass(WhitespaceSymbols);
                _token.PrePunctuation = GetTokenOfCharClass(PrepunctuationSymbols);
                if (SingleCharSymbols.IndexOf((char)_currentChar) != -1)
                {
                    _token.Word = ((char)_currentChar).ToString(CultureInfo.InvariantCulture);
                    GetNextChar();
                }
                else
                {
                    _token.Word = GetTokenNotOfCharClass(WhitespaceSymbols);

                }

                _token.Position = _currentPosition;
                _token.LineNumber = _lineNumber;
                RemoveTokenPostpunctuation();
                return _token;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
