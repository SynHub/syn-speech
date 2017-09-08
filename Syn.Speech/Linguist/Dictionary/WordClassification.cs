using System;
using System.Runtime.Serialization;
//REFACTORED
namespace Syn.Speech.Linguist.Dictionary
{
    /// <summary>
    /// Provides a classification of words
    /// </summary>
    [Serializable]
    public class WordClassification:ISerializable
    {
        /// <summary>
        /// Unconstructable...
        /// </summary>
        /// <param name="classificationName"></param>
        private WordClassification(String classificationName) 
        {
            ClassificationName = classificationName;
        }


        public string ClassificationName { get; private set; }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}
