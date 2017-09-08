using System.Collections.Generic;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Comparer
{
    public class NodeComparer : IComparer<Node>
    {
        private readonly HashMap<Node, Integer> _cost;
        public NodeComparer(HashMap<Node, Integer> cost)
        {
            _cost = cost;
        }

        public int Compare(Node o1, Node o2)
        {
            return _cost.Get(o1).CompareTo(_cost.Get(o2));
        }
    }
}
