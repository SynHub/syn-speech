using System;
using System.Diagnostics;

namespace Syn.Speech.Trainer
{
    /// <summary>
    /// Defines the basic Edge for any graph A generic graph edge must have a destination Node and an identifier.
    /// </summary>
    public class Edge
    {
        /** The identifier for this edge */
        public string id;

        /** The source node for this edge */
        public Node sourceNode;

        /** The destination node for this edge */
        public Node destinationNode;


        /*
       /// Default Constructor
        */
        Edge(Node source, Node destination, string id)
        {
            sourceNode = source;
            destinationNode = destination;
            this.id = id;
        }


        /*
       /// Constructor given no id.
        */
        Edge(Node source, Node destination)
            : this(source, destination, null)
        {
            
        }


        /**
        /// Sets the destination node for a given edge.
         *
        /// @param node the destination node for this edge
        /// @see #getDestination
         */
        public void setDestination(Node node)
        {
            destinationNode = node;
        }


        /**
        /// Sets source node for a given edge.
         *
        /// @param node the source node for this edge
        /// @see #getSource
         */
        public void setSource(Node node)
        {
            sourceNode = node;
        }


        /**
        /// Gets the destination node for a given edge.
         *
        /// @return the destination node
        /// @see #setDestination
         */
        public Node getDestination()
        {
            return destinationNode;
        }


        /**
        /// Gets source node for a given edge.
         *
        /// @return the source node
        /// @see #setSource
         */
        public Node getSource()
        {
            return sourceNode;
        }


        /**
        /// Validate this edge. Checks if source and destination are non-null.
         *
        /// @return if true, edge passed validation
         */
        public Boolean validate()
        {
            return ((sourceNode != null) && (destinationNode != null));
        }


        /** Prints out this edge. */
        public void print() 
        {
            Debug.Print("ID: " + id);
            Debug.Print(" | " + sourceNode);
            Debug.Print(" | " + destinationNode);
        }
    }
}
