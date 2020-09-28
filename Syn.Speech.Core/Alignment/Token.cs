using System.Text;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    /// <summary>
    /// Contains a parsed token from a Tokenizer.
    /// </summary>
    public class Token 
    {
        /// <summary>
        /// Gets or sets the whitespace characters of this Token.
        /// </summary>
        /// <value>the whitespace characters of this Token; null if this Token does not use whitespace characters</value>
        public virtual string Whitespace { get; set; }

        /// <summary>
        /// Returns the prepunctuation characters of this Token.
        /// </summary>
        /// <value>the postpunctuation characters of this Token; null if this Token does not use postpunctuation characters</value>
        public virtual string PrePunctuation { get;  set; }

        /// <summary>
        /// Returns the postpunctuation characters of this Token.
        /// </summary>
        /// <value>he postpunctuation characters of this Token; null if this Token oes not use postpunctuation characters</value>
        public virtual string PostPunctuation { get; set; }

        /// <summary>
        /// Returns the position of this token in the original input text.
        /// </summary>
        /// <value>The position of this token in the original input text.</value>
        public virtual int Position { get;  set; }

        /// <summary>
        /// Returns the line of this token in the original text.
        /// </summary>
        /// <value>The line of this token in the original text.</value>
        public virtual int LineNumber { get;  set; }

        /// <summary>
        /// Returns the string associated with this token.
        /// </summary>
        /// <value>The token if it exists; otherwise null</value>
        public virtual string Word { get; set; }

        /// <summary>
        /// Converts this token to a string.
        /// </summary>
        /// <returns>The string representation of this object.</returns>
        public override string ToString()
        {
            var fullToken = new StringBuilder();

            if (Whitespace != null)
            {
                fullToken.Append(Whitespace);
            }
            if (PrePunctuation != null)
            {
                fullToken.Append(PrePunctuation);
            }
            if (Word != null)
            {
                fullToken.Append(Word);
            }
            if (PostPunctuation != null)
            {
                fullToken.Append(PostPunctuation);
            }
            return fullToken.ToString();
        }
    }
}
