using System;
using System.Runtime.Serialization;

namespace Syn.Speech.Helper.Mathematics.Util
{
  
public class OpenIntToDoubleHashMap : ISerializable {

    /** Status indicator for free table entries. */
    protected const byte FREE    = 0;

    /** Status indicator for full table entries. */
    protected const byte FULL    = 1;

    /** Status indicator for removed table entries. */
    protected const byte REMOVED = 2;

    /** Load factor for the map. */
    private const float LOAD_FACTOR = 0.5f;

    /** Default starting size.
     * <p>This must be a power of two for bit mask to work properly. </p>
     */
    private const int DEFAULT_EXPECTED_SIZE = 16;

    /** Multiplier for size growth when map fills up.
     * <p>This must be a power of two for bit mask to work properly. </p>
     */
    private const int RESIZE_MULTIPLIER = 2;

    /** Number of bits to perturb the index when probing for collision resolution. */
    private const int PERTURB_SHIFT = 5;

    /** Keys table. */
    private int[] keys;

    /** Values table. */
    private double[] values;

    /** States table. */
    private byte[] states;

    /** Return value for missing entries. */
    private readonly double missingEntries;

    /** Current size of the map. */
    private int _size;

    /** Bit mask for hash values. */
    private int mask;

    /** Modifications count. */
    private  int count;

    /**
     * Build an empty map with default size and using NaN for missing entries.
     */
    public OpenIntToDoubleHashMap() :this(DEFAULT_EXPECTED_SIZE, Double.NaN) {
        ;
    }

    /**
     * Build an empty map with default size
     * @param missingEntries value to return when a missing entry is fetched
     */
    public OpenIntToDoubleHashMap(double missingEntries) :this(DEFAULT_EXPECTED_SIZE, missingEntries) {

    }

    /**
     * Build an empty map with specified size and using NaN for missing entries.
     * @param expectedSize expected number of elements in the map
     */
    public OpenIntToDoubleHashMap( int expectedSize) :this(expectedSize, Double.NaN) {
    }

    /**
     * Build an empty map with specified size.
     * @param expectedSize expected number of elements in the map
     * @param missingEntries value to return when a missing entry is fetched
     */
    public OpenIntToDoubleHashMap( int expectedSize,double missingEntries) {
        int capacity = computeCapacity(expectedSize);
        keys   = new int[capacity];
        values = new double[capacity];
        states = new byte[capacity];
        this.missingEntries = missingEntries;
        mask   = capacity - 1;
    }

    /**
     * Copy constructor.
     * @param source map to copy
     */
    public OpenIntToDoubleHashMap(OpenIntToDoubleHashMap source) {
        int length = source.keys.Length;
        keys = new int[length];
        Array.Copy(source.keys, 0, keys, 0, length);
        values = new double[length];
        Array.Copy(source.values, 0, values, 0, length);
        states = new byte[length];
        Array.Copy(source.states, 0, states, 0, length);
        missingEntries = source.missingEntries;
        _size  = source._size;
        mask  = source.mask;
        count = source.count;
    }

    /**
     * Compute the capacity needed for a given size.
     * @param expectedSize expected size of the map
     * @return capacity to use for the specified size
     */
    private static int computeCapacity(int expectedSize) {
        if (expectedSize == 0) {
            return 1;
        }
         int capacity   = (int) Math.Ceiling(expectedSize / LOAD_FACTOR);
         int powerOfTwo = Integer.HighestOneBit(capacity);
        if (powerOfTwo == capacity) {
            return capacity;
        }
        return nextPowerOfTwo(capacity);
    }

    /**
     * Find the smallest power of two greater than the input value
     * @param i input value
     * @return smallest power of two greater than the input value
     */
    private static int nextPowerOfTwo(int i) {
        return Integer.HighestOneBit(i) << 1;
    }

    /**
     * Get the stored value associated with the given key
     * @param key key associated with the data
     * @return data associated with the key
     */
    public double get(int key) {

         int hash  = hashOf(key);
        int index = hash & mask;
        if (containsKey(key, index)) {
            return values[index];
        }

        if (states[index] == FREE) {
            return missingEntries;
        }

        int j = index;
        for (int perturbShift = perturb(hash); states[index] != FREE; perturbShift >>= PERTURB_SHIFT) {
            j = probe(perturbShift, j);
            index = j & mask;
            if (containsKey(key, index)) {
                return values[index];
            }
        }

        return missingEntries;

    }

