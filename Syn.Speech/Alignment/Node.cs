using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    public sealed class Node
    {
        internal readonly int DatabaseIndex; //Field is accessed by Alignment
        internal readonly int QueryIndex; //Field is accessed by Alignment

        //ADDED: To link with Alignment Class
        private readonly Alignment _alignment;
        private readonly LongTextAligner _longTextAligner;
        private List<Integer> Shifts { get { return _alignment.Shifts; } }
        private List<Integer> Indices { get { return _alignment.Indices; } }
        private List<string> Query { get { return _alignment.Query; } }
        private List<string> Reftup { get { return _longTextAligner.Reftup; } }

        private static int i = 0;
        internal Node(LongTextAligner aligner, Alignment alignment, int row, int column)
        {
            _longTextAligner = aligner;
  
            _alignment = alignment;

            DatabaseIndex = column;
            QueryIndex = row;
            //Trace.WriteLine(this + " : " + i);
            //i++; //Todo: Remove
        }

        public int GetDatabaseIndex()
        {
            return Shifts[DatabaseIndex - 1];
        }

        public int GetQueryIndex()
        {
            return Indices[QueryIndex - 1];
        }

        public string QueryWord
        {
            get
            {
                if (QueryIndex > 0) return Query[GetQueryIndex()];
                return null;
            }
        }

        public string DatabaseWord
        {
            get
            {
                if (DatabaseIndex > 0) return Reftup[GetDatabaseIndex()];
                return null;
            }
        }

        public int GetValue()
        {
            if (IsBoundary) return Math.Max(QueryIndex, DatabaseIndex);
            return HasMatch ? 0 : 1;
        }

        public bool HasMatch
        {
            get { return QueryWord.Equals(DatabaseWord); }
        }

        public bool IsBoundary
        {
            get { return QueryIndex == 0 || DatabaseIndex == 0; }
        }

        public bool IsTarget
        {
            get
            {
                return QueryIndex == Indices.Count && DatabaseIndex == Shifts.Count;
            }
        }

        public List<Node> Adjacent()
        {
            var result = new List<Node>(3);
            if (QueryIndex < Indices.Count && DatabaseIndex < Shifts.Count)
            {
                result.Add(new Node(_longTextAligner, _alignment, QueryIndex + 1, DatabaseIndex + 1));
            }
            if (DatabaseIndex < Shifts.Count)
            {
                result.Add(new Node(_longTextAligner, _alignment, QueryIndex, DatabaseIndex + 1));
            }
            if (QueryIndex < Indices.Count)
            {
                result.Add(new Node(_longTextAligner, _alignment, QueryIndex + 1, DatabaseIndex));
            }

            return result;
        }

        public override bool Equals(Object _object)
        {
            if (!(_object is Node)) return false;

            var other = (Node)_object;
            return QueryIndex == other.QueryIndex && DatabaseIndex == other.DatabaseIndex;
        }

        public override int GetHashCode()
        {
            return 31 * (31 * QueryIndex + DatabaseIndex);
        }


        public override string ToString()
        {
            return string.Format("[{0} {1}]", QueryIndex, DatabaseIndex);
        }
    }
}
