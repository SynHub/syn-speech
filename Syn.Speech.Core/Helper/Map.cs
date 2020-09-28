using System.Collections.Generic;

namespace Syn.Speech.Helper
{
    /// <summary>
    /// Extended dictionary functionality.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>

    public interface Map<K, V> : IDictionary<K, V>
    {
        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then the defaultValue is
        /// returned.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>

        V Get(K key, V defaultValue);

        /// <summary>
        /// Fetches the value associated with the specified key.
        /// If no value can be found, then default(V) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        V Get(K key);

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to the new value.
        /// </summary>

        void Put(K key, V value);

        /// <summary>
        /// Sets the given key in the dictionary.  If the key
        /// already exists, then it is remapped to the new value.
        /// If a value was previously mapped it is returned.
        /// </summary>

        V Push(K key, V value);

        /// <summary>
        /// Puts all values from the source dictionary into
        /// this dictionary.
        /// </summary>
        /// <param name="source"></param>

        void PutAll(IDictionary<K, V> source);

        /// <summary>
        /// Returns the first value in the enumeration of values
        /// </summary>
        /// <returns></returns>

        V FirstValue { get; }

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.
        /// </summary>
        /// <param name="key">Search key into the dictionary</param>
        /// <param name="value">The value removed from the dictionary (if found).</param>
        /// <returns></returns>

        bool Remove(K key, out V value);

        /// <summary>
        /// Removes the item from the dictionary that is associated with
        /// the specified key.  The item if found is returned; if not,
        /// default(V) is returned.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>

        V RemoveAndReturn(K key);
    }
}
