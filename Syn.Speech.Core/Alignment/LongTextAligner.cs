using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Helper;
using Syn.Speech.Util;
using Math = System.Math;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    public class LongTextAligner
    {
        private readonly int _tupleSize;
        internal List<string> Reftup;
        internal HashMap<string, List<Integer>> TupleIndex;
        private readonly List<string> _refWords;

        public LongTextAligner(List<string> words, int tupleSize)
        {
            Debug.Assert(words!=null);
            Debug.Assert(tupleSize>0);

            _tupleSize = tupleSize;
            _refWords = words;
            int offset = 0;
            Reftup = GetTuples(words);
            TupleIndex = new HashMap<string, List<Integer>>();

            foreach (string tuple in Reftup)
            {
                var indexes = TupleIndex.Get(tuple);
                if (indexes == null)
                {
                    indexes = new List<Integer>();
                    TupleIndex.Put(tuple, indexes);
                }
                indexes.Add(offset++);
            }
        }

        private List<string> GetTuples(List<string> words)
        {
            var result = new List<string>();
            var tuple = new LinkedList<string>();

            //TODO: CHECK SEMATICS
            var enumerator = words.GetEnumerator();
            for (int i = 0; i < _tupleSize - 1; i++)
            {
                enumerator.MoveNext();
                tuple.AddLast(enumerator.Current);
            }
            while (enumerator.MoveNext())
            {
                tuple.AddLast(enumerator.Current);
                result.Add(Utilities.Join(new List<string>(tuple)));
                tuple.RemoveFirst();
            }
            return result;
        }

        public virtual int[] Align(List<string> words, Range range)
        {
            if (range.upperEndpoint() - range.lowerEndpoint() < _tupleSize || words.Count < _tupleSize)
            {
                return AlignTextSimple(_refWords.GetRange(range.lowerEndpoint(), range.upperEndpoint()), words, range.lowerEndpoint());
            }

            var result = new int[words.Count];
            Arrays.Fill(result,-1);
            //Arrays.fill(result, -1);
            int lastIndex = 0;

            foreach (Node node in new Alignment(this, GetTuples(words), range).GetIndices())
            {
                // for (int j = 0; j < tupleSize; ++j)
                lastIndex = Math.Max(lastIndex, node.GetQueryIndex());
                for (; lastIndex < node.GetQueryIndex() + _tupleSize; ++lastIndex)
                {
                    result[lastIndex] = node.GetDatabaseIndex() + lastIndex - node.GetQueryIndex();
                }
            }
            return result;
        }

        internal static int[] AlignTextSimple(List<string> database, List<string> query, int offset)
        {

            int n = database.Count + 1;
            int m = query.Count + 1;
            int[][] f = Java.CreateArray<int[][]>(n, m);//  new int[n][];

            f[0][0] = 0;
            for (int i = 1; i < n; ++i)
            {
                f[i][0] = i;
            }

            for (int j = 1; j < m; ++j)
            {
                f[0][j] = j;
            }

            for (int i = 1; i < n; ++i)
            {
                for (int j = 1; j < m; ++j)
                {
                    int match = f[i - 1][j - 1];
                    var refWord = database[i - 1];
                    var queryWord = query[j - 1];
                    if (!refWord.Equals(queryWord))
                    {
                        ++match;
                    }
                    int insert = f[i][j - 1] + 1;
                    int delete = f[i - 1][j] + 1;
                    f[i][j] = Math.Min(match, Math.Min(insert, delete));
                }
            }

            --n;
            --m;
            int[] alignment = new int[m];
            Arrays.Fill(alignment,-1);
            while (m > 0)
            {
                if (n == 0)
                {
                    --m;
                }
                else
                {
                    var refWord = database[n - 1];
                    var queryWord = query[m - 1];
                    if (f[n - 1][m - 1] <= f[n - 1][m - 1] && f[n - 1][m - 1] <= f[n][m - 1] && refWord.Equals(queryWord))
                    {
                        alignment[--m] = --n + offset;
                    }
                    else
                    {
                        if (f[n - 1][m] < f[n][m - 1])
                        {
                            --n;
                        }
                        else
                        {
                            --m;
                        }
                    }
                }
            }

            return alignment;
        }

        public virtual int[] Align(List<string> query)
        {
            return Align(query, new Range(0, _refWords.Count));
        }
    }
}
