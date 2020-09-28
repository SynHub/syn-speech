using System.Globalization;
using System.IO;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts
{
    public class Convert
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="Convert"/> class from being created.
        /// </summary>
        private Convert() { }

        public static void Export(Fst fst, string basename)
        {
            ExportSymbols(fst.Isyms, basename + ".input.syms");
            ExportSymbols(fst.Osyms, basename + ".output.syms");
            ExportFst(fst, basename + ".fst.txt");
        }

        private static void ExportFst(Fst fst, string filename)
        {

            StreamWriter streamWriter = new StreamWriter(filename);

            // print start first
            State start = fst.Start;
            streamWriter.WriteLine(start.GetId() + "\t" + start.FinalWeight);

            // print all states
            int numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                if (s.GetId() != fst.Start.GetId())
                {
                    streamWriter.WriteLine(s.GetId() + "\t" + s.FinalWeight);
                }
            }


            string[] isyms = fst.Isyms;
            string[] osyms = fst.Osyms;
            numStates = fst.GetNumStates();
            for (int i = 0; i < numStates; i++)
            {
                State s = fst.GetState(i);
                int numArcs = s.GetNumArcs();
                for (int j = 0; j < numArcs; j++)
                {
                    Arc arc = s.GetArc(j);
                    string isym = (isyms != null) ? isyms[arc.Ilabel] : Integer.ToString(arc.Ilabel);
                    string osym = (osyms != null) ? osyms[arc.Olabel] : Integer.ToString(arc.Olabel);

                    streamWriter.WriteLine(s.GetId() + "\t" + arc.NextState.GetId()
                            + "\t" + isym + "\t" + osym + "\t" + arc.Weight);
                }
            }

            streamWriter.Close();

        }

        private static void ExportSymbols(string[] syms, string filename)
        {
            if (syms == null) return;

            var streamWriter = new StreamWriter(filename);

            for (int i = 0; i < syms.Length; i++)
            {
                string key = syms[i];
                streamWriter.WriteLine(key + "\t" + i);
            }

            streamWriter.Close();

        }

        public static HashMap<string, Integer> ImportSymbols(string fileName)
        {
            var symfile = new FileInfo(fileName);
            if (!(symfile.Exists)) return null;

            var fileStream = new FileStream(fileName, FileMode.Open);
            TextReader textReader = new StreamReader(fileStream);

            var syms = new HashMap<string, Integer>();
            string strLine;

            while ((strLine = textReader.ReadLine()) != null)
            {
                string[] tokens = strLine.Split("\\t");
                string sym = tokens[0];
                Integer index = Integer.ParseInt(tokens[1]);
                syms.Put(sym, index);
            }

            textReader.Close();

            return syms;
        }

        public static Fst ImportFst(string basename, Semiring semiring)
        {
            Fst fst = new Fst(semiring);

            HashMap<string, Integer> isyms = ImportSymbols(basename + ".input.syms");
            if (isyms == null)
            {
                isyms = new HashMap<string, Integer>();
                isyms.Put("<eps>", 0);
            }

            HashMap<string, Integer> osyms = ImportSymbols(basename
                    + ".output.syms");
            if (osyms == null)
            {
                osyms = new HashMap<string, Integer>();
                osyms.Put("<eps>", 0);
            }

            HashMap<string, Integer> ssyms = ImportSymbols(basename + ".states.syms");

            // Parse input
            FileStream fis = new FileStream(basename + ".fst.txt", FileMode.Open);
            TextReader br = new StreamReader(fis);

            bool firstLine = true;
            string strLine;
            HashMap<Integer, State> stateMap = new HashMap<Integer, State>();

            while ((strLine = br.ReadLine()) != null)
            {
                string[] tokens = strLine.Split("\\t");
                Integer inputStateId;
                if (ssyms == null)
                {
                    inputStateId = Integer.ParseInt(tokens[0]);
                }
                else
                {
                    inputStateId = ssyms.Get(tokens[0]);
                }
                State inputState = stateMap.Get(inputStateId);
                if (inputState == null)
                {
                    inputState = new State(semiring.Zero);
                    fst.AddState(inputState);
                    stateMap.Put(inputStateId, inputState);
                }

                if (firstLine)
                {
                    firstLine = false;
                    fst.SetStart(inputState);
                }

                if (tokens.Length > 2)
                {
                    Integer nextStateId;
                    if (ssyms == null)
                    {
                        nextStateId = Integer.ParseInt(tokens[1]);
                    }
                    else
                    {
                        nextStateId = ssyms.Get(tokens[1]);
                    }

                    State nextState = stateMap.Get(nextStateId);
                    if (nextState == null)
                    {
                        nextState = new State(semiring.Zero);
                        fst.AddState(nextState);
                        stateMap.Put(nextStateId, nextState);
                    }
                    // Adding arc
                    if (isyms.Get(tokens[2]) == null)
                    {
                        isyms.Put(tokens[2], isyms.Size());
                    }
                    int? iLabel = isyms.Get(tokens[2]);
                    if (osyms.Get(tokens[3]) == null)
                    {
                        osyms.Put(tokens[3], osyms.Size());
                    }
                    int? oLabel = osyms.Get(tokens[3]);

                    float arcWeight;
                    if (tokens.Length > 4)
                    {
                        arcWeight = Float.ParseFloat(tokens[4]);
                    }
                    else
                    {
                        arcWeight = 0;
                    }
                    Arc arc = new Arc(iLabel.Value, oLabel.Value, arcWeight, nextState);
                    inputState.AddArc(arc);
                }
                else
                {
                    if (tokens.Length > 1)
                    {
                        float finalWeight = Float.ParseFloat(tokens[1]);
                        inputState.FinalWeight = finalWeight;
                    }
                    else
                    {
                        inputState.FinalWeight = 0.0f;
                    }
                }
            }
            br.Close();

            fst.Isyms = Utils.Utils.ToStringArray(isyms);
            fst.Osyms = Utils.Utils.ToStringArray(osyms);

            return fst;
        }
    }
}
