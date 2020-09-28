//REFACTORED
namespace Syn.Speech.Linguist.Flat
{
    public partial class FlatLinguist
    {

        /// <summary>
        /// The search graph that is produced by the flat linguist.
        /// </summary>
        protected class FlatSearchGraph : ISearchGraph
        {

            /**
            /// An array of classes that represents the order in which the states will be returned.
             */


            /**
            /// Constructs a flast search graph with the given initial state
             *
            /// @param initialState the initial state
             */
            public FlatSearchGraph(ISearchState initialState)
            {
                InitialState = initialState;
            }

            public bool WordTokenFirst
            {
                get { return true; }
            }

            /*
           /// (non-Javadoc)
            *
           /// @see edu.cmu.sphinx.linguist.SearchGraph#getInitialState()
            */

            public ISearchState InitialState { get; private set; }


            /*
           /// (non-Javadoc)
            *
           /// @see edu.cmu.sphinx.linguist.SearchGraph#getNumStateOrder()
            */

            public int NumStateOrder
            {
                get { return 7; }
            }
        }
    }
}
