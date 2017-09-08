using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

//PATROLLED
namespace Syn.Speech.Alignment
{
    public sealed class CharTokenizer : IEnumerator<Token>
    {
        public const int EOF = -1;
        public const string DEFAULT_WHITESPACE_SYMBOLS = " \t\n\r";
        public const string DEFAULT_SINGLE_CHAR_SYMBOLS = "(){}[]";
        public const string DEFAULT_PREPUNCTUATION_SYMBOLS = "\"'`({[";
        public const string DEFAULT_POSTPUNCTUATION_SYMBOLS = "\"'`.,:;!?(){}[]";
        private int lineNumber;
        private string inputText;
        private StringReader _reader;
        private int currentChar;
        private int currentPosition;

        private string whitespaceSymbols = DEFAULT_WHITESPACE_SYMBOLS;
        private string singleCharSymbols = DEFAULT_SINGLE_CHAR_SYMBOLS;
        private string prepunctuationSymbols = DEFAULT_PREPUNCTUATION_SYMBOLS;
        private string postpunctuationSymbols = DEFAULT_POSTPUNCTUATION_SYMBOLS;

        private string errorDescription;
        private Token token;
        private Token lastToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="CharTokenizer"/> class.
        /// </summary>
        public CharTokenizer() { }

        public CharTokenizer(string _string)
        {
            setInputText(_string);
        }

        public CharTokenizer(StringReader file)
        {
            setInputReader(file);
        }

        public void setWhitespaceSymbols(string symbols)
        {
            whitespaceSymbols = symbols;
        }

        public void setSingleCharSymbols(string symbols)
        {
            singleCharSymbols = symbols;
        }

        public void setPrepunctuationSymbols(string symbols)
        {
            prepunctuationSymbols = symbols;
        }

        public void setPostpunctuationSymbols(string symbols)
        {
            postpunctuationSymbols = symbols;
        }

        public void setInputText(string inputString)
        {
            inputText = inputString;
            currentPosition = 0;
            if (inputText == null)
                return;
            getNextChar();
        }

        public void setInputReader(StringReader reader)
        {
            _reader = reader;
            getNextChar();
        }


        private int getNextChar()
        {
            if (_reader != null)
            {
                try
                {
                    int readVal = _reader.Read();
                    if (readVal == -1)
                    {
                        currentChar = EOF;
                    }
                    else
                    {
                        currentChar = (char)readVal;
                    }
                }
                catch (IOException ioe)
                {
                    currentChar = EOF;
                    errorDescription = ioe.Message;
                }
            }
            else if (inputText != null)
            {
                if (currentPosition < inputText.Length)
                {
                    currentChar = inputText[currentPosition];
                }
                else
                {
                    currentChar = EOF;
                }
            }
            if (currentChar != EOF)
            {
                currentPosition++;
            }
            if (currentChar == '\n')
            {
                lineNumber++;
            }
            return currentChar;
        }

        private string getTokenOfCharClass(string charClass)
        {
            return getTokenByCharClass(charClass, true);
        }

        private string getTokenNotOfCharClass(string endingCharClass)
        {
            return getTokenByCharClass(endingCharClass, false);
        }

        private void removeTokenPostpunctuation()
        {
            if (token == null)
            {
                return;
            }
            var tokenWord = token.getWord();

            int tokenLength = tokenWord.Length;
            int position = tokenLength - 1;

            //while (position > 0 && postpunctuationSymbols.IndexOf((int)tokenWord.charAt(position)) != -1)
            while (position > 0 && postpunctuationSymbols.IndexOf(tokenWord[position]) != -1)
            {
                position--;
            }

            if (tokenLength - 1 != position)
            {
                // Copy postpunctuation from token
                token.setPostpunctuation(tokenWord.Substring(position + 1));

                // truncate token at postpunctuation
                token.setWord(tokenWord.Substring(0, position + 1));
            }
            else
            {
                token.setPostpunctuation("");
            }
        }

        private string getTokenByCharClass(string charClass, bool containThisCharClass)
        {
            var buffer = new StringBuilder();

            // if we want the returned string to contain chars in charClass, then
            // containThisCharClass is TRUE and
            // (charClass.indexOf(currentChar) != 1) == containThisCharClass)
            // returns true; if we want it to stop at characters of charClass,
            // then containThisCharClass is FALSE, and the condition returns
            // false.
            while ((charClass.IndexOf((char)currentChar) != -1) == containThisCharClass
                    && singleCharSymbols.IndexOf((char)currentChar) == -1
                    && currentChar != EOF)
            {
                buffer.Append((char)currentChar);
                getNextChar();
            }
            return buffer.ToString();
        }

        public void remove()
        {
            throw new InvalidOperationException();
        }

        public bool hasErrors()
        {
            return errorDescription != null;
        }

        public string getErrorDescription()
        {
            return errorDescription;
        }

        public bool isSentenceSeparator()
        {
            string tokenWhiteSpace = token.getWhitespace();
            string lastTokenPostpunctuation = null;
            if (lastToken != null)
            {
                lastTokenPostpunctuation = lastToken.getPostpunctuation();
            }

            if (lastToken == null || token == null)
            {
                return false;
            }
            else if (tokenWhiteSpace.IndexOf('\n') != tokenWhiteSpace
                  .LastIndexOf('\n'))
            {
                return true;
            }
            else if (lastTokenPostpunctuation.IndexOf(':') != -1
                  || lastTokenPostpunctuation.IndexOf('?') != -1
                  || lastTokenPostpunctuation.IndexOf('!') != -1)
            {
                return true;
            }
            else if (lastTokenPostpunctuation.IndexOf('.') != -1
                  && tokenWhiteSpace.Length > 1
                  && char.IsUpper(token.getWord()[0]))
            {
                return true;
            }
            else
            {
                string lastWord = lastToken.getWord();
                int lastWordLength = lastWord.Length;

                if (lastTokenPostpunctuation.IndexOf('.') != -1
                        &&
                    /* next word starts with a capital */
                        char.IsUpper(token.getWord()[0])
                        &&
                    /* last word isn't an abbreviation */
                        !(char.IsUpper(lastWord
                                [lastWordLength - 1]) || (lastWordLength < 4 && char
                                .IsUpper(lastWord[0]))))
                {
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool MoveNext()
        {
            return currentChar != EOF;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public Token Current
        {
            get
            {
                lastToken = token;
                token = new Token();
                token.setWhitespace(getTokenOfCharClass(whitespaceSymbols));
                token.setPrepunctuation(getTokenOfCharClass(prepunctuationSymbols));
                if (singleCharSymbols.IndexOf((char)currentChar) != -1)
                {
                    token.setWord(((char)currentChar).ToString(CultureInfo.InvariantCulture));
                    getNextChar();
                }
                else
                {
                    token.setWord(getTokenNotOfCharClass(whitespaceSymbols));

                }

                token.setPosition(currentPosition);
                token.setLineNumber(lineNumber);
                removeTokenPostpunctuation();
                return token;
            }
        }

        object IEnumerator.Current
        {
            get { return Current; }
        }
    }
}
