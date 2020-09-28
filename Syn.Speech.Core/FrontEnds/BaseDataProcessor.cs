using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    /// An abstract DataProcessor implementing elements common to all concrete DataProcessors, such as name, predecessor, and
    /// timer.
    /// </summary>
    public abstract class BaseDataProcessor : ConfigurableAdapter, IDataProcessor
    {
        //private Timer timer;

        public BaseDataProcessor() 
        {

        }

        /// <summary>
        /// Returns the processed Data output.
        /// </summary>
        /// <returns>an Data object that has been processed by this DataProcessor</returns>
        public abstract IData GetData();


        /// <summary>
        /// Initializes this DataProcessor. This is typically called after the DataProcessor has been configured. 
        /// </summary>
        public virtual void Initialize() 
        {

        }

        /// <summary>
        /// Sets the predecessor DataProcessor. This method allows dynamic reconfiguration of the front end.
        /// </summary>
        /// <value>
        /// The new predecessor of this DataProcessor
        /// </value>
        public virtual IDataProcessor Predecessor { get; set; }
    }
}
