using System;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Parser
{

    /**
     * This exception is thrown when parse errors are encountered.
     * You can explicitly create objects of this exception type by
     * calling the method generateParseException in the generated
     * parser.
     *
     * You can modify this class to customize your error reporting
     * mechanisms so long as you retain the public fields.
     */

    public class ParseException : Exception
    {

        /**
         * The version identifier for this Serializable class.
         * Increment only if the <i>serialized</i> form of the
         * class changes.
         */

        /**
         * This constructor is used by the method "generateParseException"
         * in the generated parser.  Calling this constructor generates
         * a new object of this type with the fields "currentToken",
         * "expectedTokenSequences", and "tokenImage" set.
         */
        public ParseException(Token currentTokenVal,
                              int[][] expectedTokenSequencesVal,
                              String[] tokenImageVal
                             )
            : base(Initialise(currentTokenVal, expectedTokenSequencesVal, tokenImageVal))
        {
            CurrentToken = currentTokenVal;
            ExpectedTokenSequences = expectedTokenSequencesVal;
            TokenImage = tokenImageVal;
        }

        /**
         * The following constructors are for use by you for whatever
         * purpose you can think of.  Constructing the exception in this
         * manner makes the exception behave in the normal way - i.e., as
         * documented in the class "Throwable".  The fields "errorToken",
         * "expectedTokenSequences", and "tokenImage" do not contain
         * relevant information.  The JavaCC generated code does not use
         * these constructors.
         */

        public ParseException(): base()
        {

        }

        /** Constructor with message. */
        public ParseException(String message)
            : base(message)
        {

        }


        /**
         * This is the last token that has been consumed successfully.  If
         * this object has been created due to a parse error, the token
         * followng this token will (therefore) be the first error token.
         */
        public Token CurrentToken;

        /**
         * Each entry in this array is an array of integers.  Each array
         * of integers represents a sequence of tokens (by their ordinal
         * values) that is expected at this point of the parse.
         */
        public int[][] ExpectedTokenSequences;

        /**
         * This is a reference to the "tokenImage" array of the generated
         * parser within which the parse error occurred.  This array is
         * defined in the generated ...Constants interface.
         */
        public String[] TokenImage;

        /**
         * It uses "currentToken" and "expectedTokenSequences" to generate a parse
         * error message and returns it.  If this object has been created
         * due to a parse error, and you do not catch it (it gets thrown
         * from the parser) the correct error message
         * gets displayed.
         */
        private static String Initialise(Token currentToken,
                                 int[][] expectedTokenSequences,
                                 String[] tokenImage)
        {
            String eol = Environment.NewLine; //System.getProperty("line.separator", "\n");
            StringBuilder expected = new StringBuilder();
            int maxSize = 0;
            for (int i = 0; i < expectedTokenSequences.Length; i++)
            {
                if (maxSize < expectedTokenSequences[i].Length)
                {
                    maxSize = expectedTokenSequences[i].Length;
                }
                for (int j = 0; j < expectedTokenSequences[i].Length; j++)
                {
                    expected.Append(tokenImage[expectedTokenSequences[i][j]]).Append(' ');
                }
                if (expectedTokenSequences[i][expectedTokenSequences[i].Length - 1] != 0)
                {
                    expected.Append("...");
                }
                expected.Append(eol).Append("    ");
            }
            String retval = "Encountered \"";
            Token tok = currentToken.Next;
            for (int i = 0; i < maxSize; i++)
            {
                if (i != 0) retval += " ";
                if (tok.Kind == 0)
                {
                    retval += tokenImage[0];
                    break;
                }
                retval += " " + tokenImage[tok.Kind];
                retval += " \"";
                retval += Add_Escapes(tok.Image);
                retval += " \"";
                tok = tok.Next;
            }
            retval += "\" at line " + currentToken.Next.BeginLine + ", column " + currentToken.Next.BeginColumn;
            retval += "." + eol;
            if (expectedTokenSequences.Length == 1)
            {
                retval += "Was expecting:" + eol + "    ";
            }
            else
            {
                retval += "Was expecting one of:" + eol + "    ";
            }
            retval += expected.ToString();
            return retval;
        }

        /**
         * The end of line string for this machine.
         */
        protected String Eol = Environment.NewLine; //System.getProperty("line.separator", "\n");

        /**
         * Used to convert raw characters to their escaped version
         * when these raw version cannot be used as part of an ASCII
         * string literal.
         */
        static String Add_Escapes(String str)
        {
            StringBuilder retval = new StringBuilder();
            char ch;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == Java.ToChar(0))
                {
                    continue;
                }
                if (str[i] == '\b')
                {
                    retval.Append("\\b");
                    continue;
                }
                if (str[i] == '\t')
                {
                    retval.Append("\\t");
                    continue;
                }
                if (str[i] == '\n')
                {
                    retval.Append("\\n");
                    continue;
                }
                if (str[i] == '\f')
                {
                    retval.Append("\\f");
                    continue;
                }
                if (str[i] == '\r')
                {
                    retval.Append("\\r");
                    continue;
                }
                if (str[i] == '\"')
                {
                    retval.Append("\\\"");
                    continue;
                }
                if (str[i] == '\'')
                {
                    retval.Append("\\\'");
                    continue;
                }
                if (str[i] == '\\')
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

    }
}
