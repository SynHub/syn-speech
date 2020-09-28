using System;
using System.Collections.Generic;
using Syn.Speech.Helper;

//REFACTORED
namespace Syn.Speech.Linguist.Util
{
    /// <summary>
    /// An LRU cache
    /// </summary>
    /// <typeparam name="TK"></typeparam>
    /// <typeparam name="TV"></typeparam>
    public class LRUCache<TK, TV>:LinkedHashMap<TK, TV> 
    {
        readonly int _maxSize;


        /**
        /// Creates an LRU cache with the given maximum size
         *
        /// @param maxSize the maximum size of the cache
         */
        public LRUCache(int maxSize) 
        {
            _maxSize = maxSize;
        }


        /**
        /// Determines if the eldest entry in the map should be removed.
         *
        /// @param eldest the eldest entry
        /// @return true if the eldest entry should be removed
         */
        
        protected Boolean RemoveEldestEntry(KeyValuePair<TK,TV> eldest) 
        {
            return Count > _maxSize;
        }
    }

}
