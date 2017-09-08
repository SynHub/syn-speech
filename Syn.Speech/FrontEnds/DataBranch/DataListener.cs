namespace Syn.Speech.FrontEnds.DataBranch
{
    /** Defines some API-elements for Data-observer classes. */
    public interface IDataListener
    {

        /** This method is invoked when a new {@link Data} object becomes available.
         * @param data*/
         void ProcessDataFrame(IData data);

    }
}
