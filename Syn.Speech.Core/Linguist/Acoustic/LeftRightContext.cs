using System;
using System.Text;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents  the context for a unit 
    /// </summary>
    public class LeftRightContext : Context
    {
        
        string stringRepresentation;

        /**
        /// Creates a LeftRightContext
         *
        /// @param leftContext  the left context or null if no left context
        /// @param rightContext the right context or null if no right context
         */
        private LeftRightContext(Unit[] leftContext, Unit[] rightContext) {
            LeftContext = leftContext;
            RightContext = rightContext;
        }

        /** Provides a string representation of a context */

        public override string ToString() 
        {
            return GetContextName(LeftContext) + ',' + GetContextName(RightContext);
        }

        /**
        /// Factory method for creating a left/right context
         *
        /// @param leftContext  the left context or null if no left context
        /// @param rightContext the right context or null if no right context
        /// @return a left right context
         */
        public static LeftRightContext Get(Unit[] leftContext, Unit[] rightContext) 
        { 
            return new LeftRightContext(leftContext, rightContext);
        }

        /**
        /// Retrieves the left context for this unit
         *
        /// @return the left context
         */

        public Unit[] LeftContext { get; private set; }

        /**
        /// Retrieves the right context for this unit
         *
        /// @return the right context
         */

        public Unit[] RightContext { get; private set; }

        /**
        /// Gets the context name for a particular array of units
         *
        /// @param context the context
        /// @return the context name
         */
        public static string GetContextName(Unit[] context) 
        {
            if (context == null)
                return "*";
            if (context.Length == 0)
                return "(empty)";
            var sb = new StringBuilder();
            foreach (var unit in context) 
            {
                sb.Append(unit == null ? null : unit.Name).Append('.');
            }
            sb.Length = sb.Length - 1; // remove last period
            return sb.ToString();
        }

        /**
        /// Checks to see if there is a partial match with the given context. If both contexts are LeftRightContexts then  a
        /// left or right context that is null is considered a wild card and matches anything, othewise the contexts must
        /// match exactly. Anything matches the Context.EMPTY_CONTEXT
         *
        /// @param context the context to check
        /// @return true if there is a partial match
         */
        public override Boolean IsPartialMatch(Context context) 
        {
            if (context is LeftRightContext) 
            {
                var lrContext = (LeftRightContext)context;
                var lc = lrContext.LeftContext;
                var rc = lrContext.RightContext;

                return (lc == null || LeftContext == null || Unit.IsContextMatch(lc, LeftContext))
                    && (rc == null || RightContext == null || Unit.IsContextMatch(rc, RightContext));
            }
            return context == EmptyContext && LeftContext == null && RightContext == null;
        }
    }
}
