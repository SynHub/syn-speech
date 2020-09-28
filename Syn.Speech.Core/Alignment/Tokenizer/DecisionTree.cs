using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment.Tokenizer
{
    public class DecisionTree
    {
        internal const string Total = "TOTAL";
        internal const string NodeConst = "NODE";
        internal const string Leaf = "LEAF";
        internal const string OperandMatches = "MATCHES";
        internal Node[] Cart;

        [NonSerialized]
        internal int CurNode;

        public DecisionTree(URL fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.OpenStream()))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("***"))
                    {
                        ParseAndAdd(line);
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
            }
        }

        //EXTRA
        public DecisionTree(string stringValue)
        {
            using (var reader = new StringReader(stringValue))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("***"))
                    {
                        ParseAndAdd(line);
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Creates a new CART by reading from the given reader.
        /// </summary>
        /// <param name="reader">the source of the CART data.</param>
        /// <param name="nodes">the number of nodes to read for this cart.</param>
        public DecisionTree(StreamReader reader, int nodes): this(nodes)
        {
            for (var i = 0; i < nodes; i++)
            {
                var line = reader.ReadLine();
                if (line != null && !line.StartsWith("***"))
                {
                    ParseAndAdd(line);
                }
            }
        }

        private DecisionTree(int numNodes)
        {
            Cart = new Node[numNodes];
        }

        public virtual void DumpDot(TextWriter printWriter)
        {
            printWriter.Write("digraph \"CART Tree\" {\n");
            printWriter.Write("rankdir = LR\n");

            foreach (var n in Cart)
            {
                printWriter.WriteLine("\tnode" + Math.Abs(n.GetHashCode()) + " [ label=\""
                        + n + "\", color=" + DumpDotNodeColor(n)
                        + ", shape=" + DumpDotNodeShape(n) + " ]\n");
                if (n is DecisionNode)
                {
                    var dn = (DecisionNode)n;
                    if (dn.Qtrue < Cart.Length && Cart[dn.Qtrue] != null)
                    {
                        printWriter.Write("\tnode" + Math.Abs(n.GetHashCode()) + " -> node"
                                + Math.Abs(Cart[dn.Qtrue].GetHashCode())
                                + " [ label=" + "TRUE" + " ]\n");
                    }
                    if (dn.Qfalse < Cart.Length && Cart[dn.Qfalse] != null)
                    {
                        printWriter.Write("\tnode" + Math.Abs(n.GetHashCode()) + " -> node"
                                + Math.Abs(Cart[dn.Qfalse].GetHashCode())
                                + " [ label=" + "FALSE" + " ]\n");
                    }
                }
            }

            printWriter.Write("}\n");
            printWriter.Close();
        }

        internal virtual string DumpDotNodeColor(Node n)
        {
            return n is LeafNode ? "green" : "red";
        }

        internal virtual string DumpDotNodeShape(Node n)
        {
            return "box";
        }

        /// <summary>
        /// Creates a node from the given input line and add it to the CART. 
        /// It expects the TOTAL line to come before any of the nodes.
        /// </summary>
        /// <param name="line">The line of input to parse.</param>
        /// <exception cref="Error"></exception>
        protected internal void ParseAndAdd(string line)
        {
            var tokenizer = new StringTokenizer(line, " ");
            var type = tokenizer.nextToken();
            if (type.Equals(Leaf) || type.Equals(NodeConst))
            {
                Cart[CurNode] = GetNode(type, tokenizer, CurNode);
                Cart[CurNode].SetCreationLine(line);
                CurNode++;
            }
            else if (type.Equals(Total))
            {
                Cart = new Node[int.Parse(tokenizer.nextToken(), CultureInfo.InvariantCulture.NumberFormat)];
                CurNode = 0;
            }
            else
            {
                throw new Error("Invalid CART type: " + type);
            }
        }

        /// <summary>
        /// Gets the node based upon the type and tokenizer.
        /// </summary>
        /// <param name="type"><code>NODE</code> or <code>LEAF</code></param>
        /// <param name="tokenizer">The StringTokenizer containing the data to get.</param>
        /// <param name="currentNode">The index of the current node we're looking at.</param>
        /// <returns>The node</returns>
        internal virtual Node GetNode(string type, StringTokenizer tokenizer, int currentNode)
        {
            if (type.Equals(NodeConst))
            {
                var feature = tokenizer.nextToken();
                var operand = tokenizer.nextToken();
                var value = ParseValue(tokenizer.nextToken());
                //int qfalse = Integer.parseInt(tokenizer.nextToken());
                var qfalse = int.Parse(tokenizer.nextToken(), CultureInfo.InvariantCulture.NumberFormat);
                if (operand.Equals(OperandMatches))
                {
                    return new MatchingNode(feature, value.ToString(), currentNode + 1, qfalse);
                }
                return new ComparisonNode(feature, value, operand,currentNode + 1, qfalse);
            }
            if (type.Equals(Leaf))
            {
                return new LeafNode(ParseValue(tokenizer.nextToken()));
            }

            return null;
        }

        /// <summary>
        /// Coerces a string into a value.
        /// </summary>
        /// <param name="_string">of the form "type(value)"; for example, "Float(2.3)"</param>
        /// <returns>The value.</returns>
        protected internal virtual object ParseValue(string _string)
        {
            var openParen = _string.IndexOf("(", StringComparison.Ordinal);
            var type = _string.Substring(0, openParen);
            var value = _string.JSubString(openParen + 1, _string.Length - 1);
            if (type.Equals("String"))
            {
                return value;
            }
            if (type.Equals("Float"))
            {
                //return new Float(Float.parseFloat(value));
                return float.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            if (type.Equals("Integer"))
            {
                //return new Integer(Integer.parseInt(value));
                return int.Parse(value, CultureInfo.InvariantCulture.NumberFormat);
            }
            if (type.Equals("List"))
            {

                var tok = new StringTokenizer(value, ",");
                var size = tok.countTokens();

                var values = new int[size];
                for (var i = 0; i < size; i++)
                {
                    //float fval = Float.parseFloat(tok.nextToken());
                    var fval = float.Parse(tok[i], CultureInfo.InvariantCulture.NumberFormat);
                    values[i] = (int)Math.Round(fval);
                }
                return values;
            }
            throw new Error("Unknown type: " + type);
        }

        /// <summary>
        /// Passes the given item through this CART and returns the interpretation.
        /// </summary>
        /// <param name="item">The item to analyze</param>
        /// <returns>The interpretation.</returns>
        public virtual object Interpret(Item item)
        {
            var nodeIndex = 0;
            while (!(Cart[nodeIndex] is LeafNode))
            {
                var decision = (DecisionNode)Cart[nodeIndex];
                nodeIndex = decision.GetNextNode(item);
            }
            this.LogInfo("LEAF " + Cart[nodeIndex].Value);
            return Cart[nodeIndex].Value;
        }

        internal class Node
        {
            public Node(object value)
            {
                Value = value;
            }

            public object Value { get; protected internal set; }

            public string GetValueString()
            {
                if (Value == null)
                {
                    return "NULL()";
                }
                if (Value is string)
                {
                    return "String(" + Value + ")";
                }
                if (Value is float)
                {
                    return "Float(" + Value + ")";
                }
                if (Value is int)
                {
                    return "Integer(" + Value + ")";
                }
                return Value.GetType() + "(" + Value
                       + ")";
            }

            public void SetCreationLine([In] string obj0)
            {
            }
        }

        internal abstract class DecisionNode : Node
        {
            private readonly PathExtractor _path;
            protected internal int Qfalse;
            protected internal int Qtrue;

            public virtual string GetFeature()
            {
                return _path.ToString();
            }

            public virtual object FindFeature([In] Item obj0)
            {
                return _path.FindFeature(obj0);
            }

            public int GetNextNode(Item item)
            {
                return GetNextNode(FindFeature(item));
            }


            protected DecisionNode(string feature, object value, int qtrue, int qfalse)
                : base(value)
            {
                _path = new PathExtractor(feature, true);
                Qtrue = qtrue;
                Qfalse = qfalse;
            }


            public abstract int GetNextNode([In] object objectValue);

        }

        internal class ComparisonNode : DecisionNode
        {
            internal const string LessThan = "<";
            internal const string EqualsString = "=";
            internal const string GreaterThan = ">";
            /// <summary>
            /// The comparison type. One of LESS_THAN, GREATER_THAN, or EQUAL_TO.
            /// </summary>
            internal string ComparisonType;


            public ComparisonNode(string feature, object value, string comparisonType, int qtrue, int qfalse)
                : base(feature, value, qtrue, qfalse)
            {
                if (!comparisonType.Equals(LessThan)
                           && !comparisonType.Equals(EqualsString)
                           && !comparisonType.Equals(GreaterThan))
                {
                    throw new Error("Invalid comparison type: " + comparisonType);
                }
                ComparisonType = comparisonType;
            }

            public override int GetNextNode(object objectValue)
            {
                bool yes;
                int ret;

                if (ComparisonType.Equals(LessThan)
                        || ComparisonType.Equals(GreaterThan))
                {
                    float cartFval;
                    float fval;
                    if (Value is float)
                    {
                        cartFval = ((float)Value);
                    }
                    else
                    {
                        cartFval = float.Parse(Value.ToString(),CultureInfo.InvariantCulture.NumberFormat);
                    }
                    if (objectValue is float)
                    {
                        fval = ((float)objectValue);
                    }
                    else
                    {
                        fval = float.Parse(objectValue.ToString(), CultureInfo.InvariantCulture.NumberFormat);
                    }
                    if (ComparisonType.Equals(LessThan))
                    {
                        yes = (fval < cartFval);
                    }
                    else
                    {
                        yes = (fval > cartFval);
                    }
                }
                else
                { // comparisonType = "="
                    var sval = objectValue.ToString();
                    var cartSval = Value.ToString();
                    yes = sval.Equals(cartSval);
                }
                if (yes)
                {
                    ret = Qtrue;
                }
                else
                {
                    ret = Qfalse;
                }
                this.LogInfo(Trace(objectValue, yes, ret));
                return ret;
            }

            private string Trace(object objectValue, bool match, int next)
            {
                return "NODE " + GetFeature() + " [" + objectValue + "] "
                            + ComparisonType + " [" + Value + "] "
                            + (match ? "Yes" : "No") + " next " + next;
            }

            public override string ToString()
            {
                return "NODE " + GetFeature() + " " + ComparisonType + " "
                             + GetValueString() + " " + Qtrue + " "
                             + Qfalse;
            }
        }

        internal class MatchingNode : DecisionNode
        {
            internal Pattern Pattern;

            public MatchingNode(string feature, string regex, int qtrue, int qfalse)
                : base(feature, regex, qtrue, qfalse)
            {
                Pattern = Pattern.Compile(regex);
            }

            public override int GetNextNode(object objectValue)
            {
                return Pattern.Matcher((string)objectValue).Matches() ? Qtrue : Qfalse;
            }

            public override string ToString()
            {
                var buf = new StringBuilder(NodeConst + " " + GetFeature() + " " + OperandMatches);
                buf.Append(GetValueString() + " ");
                buf.Append(Qtrue + " ");
                buf.Append(Qfalse);
                return buf.ToString();
            }
        }

        internal class LeafNode : Node
        {
            public LeafNode(object value)
                : base(value)
            {
            }

            public override string ToString()
            {
                return "LEAF " + GetValueString();
            }
        }
    }
}
