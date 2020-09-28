using System;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    public class DataProcessingException : RuntimeException
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingException"/> class.
        /// </summary>
        public DataProcessingException()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DataProcessingException(String message)
            : base(message)
        {

        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="cause">The inner exception.</param>
        public DataProcessingException(string message, Exception cause)
            : base(message, cause)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProcessingException"/> class.
        /// </summary>
        /// <param name="cause">The inner exception.</param>
        public DataProcessingException(Exception cause)
            : base(cause)
        {

        }
    }
}
