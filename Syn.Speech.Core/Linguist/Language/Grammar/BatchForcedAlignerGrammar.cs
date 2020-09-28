using System;
using System.Diagnostics;
using System.IO;
using Syn.Speech.Helper;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Linguist.Language.Grammar
{

    /// <summary>
    /// @author Peter Wolf
    /// </summary>
    public class BatchForcedAlignerGrammar : ForcedAlignerGrammar
    {

        /// <summary>
        /// Property that defines the reference file containing the transcripts used to create the froced align grammar.
        /// </summary>
        [S4String(DefaultValue = "<refFile not set>")]
        public const String PropRefFile = "refFile";

        protected String RefFile;
        protected readonly HashMap<String, GrammarNode> Grammars = new HashMap<String, GrammarNode>();
        protected String CurrentUttName = "";

        public BatchForcedAlignerGrammar(String refFile, bool showGrammar, bool optimizeGrammar, bool addSilenceWords,
                bool addFillerWords, IDictionary dictionary)
            : base(showGrammar, optimizeGrammar, addSilenceWords, addFillerWords, dictionary)
        {

            RefFile = refFile;
        }

        public BatchForcedAlignerGrammar()
        {
        }


        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            RefFile = ps.GetString(PropRefFile);
        }


        protected override GrammarNode CreateGrammar()
        {
            // TODO: FlatLinguist requires the initial grammar node
            // to contain a single silence. We'll do that for now,
            // but once the FlatLinguist is fixed, this should be
            // returned to its former method of creating an empty
            // initial grammar node
            //   initialNode = createGrammarNode(initialID, false);

            InitialNode = null;
            FinalNode = CreateGrammarNode(true);
            try
            {
                var @in = new LineNumberReader(new StreamReader(RefFile));
                String line;
                while (true)
                {
                    line = @in.ReadLine();

                    if (line == null || line.IsEmpty())
                        break;

                    int uttNameStart = line.IndexOf('(') + 1;
                    int uttNameEnd = line.IndexOf(')');

                    if (uttNameStart < 0 || uttNameStart > uttNameEnd)
                        continue;

                    String uttName = line.JSubString(uttNameStart, uttNameEnd);
                    String transcript = line.JSubString(0, uttNameStart - 1).Trim();

                    if (transcript.IsEmpty())
                        continue;

                    InitialNode = CreateGrammarNode(IDictionary.SilenceSpelling);
                    CreateForcedAlignerGrammar(InitialNode, FinalNode, transcript);
                    Grammars.Put(uttName, InitialNode);
                    CurrentUttName = uttName;
                }
                @in.Close();
            }
            catch (FileNotFoundException e)
            {
                throw new Error(e);
            }
            catch (IOException e)
            {
                throw new Error(e);
            }
            catch (NoSuchMethodException e)
            {
                throw new Error(e);
            }
            return InitialNode;
        }



        public void SetUtterance(String utteranceName)
        {
            InitialNode = Grammars.Get(utteranceName);
            Debug.Assert(InitialNode != null);
        }
    }

}
