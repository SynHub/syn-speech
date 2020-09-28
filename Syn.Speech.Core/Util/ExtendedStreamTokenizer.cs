using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
//REFACTORED
using Syn.Speech.Helper;

namespace Syn.Speech.Util
{
    /// <summary>
    /// A class that provides a mechanism for tokenizing a stream
    /// </summary>
    public class ExtendedStreamTokenizer
    {
        private readonly string _path;
        private readonly StreamTokenizer _tokenizer;
        private readonly StreamReader _reader;
        private Boolean _atEof;
        private readonly List<String> _putbackList;


        /**
        /// Creates and returns a stream tokenizer that has been properly configured to parse sphinx3 data This
        /// ExtendedStreamTokenizer has no comment characters.
         *
        /// @param path the source of the data
        /// @throws FileNotFoundException if a file cannot be found
         */
        public ExtendedStreamTokenizer(String path)
            :this(path, false)
        {
            
        }


        /**
        /// Creates and returns a stream tokenizer that has been properly configured to parse sphinx3 data This
        /// ExtendedStreamTokenizer has no comment characters.
         *
        /// @param path             the source of the data
        /// @param eolIsSignificant if true eol is significant
        /// @throws FileNotFoundException if a file cannot be found
         */
        public ExtendedStreamTokenizer(String path, Boolean eolIsSignificant)
            :this(new StreamReader(path), eolIsSignificant)
        {
            _path = path;
        }

        public ExtendedStreamTokenizer(Stream inputStream, Boolean eolIsSignificant)
            : this(new StreamReader(inputStream), eolIsSignificant)
        {

        }

    

        /**
        /// Constructs an ExtendedStreamTokenizer from the given InputStream
         *
        /// @param inputStream      the source of the data
        /// @param commentChar      the comment character
        /// @param eolIsSignificant true if EOL is significant, false otherwise
         */
        public ExtendedStreamTokenizer(StreamReader inputStream, int commentChar,Boolean eolIsSignificant)
            : this(inputStream, eolIsSignificant)
        {
            CommentChar(commentChar);
        }

        public ExtendedStreamTokenizer(Stream inputStream, int commentChar, Boolean eolIsSignificant)
            : this(new StreamReader(inputStream), eolIsSignificant)
        {
            CommentChar(commentChar);
        }

        /**
        /// Constructs an ExtendedStreamTokenizer from the given Reader. This ExtendedStreamTokenizer has no comment
        /// characters.
         *
        /// @param reader           the source of the data
        /// @param eolIsSignificant true if eol is significant
         */
        public ExtendedStreamTokenizer(StreamReader reader, Boolean eolIsSignificant) 
        {
            _reader = reader;
            if (reader != null)
            {
                _tokenizer = new StreamTokenizer(reader);
                _tokenizer.ResetSyntax();
                _tokenizer.WhitespaceChars(0, 32);
                _tokenizer.WordChars(33, 255);
                _tokenizer.EolIsSignificant(eolIsSignificant);
            }
            else
            {
                _atEof = true;
            }
            _putbackList = new List<String>();
        }


        /**
        /// Closes the tokenizer
         *
        /// @throws IOException if an error occurs while closing the stream
         */
        public void Close()
        {
            _reader.Close();
        }


        /**
        /// Specifies that all the characters between low and hi incluseive are whitespace characters
         *
        /// @param low the low end of the range
        /// @param hi  the high end of the range
         */
        public void WhitespaceChars(int low, int hi) 
        {
            _tokenizer.WhitespaceChars(low, hi);
        }


        /**
        /// Specified that the character argument starts a single-line comment. All characters from the comment character to
        /// the end of the line are ignored by this stream tokenizer.
         *
        /// @param ch the comment character
         */
        public void CommentChar(int ch) 
        {
            _tokenizer.CommentChar(ch);
        }


        /// <summary>
        /// Gets the next word from the tokenizer
        /// @throws StreamCorruptedException if the word does not match
        /// @throws IOException              if an error occurs while loading the data
        /// </summary>
        /// <returns>the next word</returns>
        public string GetString()
        {
            if (_tokenizer == null)
                return null;
            if (_putbackList.Count!=0) 
            {
                var retVal = _putbackList[_putbackList.Count - 1];
                _putbackList.RemoveAt(_putbackList.Count - 1);
                return retVal;
            }
            _tokenizer.NextToken();
            if (_tokenizer.Ttype == StreamTokenizer.TtEOF) 
            {
                _atEof = true;
            }
            if (_tokenizer.Ttype != StreamTokenizer.TtWord && _tokenizer.Ttype != StreamTokenizer.TtEol &&
                _tokenizer.Ttype != StreamTokenizer.TtEOF) 
            {
                Corrupt("word expected but not found");
            }
            if (_tokenizer.Ttype == StreamTokenizer.TtEol || _tokenizer.Ttype == StreamTokenizer.TtEOF) 
            {
                return null;
            } 
            return _tokenizer.StringValue;
        }

