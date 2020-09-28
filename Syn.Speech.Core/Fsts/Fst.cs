using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Syn.Speech.Logging;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Fsts
{
    /// <summary>
    /// A mutable finite state transducer implementation.
    /// 
    /// Holds an ArrayList of {@link edu.cmu.sphinx.fst.State} objects allowing
    /// additions/deletions.
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Fst
    {
        // fst states
        private List<State> _states;

        // initial state

        // input symbols map

        // output symbols map

        // semiring

        /**
        /// Default Constructor
         */
        public Fst() {
            _states = new List<State>();
        }

        /**
        /// Constructor specifying the initial capacity of the states ArrayList (this
        /// is an optimization used in various operations)
        /// 
        /// @param numStates
        ///            the initial capacity
         */
        public Fst(int numStates) 
        {
            if (numStates > 0) {
                _states = new List<State>(numStates);
            }
        }

        /**
        /// Constructor specifying the fst's semiring
        /// 
        /// @param s
        ///            the fst's semiring
         */
        public Fst(Semiring s) :this()
        {
            Semiring = s;
        }

        /**
        /// Get the initial states
         */

        public State Start { get; protected set; }

        /**
        /// Get the semiring
         */

        public Semiring Semiring { get; set; }

        /**
        /// Set the Semiring
        /// 
        /// @param semiring
        ///            the semiring to set
         */

        /**
        /// Set the initial state
        /// 
        /// @param start
        ///            the initial state
         */
        public void SetStart(State start) 
        {
            this.Start = start;
        }

        /**
        /// Get the number of states in the fst
         */
        public virtual int GetNumStates() 
        {
            return _states.Count;
        }

        public virtual State GetState(int index) 
        {
            return _states[index];
        }

        /**
        /// Adds a state to the fst
        /// 
        /// @param state
        ///            the state to be added
         */
        public virtual void AddState(State state) 
        {
            _states.Add(state);
            state.Id = _states.Count - 1;
        }

        /**
        /// Get the input symbols' array
         */

        public string[] Isyms { get; set; }

        /**
        /// Get the output symbols' array
         */

        public string[] Osyms { get; set; }

        /**
        /// Set the output symbols
        /// 
        /// @param osyms
        ///            the osyms to set
         */

        /**
        /// Serializes a symbol map to an ObjectOutputStream
        /// 
        /// @param out
        ///            the ObjectOutputStream. It should be already be initialized by
        ///            the caller.
        /// @param map
        ///            the symbol map to serialize
        /// @throws IOException
         */

        public void WriteStringMap(StreamWriter OutStream,String[] map)
        {
            OutStream.Write(map.Length);
            for (var i = 0; i < map.Length; i++) 
            {
                OutStream.Write(map[i]);
            }
        }

        /**
        /// Serializes the current Fst instance to an ObjectOutputStream
        /// 
        /// @param out
        ///            the ObjectOutputStream. It should be already be initialized by
        ///            the caller.
        /// @throws IOException
         */
        private void WriteFst(StreamWriter OutStream) 
        {
            WriteStringMap(OutStream,Isyms);
            WriteStringMap(OutStream,Osyms);
            OutStream.Write(_states.IndexOf(Start));

            OutStream.Write(Semiring);
            OutStream.Write(_states.Count);

            var stateMap = new Dictionary<State, int>(_states.Count);
            for (var i = 0; i < _states.Count; i++) 
            {
                var s = _states[i];
                OutStream.Write(s.GetNumArcs());
                OutStream.Write(s.FinalWeight);
                OutStream.Write(s.GetId());
                if(stateMap.ContainsKey(s))
                    stateMap[s]=i;
                else
                    stateMap.Add(s, i);
            }

            var numStates = _states.Count;
            for (var i = 0; i < numStates; i++) 
            {
                var s = _states[i];
                var numArcs = s.GetNumArcs();
                for (var j = 0; j < numArcs; j++) 
                {
                    var a = s.GetArc(j);
                    OutStream.Write(a.Ilabel);
                    OutStream.Write(a.Olabel);
                    OutStream.Write(a.Weight);
                    OutStream.Write(stateMap[a.NextState]);
                }
            }
        }

        /**
        /// Saves binary model to disk
        /// 
        /// @param filename
        ///            the binary model filename
        /// @throws IOException
         */
        public virtual void SaveModel(String filename)
        {
            var oos = new StreamWriter(filename);
            WriteFst(oos);
            oos.Flush();
            oos.Close();
        }

        /**
        /// Deserializes a symbol map from an ObjectInputStream
        /// 
        /// @param in
        ///            the ObjectInputStream. It should be already be initialized by
        ///            the caller.
        /// @return the deserialized symbol map
        /// @throws IOException
        /// @throws ClassNotFoundException
         */
        protected static String[] ReadStringMap(Stream inStream)
        {
            var serializer = new BinaryFormatter();
            
            var mapSize = (int)serializer.Deserialize(inStream);
            var map = new String[mapSize];
            for (var i = 0; i < mapSize; i++) {
                var sym = (String) serializer.Deserialize(inStream);
                map[i] = sym;
            }

            return map;
        }

        /**
        /// Deserializes an Fst from an ObjectInputStream
        /// 
        /// @param in
        ///            the ObjectInputStream. It should be already be initialized by
        ///            the caller.
        /// @return
        /// @throws IOException
        /// @throws ClassNotFoundException
         */
        private static Fst ReadFst(Stream inStream) 
        {
            var _is = ReadStringMap(inStream);
            var os = ReadStringMap(inStream);
            var serializer = new BinaryFormatter();

            var startid = (int)serializer.Deserialize(inStream);
            var semiring = (Semiring) serializer.Deserialize(inStream);
            var numStates = (int)serializer.Deserialize(inStream);
            var res = new Fst(numStates);
            res.Isyms = _is;
            res.Osyms = os;
            res.Semiring = semiring;
            for (var i = 0; i < numStates; i++) 
            {
                var numArcs = (int)serializer.Deserialize(inStream);
                var s = new State(numArcs + 1);
                var f = (float)serializer.Deserialize(inStream);
                if (f == res.Semiring.Zero) {
                    f = res.Semiring.Zero;
                } 
                else if (f == res.Semiring.One) 
                {
                    f = res.Semiring.One;
                }
                s.FinalWeight = f;
                s.Id = (int)serializer.Deserialize(inStream);
                res._states.Add(s);
            }
            res.SetStart(res._states[startid]);

            numStates = res.GetNumStates();
            for (var i = 0; i < numStates; i++) 
            {
                var s1 = res.GetState(i);
                for (var j = 0; j < s1.InitialNumArcs - 1; j++) {
                    var a = new Arc();
                    a.Ilabel = (int)serializer.Deserialize(inStream);
                    a.Olabel = (int)serializer.Deserialize(inStream);
                    a.Weight = (float)serializer.Deserialize(inStream);
                    a.NextState = res._states[(int)serializer.Deserialize(inStream)];
                    s1.AddArc(a);
                }
            }

            return res;
        }

        /**
        /// Deserializes an Fst from disk
        /// 
        /// @param filename
        ///            the binary model filename
        /// @throws IOException
        /// @throws ClassNotFoundException
         */
        public  static Fst LoadModel(String filename) 
        {
            var starttime = DateTime.Now;
            Fst obj;

            var fis = new FileStream(filename, FileMode.Open);
            var bis = new BufferedStream(fis, 8192);
            obj = ReadFst(bis);
            bis.Close();
            fis.Close();
            
            Logger.LogInfo<Fst>("Load Time: "
                    + (DateTime.Now - starttime) );
            return obj;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#equals(java.lang.Object)
         */

        public override bool Equals(Object obj)
        {
            //Note: NOT performance critical
            if (this == obj)
                return true;
            if (obj == null)
                return false;
            if (GetType() != obj.GetType())
                return false;
            var other = (Fst) obj;
            if (!Arrays.AreEqual(Isyms, other.Isyms))
                return false;
            if (!Arrays.AreEqual(Osyms, other.Osyms))
                return false;
            if (Start == null) {
                if (other.Start != null)
                    return false;
            } 
            else if (!Start.Equals(other.Start))
                return false;
            if (_states == null) 
            {
                if (other._states != null)
                    return false;
            }
            else if (!Arrays.AreEqual(_states,other._states))
                return false;
            if (Semiring == null) {
                if (other.Semiring != null)
                    return false;
            } 
            else if (!Semiring.Equals(other.Semiring))
                return false;
            return true;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see java.lang.Object#toString()
         */
        public override string ToString() 
        {
            var sb = new StringBuilder();
            sb.Append("Fst(start=" + Start + ", isyms=" + Isyms + ", osyms="
                    + Osyms + ", semiring=" + Semiring + ")\n");
            var numStates = _states.Count;
            for (var i = 0; i < numStates; i++) {
                var s = _states[i];
                sb.Append("  " + s + "\n");
                var numArcs = s.GetNumArcs();
                for (var j = 0; j < numArcs; j++) {
                    var a = s.GetArc(j);
                    sb.Append("    " + a + "\n");
                }
            }

            return sb.ToString();
        }

        /**
        /// Deletes a state
        /// 
        /// @param state
        ///            the state to delete
         */
        public virtual void DeleteState(State state) 
        {

            if (state == Start) {
                this.LogInfo("Cannot delete start state.");
                return;
            }

            _states.Remove(state);

            foreach (var s1 in _states) 
            {
                var newArcs = new List<Arc>();
                for (var j = 0; j < s1.GetNumArcs(); j++) 
                {
                    var a = s1.GetArc(j);
                    if (!a.NextState.Equals(state)) 
                    {
                        newArcs.Add(a);
                    }
                }
                s1.SetArcs(newArcs);
            }
        }

        /**
        /// Remaps the states' ids.
        /// 
        /// States' ids are renumbered starting from 0 up to @see
        /// {@link edu.cmu.sphinx.fst.Fst#getNumStates()}
         */
        public void RemapStateIds() 
        {
            var numStates = _states.Count;
            for (var i = 0; i < numStates; i++) {
                _states[i].Id = i;
            }

        }

        public void DeleteStates(HashSet<State> toDelete) 
        {

            if (toDelete.Contains(Start)) 
            {
                this.LogInfo("Cannot delete start state.");
                return;
            }

            var newStates = new List<State>();

            foreach (var s1 in _states) 
            {
                if (!toDelete.Contains(s1)) 
                {
                    newStates.Add(s1);
                    var newArcs = new List<Arc>();
                    for (var j = 0; j < s1.GetNumArcs(); j++) 
                    {
                        var a = s1.GetArc(j);
                        if (!toDelete.Contains(a.NextState)) 
                        {
                            newArcs.Add(a);
                        }
                    }
                    s1.SetArcs(newArcs);
                }
            }
            _states = newStates;

            RemapStateIds();
        }

    }
}
