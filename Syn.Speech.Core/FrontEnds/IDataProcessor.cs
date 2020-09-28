using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// /**
    /// A processor that performs a signal processing function.
    ///
    /// Since a DataProcessor usually belongs to a particular front end pipeline,
    /// you can name the pipeline it belongs to in the {@link #initialize()
    /// initialize} method. (Note, however, that it is not always the case that a
    /// DataProcessor belongs to a particular pipeline. For example, the {@link
    /// sphincs.frontend.util.Microphone Microphone}class is a DataProcessor,
    /// but it usually does not belong to any particular pipeline.  <p/> Each
    /// DataProcessor usually have a predecessor as well. This is the previous
    /// DataProcessor in the pipeline. Again, not all DataProcessors have
    /// predecessors.  <p/> Calling {@link #getData() getData}will return the
    /// processed Data object.
    ///
    /// </summary>
    public interface IDataProcessor: IConfigurable
    {

        /// <summary>
        /// Initializes this DataProcessor.
        /// This is typically called after the DataProcessor has been configured.
        /// </summary>
         void Initialize();

         /// <summary>
         /// Returns the processed Data output.
         /// </summary>
         /// <returns>a Data object that has been processed by this DataProcessor</returns>
         IData GetData();


        /// <summary>
        /// Returns the predecessor DataProcessor.
        /// </summary>
        /// <value>the predecessor</value>
        IDataProcessor Predecessor { get; set; }
    }
}