    /**
     * Check if a value is associated with a key.
     * @param key key to check
     * @return true if a value is associated with key
     */
    public bool containsKey(int key) {

        int hash  = hashOf(key);
        int index = hash & mask;
        if (containsKey(key, index)) {
            return true;
        }

        if (states[index] == FREE) {
            return false;
        }

        int j = index;
        for (int perturbShift = perturb(hash); states[index] != FREE; perturbShift >>= PERTURB_SHIFT) {
            j = probe(perturbShift, j);
            index = j & mask;
            if (containsKey(key, index)) {
                return true;
            }
        }

        return false;

    }

    /**
     * Get an iterator over map elements.
     * <p>The specialized iterators returned are fail-fast: they throw a
     * <code>ConcurrentModificationException</code> when they detect the map
     * has been modified during iteration.</p>
     * @return iterator over the map elements
     */
    public Iterator iterator() {
        return new Iterator(this);
    }

    /**
     * Perturb the hash for starting probing.
     * @param hash initial hash
     * @return perturbed hash
     */
    private static int perturb(int hash) {
        return hash & 0x7fffffff;
    }

    /**
     * Find the index at which a key should be inserted
     * @param key key to lookup
     * @return index at which key should be inserted
     */
    private int findInsertionIndex(int key) {
        return findInsertionIndex(keys, states, key, mask);
    }

    /**
     * Find the index at which a key should be inserted
     * @param keys keys table
     * @param states states table
     * @param key key to lookup
     * @param mask bit mask for hash values
     * @return index at which key should be inserted
     */
    private static int findInsertionIndex(int[] keys,  byte[] states,int key,  int mask) {
         int hash = hashOf(key);
        int index = hash & mask;
        if (states[index] == FREE) {
            return index;
        } else if (states[index] == FULL && keys[index] == key) {
            return changeIndexSign(index);
        }

        int perturbShift = perturb(hash);
        int j = index;
        if (states[index] == FULL) {
            while (true) {
                j = probe(perturbShift, j);
                index = j & mask;
                perturbShift >>= PERTURB_SHIFT;

                if (states[index] != FULL || keys[index] == key) {
                    break;
                }
            }
        }

        if (states[index] == FREE) {
            return index;
        } else if (states[index] == FULL) {
            // due to the loop exit condition,
            // if (states[index] == FULL) then keys[index] == key
            return changeIndexSign(index);
        }

        int firstRemoved = index;
        while (true) {
            j = probe(perturbShift, j);
            index = j & mask;

            if (states[index] == FREE) {
                return firstRemoved;
            } else if (states[index] == FULL && keys[index] == key) {
                return changeIndexSign(index);
            }

            perturbShift >>= PERTURB_SHIFT;

        }

    }

    /**
     * Compute next probe for collision resolution
     * @param perturb perturbed hash
     * @param j previous probe
     * @return next probe
     */
    private static int probe(int perturb,  int j) {
        return (j << 2) + j + perturb + 1;
    }

    /**
     * Change the index sign
     * @param index initial index
     * @return changed index
     */
    private static int changeIndexSign( int index) {
        return -index - 1;
    }

    /**
     * Get the number of elements stored in the map.
     * @return number of elements stored in the map
     */
    public int size() {
        return _size;
    }


    /**
     * Remove the value associated with a key.
     * @param key key to which the value is associated
     * @return removed value
     */
    public double remove(int key) {

         int hash  = hashOf(key);
        int index = hash & mask;
        if (containsKey(key, index)) {
            return doRemove(index);
        }

        if (states[index] == FREE) {
            return missingEntries;
        }

        int j = index;
        for (int perturbShift = perturb(hash); states[index] != FREE; perturbShift >>= PERTURB_SHIFT) {
            j = probe(perturbShift, j);
            index = j & mask;
            if (containsKey(key, index)) {
                return doRemove(index);
            }
        }

        return missingEntries;

    }

    /**
     * Check if the tables contain an element associated with specified key
     * at specified index.
     * @param key key to check
     * @param index index to check
     * @return true if an element is associated with key at index
     */
    private bool containsKey(int key,  int index) {
        return (key != 0 || states[index] == FULL) && keys[index] == key;
    }

    /**
     * Remove an element at specified index.
     * @param index index of the element to remove
     * @return removed value
     */
    private double doRemove(int index) {
        keys[index]   = 0;
        states[index] = REMOVED;
        double previous = values[index];
        values[index] = missingEntries;
        --_size;
        ++count;
        return previous;
    }

