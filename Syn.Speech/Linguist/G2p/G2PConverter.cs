using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Syn.Speech.Fsts;
using Syn.Speech.Fsts.Operations;
using Syn.Speech.Fsts.Semirings;
using Syn.Speech.Fsts.Utils;
//REFACTORED
using Syn.Speech.Helper;

namespace Syn.Speech.Linguist.G2p
{
    /// <summary>
    /// The grapheme-to-phoneme (g2p) decoder
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class G2PConverter
    {
        // epsilon symbol
        private const string Eps = "<eps>";

        // end sequence symbol
        private const string Se = "</s>";

        // begin sequence symbol
        private const string Sb = "<s>";

        // skip symbol
        private const string Skip = "_";

        // separator symbol
        private const string Tie = "|";

        // set containing sequences to ignore
        readonly HashSet<String> _skipSeqs = new HashSet<String>();

        // clusters
        List<List<String>> _clusters;

        // the g2p model
        readonly ImmutableFst _g2Pmodel;

        // fst containing the epsilon filter for the compose operation
        Fsts.Fst _epsilonFilter;

        /**
        /// Create a decoder by loading the serialized model from a specified URL
        /// 
        /// @param g2pModelUrl
        ///            the URL of the serialized model
        /// @throws IOException
        /// @throws ClassNotFoundException 
         */
        public G2PConverter(URL g2PModelUrl)  
        {
            try {
                _g2Pmodel = ImmutableFst.LoadModel(g2PModelUrl.OpenStream());
            } 
            catch (Exception e) 
            {
                throw new IOException("Failed to load the model from " + g2PModelUrl, e);
            }
            Init();
        }

        ///**
        ///// Create a decoder by loading the serialized model from a specified
        ///// filename
        ///// 
        ///// @param g2pmodel_file
        /////            the filename of the serialized model
        // */
        //public G2PConverter(String g2pmodel_file) 
        //{
        //    g2pmodel = ImmutableFst.loadModel(g2pmodel_file);
        //    init();
        //}

        /**
        /// Initialize the decoder
         */
        private void Init() {
            _skipSeqs.Add(Eps);
            _skipSeqs.Add(Sb);
            _skipSeqs.Add(Se);
            _skipSeqs.Add(Skip);
            _skipSeqs.Add("-");
            // keep an augmented copy (for compose)
            Compose.Augment(0, _g2Pmodel, _g2Pmodel.Semiring);
            ArcSort.Apply(_g2Pmodel, new ILabelCompare());

            var isyms = _g2Pmodel.Isyms;

            LoadClusters(isyms);

            // get epsilon filter for composition
            _epsilonFilter = Compose.GetFilter(_g2Pmodel.Isyms,_g2Pmodel.Semiring);
            ArcSort.Apply(_epsilonFilter, new ILabelCompare());
        }

        /**
        /// Phoneticize a word
        /// 
        /// @param entry
        ///            the word to phoneticize transformed to an ArrayList of Strings
        ///            (each element hold a single character)
        /// @param nbest
        ///            the number of distinct pronunciations to return
        /// @return the pronunciation(s) of the input word
         */
        public List<Path> Phoneticize(List<String> entry, int nbest) 
        {
            var efst = EntryToFsa(entry);
            var s = efst.Semiring;
            Compose.Augment(1, efst, s);
            ArcSort.Apply(efst, new OLabelCompare());
            var result = Compose.compose(efst, _epsilonFilter, s, true);
            ArcSort.Apply(result, new OLabelCompare());
            result = Compose.compose(result, _g2Pmodel, s, true);
            Project.Apply(result, ProjectType.Output);
            if (nbest == 1) {
                result = NShortestPaths.Get(result, 1, false);
            } else {
                // Requesting 10 times more best paths than what was asking
                // as there might be several paths resolving to same pronunciation
                // due to epsilon transitions.
                // I really hate cosmological constants :)
                result = NShortestPaths.Get(result, nbest* 10, false);
            }
            // result = NShortestPaths.get(result, nbest, false);
            result = RmEpsilon.Get(result);
            var paths = FindAllPaths(result, nbest, _skipSeqs,Tie);

            return paths;
        }

        /**
        /// Phoneticize a word
        /// 
        /// @param entry
        ///            the word to phoneticize
        /// @param nbest
        ///            the number of distinct pronunciations to return
        /// @return the pronunciation(s) of the input word
         */
        public List<Path> Phoneticize(String word, int nbest) 
        {
            var entry = new List<String>(word.Length);
            for (var i = 0; i < word.Length; i++) {
                var ch = word.Substring(i, i + 1);
                if (Utils.GetIndex(_g2Pmodel.Isyms, ch) >= 0) 
                {
                    entry.Add(ch);
                }
            }
            return Phoneticize(entry, nbest);
        }

        /**
        /// Transforms an input spelling/pronunciation into an equivalent FSA, adding
        /// extra arcs as needed to accommodate clusters.
        /// 
        /// @param entry
        ///            the input vector
        /// @return the created fst
         */
        private Fsts.Fst EntryToFsa(List<String> entry) 
        {
            var ts = new TropicalSemiring();
            var efst = new Fsts.Fst(ts);

            var s = new State(ts.Zero);
            efst.AddState(s);
            efst.SetStart(s);

            // Build the basic FSA
            for (var i = 0; i < entry.Count + 1; i++) {
                s = new State(ts.Zero);
                efst.AddState(s);
                if (i >= 1) {
                    var symIndex = Utils.GetIndex(_g2Pmodel.Isyms,
                            entry[i - 1]);
                    efst.GetState(i).AddArc(new Arc(symIndex, symIndex, 0.0f, s));
                } else if (i == 0) {
                    var symIndex = Utils.GetIndex(_g2Pmodel.Isyms, Sb);
                    efst.Start.AddArc(new Arc(symIndex, symIndex, 0.0f, s));
                }

                if (i == entry.Count) {
                    var s1 = new State(ts.Zero);
                    efst.AddState(s1);
                    var symIndex = Utils.GetIndex(_g2Pmodel.Isyms, Se);
                    s.AddArc(new Arc(symIndex, symIndex, 0.0f, s1));
                    s1.FinalWeight = 0.0f;
                }
            }

            // Add any cluster arcs
            for (var value = 0; value < _clusters.Count; value++) {
                var cluster = _clusters[value];
                if (cluster != null) {
                    var start = 0;
                    var k = 0;
                    while (k != -1) {
                        k = Utils.Search(entry, cluster, start);
                        if (k != -1) {
                            var from = efst.GetState(start + k + 1);
                            from.AddArc(new Arc(value, value, 0.0f, efst
                                    .GetState(start + k + cluster.Count + 1)));
                            start = start + k + cluster.Count;
                        }
                    }
                }
            }

            efst.Isyms = _g2Pmodel.Isyms;
            efst.Osyms = _g2Pmodel.Isyms;

            return efst;
        }

        /**
        /// Finds nbest paths in an Fst returned by NShortestPaths operation
        /// 
        /// @param fst
        ///            the input fst
        /// @param nbest
        ///            the number of paths to return
        /// @param skipSeqs
        ///            the sequences to ignore
        /// @param tie
        ///            the separator symbol
        /// @return the paths
         */
        private List<Path> FindAllPaths(Fsts.Fst fst, int nbest, HashSet<String> skipSeqs, string tie) 
        {
            var semiring = fst.Semiring;

            // ArrayList<Path> finalPaths = new ArrayList<Path>();
            var finalPaths = new Dictionary<String, Path>();
            var paths = new Dictionary<State, Path>();
            var queue = new Queue<State>();
            var p = new Path(fst.Semiring);
            p.Cost = semiring.One;
            paths.Add(fst.Start, p);

            queue.Enqueue(fst.Start);

            var osyms = fst.Osyms;
            while (queue.Count!=0) 
            {
                var s = queue.Dequeue();
                var currentPath = paths[s];

                if (s.FinalWeight != semiring.Zero) {
                    var pathString = currentPath.GetPath().ToString();
                    if (finalPaths.ContainsKey(pathString)) 
                    {
                        // path already exist. update its cost
                        var old = finalPaths[pathString];
                        if (old.Cost > currentPath.Cost) 
                        {
                            finalPaths.Add(pathString, currentPath);
                        }
                    } 
                    else 
                    {
                        finalPaths.Add(pathString, currentPath);
                    }
                }

                var numArcs = s.GetNumArcs();
                for (var j = 0; j < numArcs; j++) 
                {
                    var a = s.GetArc(j);
                    p = new Path(fst.Semiring);
                    var cur = paths[s];
                    p.Cost = cur.Cost;
                    p.SetPath(cur.GetPath().ToList());

                    var sym = osyms[a.Olabel];

                    var symsArray = sym.Split(new String[]{"\\" + tie}, StringSplitOptions.None);

                    for (var i = 0; i < symsArray.Length; i++) 
                    {
                        var phone = symsArray[i];
                        if (!skipSeqs.Contains(phone)) 
                        {
                            p.GetPath().Add(phone);
                        }
                    }
                    p.Cost = semiring.Times(p.Cost, a.Weight);
                    var nextState = a.NextState;
                    paths.Add(nextState, p);
                    if (!queue.Contains(nextState)) 
                    {
                        queue.Enqueue(nextState);
                    }
                }
            }

            var res = new List<Path>();
            foreach (var path in finalPaths.Values) 
            {
                res.Add(path);
            }

            res.Sort(new PathComparator());
            var numPaths = res.Count;
            for (var i = nbest; i < numPaths; i++) 
            {
                res.RemoveAt(res.Count - 1);
            }

            return res;
        }

        /**
        /// Initialize clusters
         */
        private void LoadClusters(String[] syms) 
        {
            _clusters = new List<List<String>>();
            if (syms == null)
                return;
            for (var i = 0; i < syms.Length; i++) 
            {
                if (i < 2)
                {
                    _clusters.Add(null);
                    continue;
                }
                var sym = syms[i];
                if (sym.Contains(Tie)) 
                {
                    var split = sym.Split(new String[]{Tie},StringSplitOptions.None);
                    var clusterString = split.ToList();
                    _clusters.Add(clusterString);
                }
            }
        }
    }
}
