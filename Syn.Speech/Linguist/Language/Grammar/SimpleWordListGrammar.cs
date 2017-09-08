using System;
using System.Collections.Generic;
using System.IO;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{
    /// <summary>
    /**
    /// Defines a grammar based upon a list of words in a file. The format of the file is just one word per line. For
    /// example, for an isolated digits grammar the file will simply look like:
    /// <pre>
    /// zero
    /// one
    /// two
    /// three
    /// four
    /// five
    /// six
    /// seven
    /// eight
    /// nine
    /// </pre>
    /// The path to the file is defined by the {@link #PROP_PATH PROP_PATH} property. If the {@link #PROP_LOOP PROP_LOOP}
    /// property is true, the grammar created will be a looping grammar. Using the above digits grammar example, setting
    /// PROP_LOOP to true will make it a connected-digits grammar.
    /// <p/>
    /// All probabilities are maintained in LogMath log base.
     */
    /// </summary>
    class SimpleWordListGrammar:Grammar
    {
        /// <summary>
        /// The property that defines the location of the word list grammar.
        /// </summary>
        [S4String(DefaultValue = "spelling.gram")]
        public static string PropPath = "path";

        /// <summary>
        /// The property that if true, indicates that this is a looping grammar.
        /// </summary>
        [S4Boolean(DefaultValue = true)]
        public static string PropLoop = "isLooping";

        // ---------------------
        // Configurable data
        // ---------------------
        private string _path;
        private Boolean _isLooping;
        private LogMath _logMath;

        public SimpleWordListGrammar(String path, Boolean isLooping, Boolean showGrammar, Boolean optimizeGrammar, Boolean addSilenceWords, Boolean addFillerWords, IDictionary dictionary) 
            :base(showGrammar,optimizeGrammar,addSilenceWords,addFillerWords,dictionary)
        {
            this._path = path;
            this._isLooping = isLooping;
            _logMath = LogMath.GetLogMath();
        }

        public SimpleWordListGrammar() 
        {

        }

        /*
       /// (non-Javadoc)
        *
       /// @see edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util.props.PropertySheet)
        */
        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
        
            _path = ps.GetString(PropPath);
            _isLooping = ps.GetBoolean(PropLoop);
            _logMath = LogMath.GetLogMath();
        }


        /**
        /// Create class from reference text (not implemented).
         *
        /// @param bogusText dummy variable
         */
        protected override GrammarNode CreateGrammar(String bogusText)
        {
            throw new NotImplementedException("Does not create "
                    + "grammar with reference text");
        }


        /** Creates the grammar. */
        override
        protected GrammarNode CreateGrammar()
        {

            ExtendedStreamTokenizer tok = null;
            if(File.Exists(_path))
                tok = new ExtendedStreamTokenizer(_path, true);
            else
                tok = new ExtendedStreamTokenizer((StreamReader)null, true);
            GrammarNode initialNode = CreateGrammarNode("<sil>");
            GrammarNode branchNode = CreateGrammarNode(false);
            GrammarNode finalNode = CreateGrammarNode("<sil>");
            finalNode.SetFinalNode(true);
            List<GrammarNode> wordGrammarNodes = new List<GrammarNode>();
            while (!tok.IsEOF()) 
            {
                string word;
                while ((word = tok.GetString()) != null) 
                {
                    wordGrammarNodes.Add(CreateGrammarNode(word));
                }
            }
            // now connect all the GrammarNodes together
            initialNode.Add(branchNode, LogMath.LogOne);
            if (wordGrammarNodes.Count != 0)
            {
                float branchScore = _logMath.LinearToLog(1.0 / wordGrammarNodes.Count);
                foreach (GrammarNode wordNode in wordGrammarNodes)
                {
                    branchNode.Add(wordNode, branchScore);
                    wordNode.Add(finalNode, LogMath.LogOne);
                    if (_isLooping)
                    {
                        wordNode.Add(branchNode, LogMath.LogOne);
                    }
                }
            }
            return initialNode;
        }
    }
}
