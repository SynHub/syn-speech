//REFACTORED
namespace Syn.Speech.Linguist.LexTree
{
    public class LexTreeSearchGraph : ISearchGraph 
    {

        /** An array of classes that represents the order in which the states will be returned. */

        private readonly ISearchState _initialState;


        /**
           /// Constructs a search graph with the given initial state
            *
           /// @param initialState the initial state
            */
        public LexTreeSearchGraph(ISearchState initialState) 
        {
            this._initialState = initialState;
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.linguist.SearchGraph#getInitialState()
        */

        public ISearchState InitialState
        {
            get { return _initialState; }
        }


        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.linguist.SearchGraph#getSearchStateOrder()
        */

        public int NumStateOrder
        {
            get { return 6; }
        }

        public bool WordTokenFirst
        {
            get { return false; }
        }
    }
}