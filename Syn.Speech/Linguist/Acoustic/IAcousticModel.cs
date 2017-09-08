using System;
using System.Collections.Generic;
using System.Reflection;
using Syn.Speech.Common;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Linguist.Acoustic
{
    /// <summary>
    /// Represents the generic interface to the Acoustic Model for sphinx4
    /// </summary>
    public abstract class IAcousticModel:IConfigurable
    {
        /** The directory where the acoustic model data can be found. */
        [S4String(defaultValue = ".")]
        public static String PROP_LOCATION = "location";

        /**
        /// Gets this acoustic model ready to use, allocating all necessary resources.
         *
        /// @throws IOException if the model could not be loaded
         */
        public abstract void allocate();


        /** Deallocates previously allocated resources */
        public abstract void deallocate();


        /**
        /// Returns the name of this AcousticModel, or null if it has no name.
         *
        /// @return the name of this AcousticModel, or null if it has no name
         */
        public abstract String getName();


        /**
        /// Given a unit, returns the HMM that best matches the given unit. If exactMatch is false and an exact match is not
        /// found, then different word positions are used. If any of the contexts are non-silence filler units. a silence
        /// filler unit is tried instead.
         *
        /// @param unit       the unit of interest
        /// @param position   the position of the unit of interest
        /// @param exactMatch if true, only an exact match is acceptable.
        /// @return the HMM that best matches, or null if no match could be found.
         */
        public abstract IHMM lookupNearestHMM(IUnit unit, HMMPosition position, Boolean exactMatch);


        /**
        /// Returns an iterator that can be used to iterate through all the HMMs of the acoustic model
         *
        /// @return an iterator that can be used to iterate through all HMMs in the model. The iterator returns objects of
        ///         type <code>HMM</code>.
         */
        public abstract IEnumerator<IHMM> getHMMIterator();


        /**
        /// Returns an iterator that can be used to iterate through all the CI units in the acoustic model
         *
        /// @return an iterator that can be used to iterate through all CI units. The iterator returns objects of type
        ///         <code>Unit</code>
         */
        public abstract IEnumerator<IUnit> getContextIndependentUnitIterator();


        /**
        /// Returns the size of the left context for context dependent units
         *
        /// @return the left context size
         */
        public abstract int getLeftContextSize();


        /**
        /// Returns the size of the right context for context dependent units
         *
        /// @return the left context size
         */
        public abstract int getRightContextSize();


        /**
        /// Returns the properties of this acoustic model.
         *
        /// @return the properties of this acoustic model
         */
        public abstract PropertyInfo[] getProperties();
        public abstract void newProperties(PropertySheet ps);


        void IConfigurable.newProperties(PropertySheet ps)
        {
            newProperties(ps);
        }
    }
}