        /**
        /// Puts a string back, the next get will return this string
         *
        /// @param string the string to unget
         */
        public void Unget(String _string) 
        {
            _putbackList.Add(_string);
        }


        /**
        /// Determines if the stream is at the end of file
         *
        /// @return true if the stream is at EOF
         */
        public Boolean IsEOF() 
        {
            return _atEof;
        }


        /**
        /// Throws an error with the line and path added
         *
        /// @param msg the annotation message
         */
        private void Corrupt(String msg) 
        {
            throw new Exception(msg + " at line " + _tokenizer.LineNumber + " in file " + _path);
        }


        /**
        /// Gets the current line number
         *
        /// @return the line number
         */
        public int GetLineNumber() 
        {
            return _tokenizer.LineNumber;
        }


        /// <summary>
        /// Loads a word from the tokenizer and ensures that it matches 'expecting'
        /// @throws StreamCorruptedException if the word does not match
        /// @throws IOException              if an error occurs while loading the data
        /// </summary>
        /// <param name="expecting">expecting the word read must match this</param>
        public void ExpectString(String expecting)
        {
            var line = GetString();
            if (!line.Equals(expecting)) 
            {
                Corrupt("error matching expected string '" + expecting +
                        "' in line: '" + line + '\'');
            }
        }

        /**
        /// Loads an integer  from the tokenizer and ensures that it matches 'expecting'
         *
        /// @param name      the name of the value
        /// @param expecting the word read must match this
        /// @throws StreamCorruptedException if the word does not match
        /// @throws IOException              if an error occurs while loading the data
         */
        public void ExpectInt(String name, int expecting)
        {
            var val = GetInt(name);
            if (val != expecting) 
            {
                Corrupt("Expecting integer " + expecting);
            }
        }


        /**
        /// gets an integer from the tokenizer stream
         *
        /// @param name the name of the parameter (for error reporting)
        /// @return the next word in the stream as an integer
        /// @throws StreamCorruptedException if the next value is not a
        /// @throws IOException              if an error occurs while loading the data number
         */
        public int GetInt(String name)
        {
            var iVal = 0;
            try {
                var val = GetString();
                Int32.TryParse(val,out iVal);
            } 
            catch (FormatException) 
            {
                Corrupt("while parsing int " + name);
            }
            return iVal;
        }


        /**
        /// gets a double from the tokenizer stream
         *
        /// @param name the name of the parameter (for error reporting)
        /// @return the next word in the stream as a double
        /// @throws StreamCorruptedException if the next value is not a
        /// @throws IOException              if an error occurs while loading the data number
         */
        public double GetDouble(String name)
        {
            var dVal = 0.0;
            try {
                var val = GetString();
                if (val.Equals("inf")) 
                {
                    dVal = Double.PositiveInfinity;
                } 
                else 
                {
                    dVal = Double.Parse(val, CultureInfo.InvariantCulture.NumberFormat);
                }
            } 
            catch (FormatException) 
            {
                Corrupt("while parsing double " + name);
            }
            return dVal;
        }


        /**
        /// gets a float from the tokenizer stream
         *
        /// @param name the name of the parameter (for error reporting)
        /// @return the next word in the stream as a float
        /// @throws StreamCorruptedException if the next value is not a
        /// @throws IOException              if an error occurs while loading the data number
         */
        public float GetFloat(String name)
        {
            var fVal = 0.0F;
            try {
                var val = GetString();
                if (val.Equals("inf")) 
                {
                    fVal = float.PositiveInfinity;
                } 
                else 
                {
                    fVal = float.Parse(val, CultureInfo.InvariantCulture.NumberFormat);
                }
            } 
            catch (FormatException) 
            {
                Corrupt("while parsing float " + name);
            }
            return fVal;
        }


        /**
        /// gets a optional float from the tokenizer stream. If a float is not present, the default is returned
         *
        /// @param name         the name of the parameter (for error reporting)
        /// @param defaultValue the default value
        /// @return the next word in the stream as a float
        /// @throws StreamCorruptedException if the next value is not a
        /// @throws IOException              if an error occurs while loading the data number
         */
        public float GetFloat(String name, float defaultValue)
        {
            var fVal = 0.0F;
            try {
                var val = GetString();
                if (val == null) 
                {
                    fVal = defaultValue;
                } 
                else if (val.Equals("inf")) 
                {
                    fVal = float.PositiveInfinity;
                } 
                else 
                {
                    fVal = float.Parse(val, CultureInfo.InvariantCulture.NumberFormat);
                }
            } 
            catch (FormatException) 
            {
                Corrupt("while parsing float " + name);
            }
            return fVal;
        }


        /**
        /// Skip any carriage returns.
         *
        /// @throws IOException if an error occurs while reading data from the stream.
         */
        public void Skipwhite()
        {
            string next;
            while (!IsEOF()) {
                if ((next = GetString()) != null) 
                {
                    Unget(next);
                    break;
                }
            }
        }

    }
}
