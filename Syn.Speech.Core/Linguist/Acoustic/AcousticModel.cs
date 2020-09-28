using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents the generic interface to the Acoustic Model for sphinx4
    /// </summary>
    public abstract class AcousticModel:IConfigurable
    {
        /// <summary>
        /// The directory where the acoustic model data can be found.
        /// </summary>
        [S4String(DefaultValue = ".")]
        public static string PropLocation = "location";

        /// <summary>
        /// Gets this acoustic model ready to use, allocating all necessary resources.
        /// </summary>
        public abstract void Allocate();


        /// <summary>
        /// Deallocates previously allocated resources.
        /// </summary>
        public abstract void Deallocate();


        /// <summary>
        /// Gets the name of this AcousticModel, or null if it has no name.
        /// </summary>
        /// <value>
        /// The name of this AcousticModel, or null if it has no name.
        /// </value>
        public abstract string Name { get; }

        /// <summary>
        /// Given a unit, returns the HMM that best matches the given unit. If exactMatch is false and an exact match is not
        /// found, then different word positions are used. If any of the contexts are non-silence filler units. a silence
        /// filler unit is tried instead.
        /// </summary>
        /// <param name="unit">The unit of interest.</param>
        /// <param name="position">The position of the unit of interest.</param>
        /// <param name="exactMatch">if true, only an exact match is acceptable..</param>
        /// <returns>The HMM that best matches, or null if no match could be found.</returns>
        public abstract IHMM LookupNearestHMM(Unit unit, HMMPosition position, Boolean exactMatch);

        /// <summary>
        /// Gets the HMM iterator.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<IHMM> GetHMMIterator();


        /// <summary>
        /// Gets the Enumerator that can be used to iterate through all the CI units in the acoustic model
        /// </summary>
        /// <returns>An Enumerator that can be used to iterate through all CI units. The Enumerator returns objects of type <code>Unit</code></returns>
        public abstract IEnumerator<Unit> GetContextIndependentUnitIterator();

        /// <summary>
        /// Gets the size of the left context for context dependent units.
        /// </summary>
        /// <returns>The left context size.</returns>
        public abstract int GetLeftContextSize();


    
        /// <summary>
        /// Gets the size of the right context for context dependent units.
        /// </summary>
        /// <returns>The left context size.</returns>
        public abstract int GetRightContextSize();


        /// <summary>
        /// Gets the properties of this acoustic model.
        /// </summary>
        /// <returns>The properties of this acoustic model.</returns>
        public abstract JProperties[] GetProperties();

        public abstract void NewProperties(PropertySheet ps);

        void IConfigurable.NewProperties(PropertySheet ps)
        {
            NewProperties(ps);
        }
    }
}
