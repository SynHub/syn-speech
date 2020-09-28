using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
//PATROLLED + REFACTORED
namespace Syn.Speech.FrontEnds
{
    /// <summary>
    ///  Indicates events like beginning or end of data, data dropped, quality changed, etc.. It implements the Data
    /// interface, and it will pass between DataProcessors to inform them about the Data that is passed between
    /// DataProcessors.
    ///
    /// @see Data
    /// @see DataProcessor
    /// </summary>
    public class Signal: IData
    {
        /// <summary>
        /// A (lazily initialized) collection of names properties of this signal. This collection might contain infos about
        /// the file being processed, shift-size of frame-length of the windowing process, etc.
        /// </summary>
        private Dictionary<String, Object> _props;

        /// <summary>
        /// Constructs a Signal with the given name.
        /// </summary>
        /// <param name="time">The time this Signal is created.</param>
        protected Signal(long time) 
        {
            Time = time;
        }

        /// <summary>
        /// Returns the time this Signal was created.
        /// </summary>
        /// <value>The time this Signal was created</value>
        public long Time { get; private set; }

        /// <summary>
        /// Returns the properties associated to this signal.
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, Object> GetProps()
        {
            return _props ?? (_props = new Dictionary<String, Object>());
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
