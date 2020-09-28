using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Syn.Speech.Trainer
{
    /// <summary>
    /// Defines the basic Node for any graph A generic graph Node must have a list of outgoing edges and an identifier.
    /// </summary>
    public class Node
    {
        // Do we really need nodeId and object? Maybe we can use object as
        // the id when we assign a string to it.
        /// <summary>
        /// The identifier for this Node 
        /// </summary>
        private readonly string nodeId;
        
        /// <summary>
        /// Object contained in this mode. Typically, an HMM state, a senone. 
        /// </summary>
        private Object _object;

        /** The type of node, such as a dummy node or node represented by a specific type of symbol */
        private readonly NodeType nodeType;

        /** The list of incoming edges to this node. */
        private readonly List<Edge> incomingEdges;
        private Edge incomingEdgeIterator;

        /** The list of outgoing edges from this node */
        private readonly List<Edge> outgoingEdges;
        private Edge outgoingEdgeIterator;


        /**
        /// Constructor for node when a type and symbol are given.
         *
        /// @param nodeType   the type of node.
        /// @param nodeSymbol the symbol for this type.
         */
        Node(NodeType nodeType, string nodeSymbol) 
        {
            incomingEdges = new List<Edge>();
            outgoingEdges = new List<Edge>();
            nodeId = nodeSymbol;
            this.nodeType = nodeType;
            _object = null;
        }


        /**
        /// Constructor for node when a type only is given.
         *
        /// @param nodeType the type of node.
         */
        Node(NodeType nodeType) :this(nodeType, null)
        {
            
        }


        /**
        /// Assign an object to this node.
         *
        /// @param object the object to assign
         */
        public void setObject(Object pobject) 
        {
            _object = pobject;
        }


        /**
        /// Retrieves the object associated with this node.
         *
        /// @return the object
         */
        public Object getObject() 
        {
            return _object;
        }


        /**
        /// Method to add an incoming edge. Note that we do not check if the destination node of the incoming edge is
        /// identical to this node
         *
        /// @param edge the incoming edge
         */
        public void addIncomingEdge(Edge edge) 
        {
            incomingEdges.Add(edge);
        }


        /** Start iterator for incoming edges. */
        public void startIncomingEdgeIterator() 
        {
            incomingEdgeIterator = incomingEdges.First();
        }


        /**
        /// Whether there are more incoming edges.
         *
        /// @return if true, there are more incoming edges
         */
        public Boolean hasMoreIncomingEdges() 
        {
            return incomingEdges.IndexOf(incomingEdgeIterator) < incomingEdges.Count;
        }


        /**
        /// Returns the next incoming edge to this node.
         *
        /// @return the next edge incoming edge
         */
        public Edge nextIncomingEdge() 
        {
            if (!hasMoreIncomingEdges())
                return incomingEdgeIterator;
            incomingEdgeIterator = incomingEdges[incomingEdges.IndexOf(incomingEdgeIterator) +1];
            return incomingEdgeIterator;
        }


        /**
        /// Returns the size of the incoming edges list.
         *
        /// @return the number of incoming edges
         */
        public int incomingEdgesSize() 
        {
            return incomingEdges.Count;
        }


        /**
        /// Method to add an outgoing edge. Note that we do not check if the source node of the outgoing edge is identical to
        /// this node
         *
        /// @param edge the outgoing edge
         */
        public void addOutgoingEdge(Edge edge) 
        {
            outgoingEdges.Add(edge);
        }


        /** Start iterator for outgoing edges. */
        public void startOutgoingEdgeIterator() 
        {
            outgoingEdgeIterator = outgoingEdges.First();
        }


        /**
        /// Whether there are more outgoing edges.
         *
        /// @return if true, there are more outgoing edges
         */
        public Boolean hasMoreOutgoingEdges() 
        {
            return outgoingEdges.IndexOf(outgoingEdgeIterator) < outgoingEdges.Count;
        }


        /**
        /// Returns the next outgoing edge from this node.
         *
        /// @return the next outgoing edge
         */
        public Edge nextOutgoingEdge() 
        {
            if (!hasMoreOutgoingEdges())
                return outgoingEdgeIterator;
            outgoingEdgeIterator = outgoingEdges[outgoingEdges.IndexOf(outgoingEdgeIterator) + 1];
            return outgoingEdgeIterator;
        }


        /**
        /// Returns the size of the outgoing edges list.
         *
        /// @return the number of outgoing edges
         */
        public int outgoingEdgesSize() 
        {
            return outgoingEdges.Count;
        }


        /**
        /// Method to check the type of a node.
         *
        /// @return if true, this node is of the type specified
         */
        public Boolean isType(String type) 
        {
            return (type.Equals(nodeType.ToString()));
        }


        /**
        /// Returns type of a node.
         *
        /// @return returns the type of this node
         */
        public NodeType getType() 
        {
            return nodeType;
        }


        /**
        /// Returns the ID of a node. Typically, a string representing a word or a phoneme.
         *
        /// @return this node's ID
         */
        public string getID() 
        {
            return nodeId;
        }


        /**
        /// Validade node. Checks if all nodes have at least one incoming and one outgoing edge.
         *
        /// @return if true, node passed validation
         */
        public Boolean validate() 
        {
            Boolean passed = true;

            if (isType("WORD") || isType("PHONE")) {
                if (nodeId == null) 
                {
                    Debug.Print("Content null in a WORD node.");
                    passed = false;
                }
            }
            if ((incomingEdgesSize() == 0) && (outgoingEdgesSize() == 0)) 
            {
                Debug.Print("Node not connected anywhere.");
                passed = false;
            }
            return passed;
        }


        /** Prints out this node. */
        public void print() 
        {
            Debug.Print("ID: " + nodeId);
            Debug.Print(" Type: " + nodeType + " | ");
            for (startIncomingEdgeIterator();
                 hasMoreIncomingEdges();) {
                Debug.Print(nextIncomingEdge() + " ");
            }
            Debug.Print(" | ");
            for (startOutgoingEdgeIterator();
                 hasMoreOutgoingEdges();) {
                Debug.Print(nextOutgoingEdge() + " ");
            }
            Debug.Print("\n");
        }
    }
}
