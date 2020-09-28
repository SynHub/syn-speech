using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util.Props
{
    /// <summary>
    /// An default (abstract) implementation of a configurable that implements a meaning {@code toString()} and keeps a
    /// references to the {@code Confurable}'s logger.
    /// @author Holger Brandl
    /// </summary>
    public abstract class ConfigurableAdapter : IConfigurable
    {
        private string _name;

        public ConfigurableAdapter() 
        {
        }

        virtual public void NewProperties(PropertySheet ps)
        {
            Init(ps.InstanceName);
        }

        private void Init(String name) 
        {
            _name = name;
        }

        /** Returns the configuration name this {@code Configurable}. */

        public virtual string Name
        {
            get
            {
                // fix null names
                return _name ?? GetType().Name;
            }
        }


        /**
        /// Returns the name of this BaseDataProcessor.
         *
        /// @return the name of this BaseDataProcessor
         */

        public override string ToString() 
        {
            return Name;
        }

        void IConfigurable.NewProperties(PropertySheet ps)
        {
            NewProperties(ps);
        }
    }
}
