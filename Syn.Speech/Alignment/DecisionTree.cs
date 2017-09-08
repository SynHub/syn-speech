using System;
using System.IO;
using System.Runtime.InteropServices;
//PATROLLED
using System.Text;
using Syn.Speech.Helper;

namespace Syn.Speech.Alignment
{
    public class DecisionTree
    {
        internal const string TOTAL = "TOTAL";
        internal const string NODE = "NODE";
        internal const string LEAF = "LEAF";
        internal const string OPERAND_MATCHES = "MATCHES";
        internal Node[] cart;

        [NonSerialized]
        internal int curNode;

        public DecisionTree(FileInfo fileInfo)
        {
            using (var reader = new StreamReader(fileInfo.OpenRead()))
            {
                string line;
                line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("***"))
                    {
                        parseAndAdd(line);
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
                string line;
                line = reader.ReadLine();
                while (line != null)
                {
                    if (!line.StartsWith("***"))
                    {
                        parseAndAdd(line);
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
            for (int i = 0; i < nodes; i++)
            {
                var line = reader.ReadLine();
                if (!line.StartsWith("***"))
                {
                    parseAndAdd(line);
                }
            }
        }

        private DecisionTree(int numNodes)
        {
            cart = new Node[numNodes];
        }

        public virtual void dumpDot(TextWriter printWriter)
        {
            printWriter.Write("digraph \"CART Tree\" {\n");
            printWriter.Write("rankdir = LR\n");

            foreach (Node n in cart)
            {
                printWriter.WriteLine("\tnode" + Math.Abs(n.GetHashCode()) + " [ label=\""
                        + n + "\", color=" + dumpDotNodeColor(n)
                        + ", shape=" + dumpDotNodeShape(n) + " ]\n");
                if (n is DecisionNode)
                {
                    DecisionNode dn = (DecisionNode)n;
                    if (dn.qtrue < cart.Length && cart[dn.qtrue] != null)
                    {
                        printWriter.Write("\tnode" + Math.Abs(n.GetHashCode()) + " -> node"
                                + Math.Abs(cart[dn.qtrue].GetHashCode())
                                + " [ label=" + "TRUE" + " ]\n");
                    }
                    if (dn.qfalse < cart.Length && cart[dn.qfalse] != null)
                    {
                        printWriter.Write("\tnode" + Math.Abs(n.GetHashCode()) + " -> node"
                                + Math.Abs(cart[dn.qfalse].GetHashCode())
                                + " [ label=" + "FALSE" + " ]\n");
                    }
                }
            }

            printWriter.Write("}\n");
            printWriter.Close();
        }

        internal virtual string dumpDotNodeColor(Node n)
        {
            return n is LeafNode ? "green" : "red";
        }

        internal virtual string dumpDotNodeShape(Node n)
        {
            return "box";
        }

        /// <summary>
        /// Creates a node from the given input line and add it to the CART. 
        /// It expects the TOTAL line to come before any of the nodes.
        /// </summary>
        /// <param name="line">The line of input to parse.</param>
        /// <exception cref="Error"></exception>
        protected internal virtual void parseAndAdd(string line)
        {
            var tokenizer = new StringTokenizer(line, " ");
            string type = tokenizer.nextToken();
            if (type.Equals(LEAF) || type.Equals(NODE))
            {
                cart[curNode] = getNode(type, tokenizer, curNode);
                cart[curNode].setCreationLine(line);
                curNode++;
            }
            else if (type.Equals(TOTAL))
            {
                cart = new Node[int.Parse(tokenizer.nextToken())];
                curNode = 0;
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
        internal virtual Node getNode(string type, StringTokenizer tokenizer, int currentNode)
        {
            if (type.Equals(NODE))
            {
                string feature = tokenizer.nextToken();
                string operand = tokenizer.nextToken();
                object value = parseValue(tokenizer.nextToken());
                //int qfalse = Integer.parseInt(tokenizer.nextToken());
                int qfalse = int.Parse(tokenizer.nextToken());
                if (operand.Equals(OPERAND_MATCHES))
                {
                    return new MatchingNode(feature, value.ToString(),
                            currentNode + 1, qfalse);
                }
                else
                {
                    return new ComparisonNode(feature, value, operand,
                            currentNode + 1, qfalse);
                }
            }
            else if (type.Equals(LEAF))
            {
                return new LeafNode(parseValue(tokenizer.nextToken()));
            }

            return null;
        }

        /// <summary>
        /// Coerces a string into a value.
        /// </summary>
        /// <param name="_string">of the form "type(value)"; for example, "Float(2.3)"</param>
        /// <returns>The value.</returns>
        protected internal virtual object parseValue(string _string)
        {
            int openParen = _string.IndexOf("(");
            string type = _string.Substring(0, openParen);
            string value = _string.Substring(openParen + 1, _string.Length - 1);
            if (type.Equals("String"))
            {
                return value;
            }
            else if (type.Equals("Float"))
            {
                //return new Float(Float.parseFloat(value));
                return float.Parse(value);
            }
            else if (type.Equals("Integer"))
            {
                //return new Integer(Integer.parseInt(value));
                return int.Parse(value);
            }
            else if (type.Equals("List"))
            {

                var tok = new StringTokenizer(value, ",");
                int size = tok.countTokens();

                int[] values = new int[size];
                for (int i = 0; i < size; i++)
                {
                    //float fval = Float.parseFloat(tok.nextToken());
                    float fval = float.Parse(tok[i]);
                    values[i] = (int)Math.Round(fval);
                }
                return values;
            }
            else
            {
                throw new Error("Unknown type: " + type);
            }
        }

        /// <summary>
        /// Passes the given item through this CART and returns the interpretation.
        /// </summary>
        /// <param name="item">The item to analyze</param>
        /// <returns>The interpretation.</returns>
        public virtual object interpret(Item item)
        {
            int nodeIndex = 0;
            DecisionNode decision;

            while (!(cart[nodeIndex] is LeafNode))
            {
                decision = (DecisionNode)cart[nodeIndex];
                nodeIndex = decision.getNextNode(item);
            }
            this.LoggerInfo("LEAF " + cart[nodeIndex].getValue());
            return cart[nodeIndex].getValue();
        }

        internal abstract class Node
        {
            protected internal object value;

            public Node(object value)
            {
                this.value = value;
            }

            public virtual object getValue()
            {
                return value;
            }

            public virtual string getValueString()
            {

                if (value == null)
                {
                    return "NULL()";
                }
                else if (value is string)
                {
                    return "String(" + value + ")";
                }
                else if (value is float)
                {
                    return "Float(" + value + ")";
                }
                else if (value is int)
                {
                    return "Integer(" + value + ")";
                }
                else
                {
                    return value.GetType() + "(" + value
                            + ")";
                }
            }

            public virtual void setCreationLine([In] string obj0)
            {
            }
        }

        internal abstract class DecisionNode : Node
        {
            private readonly PathExtractor path;
            protected internal int qfalse;
            protected internal int qtrue;

            public virtual string getFeature()
            {
                return path.ToString();
            }

            public virtual object findFeature([In] Item obj0)
            {
                return path.findFeature(obj0);
            }

            public int getNextNode(Item item)
            {
                return getNextNode(findFeature(item));
            }


            protected DecisionNode(string feature, object value, int qtrue, int qfalse)
                : base(value)
            {
                path = new PathExtractor(feature, true);
                this.qtrue = qtrue;
                this.qfalse = qfalse;
            }


            public abstract int getNextNode([In] object objectValue);

        }

        internal class ComparisonNode : DecisionNode
        {
            internal const string LESS_THAN = "<";
            internal const string EQUALS = "=";
            internal const string GREATER_THAN = ">";
            /// <summary>
            /// The comparison type. One of LESS_THAN, GREATER_THAN, or EQUAL_TO.
            /// </summary>
            internal string comparisonType;


            public ComparisonNode(string feature, object value, string comparisonType, int qtrue, int qfalse)
                : base(feature, value, qtrue, qfalse)
            {
                if (!comparisonType.Equals(LESS_THAN)
                           && !comparisonType.Equals(EQUALS)
                           && !comparisonType.Equals(GREATER_THAN))
                {
                    throw new Error("Invalid comparison type: " + comparisonType);
                }
                else
                {
                    this.comparisonType = comparisonType;
                }
            }

            public override int getNextNode(object objectValue)
            {
                bool yes = false;
                int ret;

                if (comparisonType.Equals(LESS_THAN)
                        || comparisonType.Equals(GREATER_THAN))
                {
                    float cart_fval;
                    float fval;
                    if (value is float)
                    {
                        cart_fval = ((float)value);
                    }
                    else
                    {
                        cart_fval = float.Parse(value.ToString());
                    }
                    if (objectValue is float)
                    {
                        fval = ((float)objectValue);
                    }
                    else
                    {
                        fval = float.Parse(objectValue.ToString());
                    }
                    if (comparisonType.Equals(LESS_THAN))
                    {
                        yes = (fval < cart_fval);
                    }
                    else
                    {
                        yes = (fval > cart_fval);
                    }
                }
                else
                { // comparisonType = "="
                    string sval = objectValue.ToString();
                    string cart_sval = value.ToString();
                    yes = sval.Equals(cart_sval);
                }
                if (yes)
                {
                    ret = qtrue;
                }
                else
                {
                    ret = qfalse;
                }
                this.LoggerInfo(trace(objectValue, yes, ret));
                return ret;
            }

            private string trace(object objectValue, bool match, int next)
            {
                return "NODE " + getFeature() + " [" + objectValue + "] "
                            + comparisonType + " [" + getValue() + "] "
                            + (match ? "Yes" : "No") + " next " + next;
            }

            public override string ToString()
            {
                return "NODE " + getFeature() + " " + comparisonType + " "
                             + getValueString() + " " + qtrue + " "
                             + qfalse;
            }
        }

        internal class MatchingNode : DecisionNode
        {
            internal Pattern pattern;

            public MatchingNode(string feature, string regex, int qtrue, int qfalse)
                : base(feature, regex, qtrue, qfalse)
            {
                pattern = Pattern.Compile(regex);
            }

            public override int getNextNode(object objectValue)
            {
                return pattern.Matcher((string)objectValue).Matches() ? qtrue : qfalse;
            }

            public override string ToString()
            {
                var buf = new StringBuilder(NODE + " " + getFeature() + " " + OPERAND_MATCHES);
                buf.Append(getValueString() + " ");
                buf.Append(qtrue + " ");
                buf.Append(qfalse);
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
                return "LEAF " + getValueString();
            }
        }
    }
}
