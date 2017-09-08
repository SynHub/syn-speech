using System;
using System.Collections.Generic;
using Syn.Logging;
//PATROLLED + REFACTORED
using Syn.Speech.Helper;

namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    public enum Feature { NumSenones, NumGaussiansPerState, NumStreams };
    /// <summary>
    /// Used to pool shared objects in the acoustic model
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T>
    {
        public  List<T> pool;
        private readonly HashMap<Feature, int> _features = new HashMap<Feature, int>(); //TODO: Find Replacement for Java's EnumMap

        /**
        /// Creates a new pool.
         *
        /// @param name the name of the pool
         */
        public Pool(String name) 
        {
            this.Name = name;
            pool = new List<T>();
        }

        /**
        /// Returns the pool's name.
         *
        /// @return the pool name
         */

        public string Name { get; private set; }

        /**
        /// Returns the object with the given ID from the pool.
         *
        /// @param id the id of the object
        /// @return the object
        /// @throws IndexOutOfBoundsException if the ID is out of range
         */
        public T Get(int id) 
        {
            return pool[id];
        }

        /**
        /// Returns the ID of a given object from the pool.
         *
        /// @param object the object
        /// @return the index
         */
        public int IndexOf(T @object) 
        {
            return pool.IndexOf(@object);
        }

        /**
        /// Places the given object in the pool.
         *
        /// @param id a unique ID for this object
        /// @param o  the object to add to the pool
         */
        public void Put(int id, T o) 
        {
            if (id == pool.Count) 
            {
                pool.Add(o);
            } 
            else 
            {
                pool[id] =  o;
            }
        }

        /**
        /// Retrieves the size of the pool.
         *
        /// @return the size of the pool
         */

        public int Size
        {
            get { return pool.Count; }
        }

        /**
        /// Dump information on this pool to the given logger.
         *
        /// @param logger the logger to send the info to
         */
        public void LogInfo() 
        {
            this.LogInfo("Pool " + Name + " Entries: " + Size);
        }

        /**
        /// Sets a feature for this pool.
         *
        /// @param feature feature to set
        /// @param value the value for the feature
         */
        public void SetFeature(Feature feature, int value)
        {
            _features.Put(feature, value);
        }

        /**
        /// Retrieves a feature from this pool.
         *
        /// @param feature feature to get
        /// @param defaultValue the defaultValue for the pool
        /// @return the value for the feature
         */
        public int GetFeature(Feature feature, int defaultValue) 
        {
            Integer val = _features.Get(feature);
            return val == null ? defaultValue :  (int) val;
        }
    }
}
