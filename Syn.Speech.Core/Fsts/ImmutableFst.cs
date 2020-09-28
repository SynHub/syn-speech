using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Fsts
{
    /// <summary>
    /// An immutable finite state transducer implementation.
    /// 
    /// Holds a fixed size array of {@link edu.cmu.sphinx.fst.ImmutableState} objects
    /// not allowing additions/deletions
    /// </summary>
    public class ImmutableFst: Fst
    {
        // fst states
        private readonly ImmutableState[] _states;

        // number of states
        private readonly int _numStates;

        /**
        /// Default private constructor.
        /// 
        /// An ImmutableFst cannot be created directly. It needs to be deserialized.
        /// 
        /// @see edu.cmu.sphinx.fst.ImmutableFst#loadModel(String)
         */
        private ImmutableFst() {

        }

        /**
        /// Private Constructor specifying the capacity of the states array
        /// 
        /// An ImmutableFst cannot be created directly. It needs to be deserialized.
        /// 
        /// @see edu.cmu.sphinx.fst.ImmutableFst#loadModel(String)
        /// 
        /// @param numStates
        ///            the number of fst's states
         */
        private ImmutableFst(int numStates)  :base(0)
        {
            _numStates = numStates;
            _states = new ImmutableState[numStates];
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.Fst#getNumStates()
         */
        public override int GetNumStates() 
        {
            return _numStates;
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.Fst#getState(int)
         */
        public new ImmutableState GetState(int index) 
        {
            return _states[index];
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.Fst#addState(edu.cmu.sphinx.fst.State)
         */
        public override void AddState(State state) 
        {
            throw new InvalidOperationException("You cannot modify an ImmutableFst.");
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.Fst#saveModel(java.lang.String)
         */
        public override void SaveModel(String filename)
        {
            throw new InvalidOperationException("You cannot serialize an ImmutableFst.");
        }

        /**
        /// Deserializes an ImmutableFst from an ObjectInputStream
        /// 
        /// @param in
        ///            the ObjectInputStream. It should be already be initialized by
        ///            the caller.
        /// @return
        /// @throws IOException
        /// @throws ClassNotFoundException
         */
        private static ImmutableFst ReadImmutableFst(Stream inStream)
        {
            BinaryFormatter serializer = new BinaryFormatter();
            String[] _is = ReadStringMap(inStream);
            String[] os = ReadStringMap(inStream);
            int startid = (int)serializer.Deserialize(inStream);
            Semiring semiring = (Semiring) serializer.Deserialize(inStream);
            int numStates = (int)serializer.Deserialize(inStream);
            ImmutableFst res = new ImmutableFst(numStates);
            res.Isyms = _is;
            res.Osyms = os;
            res.Semiring = semiring;
            for (int i = 0; i < numStates; i++) 
            {
                int numArcs = (int)serializer.Deserialize(inStream);
                ImmutableState s = new ImmutableState(numArcs + 1);
                float f = (float)serializer.Deserialize(inStream);
                if (f == res.Semiring.Zero) {
                    f = res.Semiring.Zero;
                } else if (f == res.Semiring.One) 
                {
                    f = res.Semiring.One;
                }
                s.FinalWeight = f;
                s.Id = (int)serializer.Deserialize(inStream);
                res._states[s.GetId()] = s;
            }
            res.SetStart(res._states[startid]);

            numStates = res._states.Length;
            for (int i = 0; i < numStates; i++) 
            {
                ImmutableState s1 = res._states[i];
                for (int j = 0; j < s1.InitialNumArcs - 1; j++) {
                    Arc a = new Arc();
                    a.Ilabel = (int)serializer.Deserialize(inStream);
                    a.Olabel = (int)serializer.Deserialize(inStream);
                    a.Weight = (float)serializer.Deserialize(inStream);
                    a.NextState = res._states[(int)serializer.Deserialize(inStream)];
                    s1.SetArc(j, a);
                }
            }

            return res;
        }

        public static ImmutableFst LoadModel(Stream inputStream){
        ImmutableFst obj;

        BufferedStream bis = null;
        bis = new BufferedStream(inputStream);
        //ois = new ObjectInputStream(bis);
        obj = ReadImmutableFst(bis);
        //ois.close();
        bis.Close();
        inputStream.Close();

        return obj;
    }

        /// <summary>
        /// Deserializes an ImmutableFst from disk
        /// </summary>
        /// <param name="filename">the binary model filename</param>
        /// <returns></returns>
        public new static ImmutableFst LoadModel(String filename) 
        {
            ImmutableFst obj=null;

            try {
                FileStream fis = new FileStream(filename, FileMode.Open);
                BufferedStream bis = new BufferedStream(fis);
                obj = ReadImmutableFst(bis);
                bis.Close();
                fis.Close();
            } 
            catch (FileNotFoundException e) {
                Trace.Write(e);
            } 
            catch (IOException e) 
            {
                Trace.Write(e);
            } 
            catch (Exception e) 
            {
                Trace.Write(e);
            }

            //return an empty object not null
            return obj==null?new ImmutableFst():obj;
        }
        
        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.Fst#deleteState(edu.cmu.sphinx.fst.State)
         */
        public override void DeleteState(State state) 
        {
            throw new InvalidOperationException("You cannot modify an ImmutableFst.");
        }

        /*
        /// (non-Javadoc)
        /// 
        /// @see edu.cmu.sphinx.fst.Fst#toString()
         */
        public override string ToString() 
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Fst(start=" + Start + ", isyms=" + Isyms + ", osyms="
                    + Osyms + ", semiring=" + Semiring + ")\n");
            int numStates = _states.Length;
            for (int i = 0; i < numStates; i++) 
            {
                State s = _states[i];
                sb.Append("  " + s + "\n");
                int numArcs = s.GetNumArcs();
                for (int j = 0; j < numArcs; j++) {
                    Arc a = s.GetArc(j);
                    sb.Append("    " + a + "\n");
                }
            }

            return sb.ToString();
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
            if (GetType() != obj.GetType())
                return false;
            var other = (ImmutableFst) obj;
            if (!Arrays.AreEqual(_states, other._states))
                return false;
            if (!base.Equals(obj))
                return false;
            return true;
        }
    }
}
