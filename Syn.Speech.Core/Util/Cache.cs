using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Provides a simple object cache.
    ///
    /// Object stored in cache must properly implement {@link Object#hashCode hashCode} and {@link Object#equals equals}.
    ///
    /// <p><strong>Note that this class is not synchronized.</strong>
    /// If multiple threads access a cache concurrently, and at least one of
    /// the threads modifies the cache, it <i>must</i> be synchronized externally.
    /// This is typically accomplished by synchronizing on some object that
    /// naturally encapsulates the cache.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Cache<T>
    {
        private readonly HashMap<T, T> _map = new HashMap<T, T>();

        /**
        /// Puts the given object in the cache if it is not already present.
         *
        /// <p>If the object is already cached, than the instance that exists in the cached is returned.
        /// Otherwise, it is placed in the cache and null is returned.
         *
        /// @param object object to cache
        /// @return the cached object or null if the given object was not already cached
         */
        public T cache(T _object) 
        {
            if(_map.ContainsKey(_object))
            {
                Hits++;
            }
            else
            {
                _map.Add(_object, _object);
            }
            var result = _map[_object];
            return result;
        }

        /**
        /// Returns the number of cache hits, which is the number of times {@link #cache} was called
        /// and returned an object that already existed in the cache.
         *
        /// @return the number of cache hits
         */

        public int Hits { get; private set; }

        /**
        /// Returns the number of cache misses, which is the number of times {@link #cache} was called
        /// and returned null (after caching the object), effectively representing the size of the cache. 
         *
        /// @return the number of cache misses
         */

        public int Misses
        {
            get { return _map.Count; }
        }
    }
}
