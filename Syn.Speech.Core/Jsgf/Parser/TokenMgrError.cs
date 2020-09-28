using System;
using System.Globalization;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Parser
{
    /// <summary>
    /// Token Manager Error.
    /// </summary>
    public class TokenMgrError : Error
    {

        /*
         * Ordinals for various reasons why an Error of this type can be thrown.
         */

        /// <summary>
        /// Lexical error occurred.
        /// </summary>
        internal const int LEXICAL_ERROR = 0;

        /// <summary>
        /// An attempt was made to create a second instance of a static token manager.
        /// </summary>
        const int StaticLexerError = 1;

        /// <summary>
        ///  Tried to change to an invalid lexical state.
        /// </summary>
        internal const int InvalidLexicalState = 2;

        /// <summary>
        /// Detected (and bailed out of) an infinite loop in the token manager.
        /// </summary>
        const int LoopDetected = 3;

        /// <summary>
        /// Indicates the reason why the exception is thrown. It will have one of the above 4 values.
        /// </summary>
        int errorCode;

        /// <summary>
        /// Replaces unprintable characters by their escaped (or unicode escaped) equivalents in the given string
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        protected static String AddEscapes(String str)
        {
            StringBuilder retval = new StringBuilder();
            char ch;

            char zeroChar = Java.ToChar(0);
            for (int i = 0; i < str.Length; i++)
            {
                //Just because C# only takes constant values in switch statements - the following code has been adopted
                var item = str[i];
                if (item == zeroChar)
                {
                    continue;
                }
                if (item == '\b')
                {
                    retval.Append("\\b");
                    continue;
                }
                if  (item == '\t')
                {
                    retval.Append("\\t");
                    continue;
                }
                if  (item == '\n')
                {
                    retval.Append("\\n");
                    continue;
                }
                if  (item == '\f')
                {
                    retval.Append("\\f");
                    continue;
                }
                if  (item == '\r')
                {
                    retval.Append("\\r");
                    continue;
                }
                if  (item == '\"')
                {
                    retval.Append("\\\"");
                    continue;
                }
                if  (item == '\'')
                {
                    retval.Append("\\\'");
                    continue; 
                }
                if (item == '\\')
                {
                    retval.Append("\\\\");
                    continue;
                }
                if ((ch = str[i]) < 0x20 || ch > 0x7e)
                {
                    String s = "0000" + Integer.ToString(ch, 16);
                    retval.Append("\\u" + s.Substring(s.Length - 4, s.Length));
                }
                else
                {
                    retval.Append(ch);
                }
            }
            return retval.ToString();
        }

        /**
         * Returns a detailed message for the Error when it is thrown by the
         * token manager to indicate a lexical error.
         * Parameters :
         *    EOFSeen     : indicates if EOF caused the lexical error
         *    curLexState : lexical state in which this error occurred
         *    errorLine   : line number when the error occurred
         *    errorColumn : column number when the error occurred
         *    errorAfter  : prefix that was seen before this error occurred
         *    curchar     : the offending character
         * Note: You can customize the lexical error message by modifying this method.
         */
        protected static String LexicalError(bool eofSeen, int lexState, int errorLine, int errorColumn, String errorAfter, char curChar)
        {
            return ("Lexical error at line " +
                  errorLine + ", column " +
                  errorColumn + ".  Encountered: " +
                  (eofSeen ? "<EOF> " : ("\"" + AddEscapes(curChar.ToString(CultureInfo.InvariantCulture)) + "\"") + " (" + (int)curChar + "), ") +
                  "after : \"" + AddEscapes(errorAfter) + "\"");
        }

        /**
         * You can also modify the body of this method to customize your error messages.
         * For example, cases like LOOP_DETECTED and INVALID_LEXICAL_STATE are not
         * of end-users concern, so you can return something like :
         *
         *     "Internal Error : Please file a bug report .... "
         *
         * from this method for such cases in the release version of your parser.
         */
        public String GetMessage()
        {
            return base.Message;
        }

        /*
         * Constructors of various flavors follow.
         */

        /** No arg constructor. */
        public TokenMgrError()
        {
        }

        /** Constructor with message and reason. */
        public TokenMgrError(String message, int reason)
            : base(message)
        {
            errorCode = reason;
        }

        /** Full Constructor. */
        public TokenMgrError(bool eofSeen, int lexState, int errorLine, int errorColumn, String errorAfter, char curChar, int reason)
            : this(LexicalError(eofSeen, lexState, errorLine, errorColumn, errorAfter, curChar), reason)
        {

        }
    }
}