    /**
     * Put a value associated with a key in the map.
     * @param key key to which value is associated
     * @param value value to put in the map
     * @return previous value associated with the key
     */
    public double put(int key,  double value) {
        int index = findInsertionIndex(key);
        double previous = missingEntries;
        bool newMapping = true;
        if (index < 0) {
            index = changeIndexSign(index);
            previous = values[index];
            newMapping = false;
        }
        keys[index]   = key;
        states[index] = FULL;
        values[index] = value;
        if (newMapping) {
            ++_size;
            if (shouldGrowTable()) {
                growTable();
            }
            ++count;
        }
        return previous;

    }

    /**
     * Grow the tables.
     */
    private void growTable() {

         int oldLength      = states.Length;
         int[] oldKeys      = keys;
         double[] oldValues = values;
         byte[] oldStates   = states;

         int newLength = RESIZE_MULTIPLIER * oldLength;
         int[] newKeys = new int[newLength];
         double[] newValues = new double[newLength];
         byte[] newStates = new byte[newLength];
         int newMask = newLength - 1;
        for (int i = 0; i < oldLength; ++i) {
            if (oldStates[i] == FULL) {
                 int key = oldKeys[i];
                 int index = findInsertionIndex(newKeys, newStates, key, newMask);
                newKeys[index]   = key;
                newValues[index] = oldValues[i];
                newStates[index] = FULL;
            }
        }

        mask   = newMask;
        keys   = newKeys;
        values = newValues;
        states = newStates;

    }

    /**
     * Check if tables should grow due to increased size.
     * @return true if  tables should grow
     */
    private bool shouldGrowTable() {
        return _size > (mask + 1) * LOAD_FACTOR;
    }

    /**
     * Compute the hash value of a key
     * @param key key to hash
     * @return hash value of the key
     */
    private static int hashOf( int key) {
        //int h = key ^ ((key >>> 20) ^ (key >>> 12));
        int h = key ^ ((Java.TripleShift(key,20)) ^ (Java.TripleShift(key,12)));
        //return h ^ (h >>> 7) ^ (h >>> 4);
        return h ^ (Java.TripleShift(h, 7)) ^ (Java.TripleShift(h, 4));
    }


    /** Iterator class for the map. */
    public class Iterator
    {

        private readonly OpenIntToDoubleHashMap _parent;

        /** Reference modification count. */
        private readonly int referenceCount;

        /** Index of current element. */
        private int current;

        /** Index of next element. */
        private int next;

        public Iterator(OpenIntToDoubleHashMap parent)
        {
            _parent = parent;
        }

        /**
         * Simple constructor.
         */
        private Iterator() {

            // preserve the modification count of the map to detect concurrent modifications later
            referenceCount = _parent.count;

            // initialize current index
            next = -1;
            try {
                advance();
            } catch (NoSuchElementException nsee) { // NOPMD
                // ignored
            }

        }

        /**
         * Check if there is a next element in the map.
         * @return true if there is a next element
         */
        public bool hasNext() {
            return next >= 0;
        }

        /**
         * Get the key of current entry.
         * @return key of current entry
         * @exception ConcurrentModificationException if the map is modified during iteration
         * @exception NoSuchElementException if there is no element left in the map
         */
        public int key() {
            if (referenceCount != _parent.count)
            {
                throw new Exception("ConcurrentModificationException");
            }
            if (current < 0) {
                throw new NoSuchElementException();
            }
            return _parent.keys[current];
        }

        /**
         * Get the value of current entry.
         * @return value of current entry
         * @exception ConcurrentModificationException if the map is modified during iteration
         * @exception NoSuchElementException if there is no element left in the map
         */
        public double value(){
            if (referenceCount != _parent.count)
            {
                throw new Exception("ConcurrentModificationException");
            }
            if (current < 0) {
                throw new NoSuchElementException();
            }
            return _parent.values[current];
        }

        /**
         * Advance iterator one step further.
         * @exception ConcurrentModificationException if the map is modified during iteration
         * @exception NoSuchElementException if there is no element left in the map
         */
        public void advance(){

            if (referenceCount != _parent.count)
            {
                throw new Exception("ConcurrentModificationException");
            }

            // advance on step
            current = next;

            // prepare next step
            try {
                while (_parent.states[++next] != FULL)
                { // NOPMD
                    // nothing to do
                }
            } catch (IndexOutOfRangeException e) {
                next = -2;
                if (current < 0) {
                    throw new NoSuchElementException();
                }
            }

        }

    }

    /**
     * Read a serialized object.
     * @param stream input stream
     * @throws IOException if object cannot be read
     * @throws ClassNotFoundException if the class corresponding
     * to the serialized object cannot be found
     */
    //private void readObject(ObjectInputStream stream) { //Todo: Find replacement
    //    stream.defaultReadObject();
    //    count = 0;
    //}


    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        throw new NotImplementedException();
    }
}
}
