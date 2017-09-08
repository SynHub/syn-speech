using System;

namespace Syn.Speech.Trainer
{
    /// <summary>
    /// Indicates node types such as beginning, end, containing word etc.
    /// </summary>
    public class NodeType
    {
         private readonly string name;

        /** NodeType to indicate dummy node. */
        public static NodeType DUMMY = new NodeType("DUMMY");

        /** NodeType to indicate node containing silence with loopback. */
        public static NodeType SILENCE_WITH_LOOPBACK =
                new NodeType("SILENCE_WITH_LOOPBACK");

        /** NodeType to indicate the end of a speech utterance. */
        public static NodeType UTTERANCE_END = new NodeType("UTTERANCE_END");

        /** NodeType to indicate the start of am utterance. */
        public static NodeType UTTERANCE_BEGIN =
                new NodeType("UTTERANCE_BEGIN");

        /** NodeType to indicate the node contains a word. */
        public static NodeType WORD = new NodeType("WORD");

        /** NodeType to indicate the node contains a word. */
        public static NodeType PHONE = new NodeType("PHONE");

        /** NodeType to indicate the node contains a word. */
        public static NodeType STATE = new NodeType("STATE");


        /** Constructs a NodeType with the given name. */
        protected NodeType(String name) {
            this.name = name;
        }


        /**
        /// Returns true if the given NodeType is equal to this NodeType.
         *
        /// @param nodeType the NodeType to compare
        /// @return true if they are the same, false otherwise
         */
        public bool Equals(NodeType nodeType) 
        {
            if (nodeType != null) {
                return ToString().Equals(nodeType.ToString());
            } else {
                return false;
            }
        }


        /**
        /// Returns the name of this NodeType.
         *
        /// @return the name of this NodeType.
         */
        public override string ToString() 
        {
            return name;
        }

    }
}
