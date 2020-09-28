using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    public class JSGFGrammarParseException : Exception
    {
        public int LineNumber;
        public int CharNumber;
        public new String Message;
        public String Details;

        public JSGFGrammarParseException(int lineNumber, int charNumber, String message, String details)
        {
            this.LineNumber = lineNumber;
            this.CharNumber = charNumber;
            this.Message = message;
            this.Details = details;
        }

        public JSGFGrammarParseException(String message)
        {
            this.Message = message;
        }
    }
}
