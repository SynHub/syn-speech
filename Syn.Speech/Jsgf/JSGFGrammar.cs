using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
using Syn.Speech.Jsgf.Parser;
using Syn.Speech.Jsgf.Rule;
using Syn.Speech.Linguist.Dictionary;
using Syn.Speech.Linguist.Language.Grammar;
using Syn.Speech.Util;
using Syn.Speech.Util.Props;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{
    /**
     * <h3>Defines a BNF-style grammar based on JSGF grammar rules in a file.</h3>
     * 
     * 
     * The Java Speech Grammar Format (JSGF) is a BNF-style, platform-independent,
     * and vendor-independent textual representation of grammars for use in speech
     * recognition. It is used by the <a
     * href="http://java.sun.com/products/java-media/speech/">Java Speech API
     * (JSAPI) </a>.
     * 
     * Here we only intend to give a couple of examples of grammars written in JSGF,
     * so that you can quickly learn to write your own grammars. For more examples
     * and a complete specification of JSGF, go to
     * 
     * <a href="http://java.sun.com/products/java-media/speech/forDevelopers/JSGF/">
     * http://java.sun.com/products/java-media/speech/forDevelopers/JSGF/ </a>.
     * 
     * 
     * <h3>Example 1: "Hello World" in JSGF</h3>
     * 
     * The example below shows how a JSGF grammar that generates the sentences
     * "Hello World":
     * 
     * <pre>
     *  #JSGF V1.0
     *  public &lt;helloWorld&gt; = Hello World;
     * </pre>
     * 
     * <i>Figure 1: Hello grammar that generates the sentences "Hello World". </i>
     * <p/>
     * 
     * The above grammar is saved in a file called "hello.gram". It defines a public
     * grammar rule called "helloWorld". In order for this grammar rule to be
     * publicly accessible, we must be declared it "public". Non-public grammar
     * rules are not visible outside of the grammar file.
     * 
     * The location of the grammar file(s) is(are) defined by the
     * {@link #PROP_BASE_GRAMMAR_URL baseGrammarURL}property. Since all JSGF grammar
     * files end with ".gram", it will automatically search all such files at the
     * given URL for the grammar. The name of the grammar to search for is specified
     * by {@link #PROP_GRAMMAR_NAME grammarName}. In this example, the grammar name
     * is "helloWorld".
     * 
     * <h3>Example 2: Command Grammar in JSGF</h3>
     * 
     * This examples shows a grammar that generates basic control commands like
     * "move a menu thanks please", "close file",
     * "oh mighty computer please kindly delete menu thanks". It is the same as one
     * of the command & control examples in the <a
     * href="http://java.sun.com/products/java-media/speech/forDevelopers/JSGF/"
     * >JSGF specification </a>. It is considerably more complex than the previous
     * example. It defines the public grammar called "basicCmd".
     * 
     * <pre>
     *  #JSGF V1.0
     *  public &lt;basicCmd&gt; = &lt;startPolite&gt; &lt;command&gt; &lt;endPolite&gt;;
     *  &lt;command&gt; = &lt;action&gt; &lt;object&gt;;
     *  &lt;action&gt; = /10/ open |/2/ close |/1/ delete |/1/ move;
     *  &lt;object&gt; = [the | a] (window | file | menu);
     *  &lt;startPolite&gt; = (please | kindly | could you | oh mighty computer) *;
     *  &lt;endPolite&gt; = [ please | thanks | thank you ];
     * </pre>
     * 
     * <i>Figure 2: Command grammar that generates simple control commands. </i>
     * <p/>
     * 
     * The features of JSGF that are shown in this example includes:
     * <ul>
     * <li>using other grammar rules within a grammar rule.
     * <li>the OR "|" operator.
     * <li>the grouping "(...)" operator.
     * <li>the optional grouping "[...]" operator.
     * <li>the zero-or-many "*" (called Kleene star) operator.
     * <li>a probability (e.g., "open" is more likely than the others).
     * </ul>
     * 
     * <h3>From JSGF to Grammar Graph</h3>
     * 
     * After the JSGF grammar is read in, it is converted to a graph of words
     * representing the grammar. Lets call this the grammar graph. It is from this
     * grammar graph that the eventual search structure used for speech recognition
     * is built. Below, we show the grammar graphs created from the above JSGF
     * grammars. The nodes <code>"&lt;sil&gt;"</code> means "silence".
     * 
     * <p/>
     * <img src="doc-files/helloWorld.jpg"> <br>
     * 
     * <i>Figure 3: Grammar graph created from the Hello World grammar. </i>
     * <p/>
     * <img src="doc-files/commandGrammar.jpg"> <br>
     * 
     * <i>Figure 4: Grammar graph created from the Command grammar. </i>
     * 
     * <h3>Limitations</h3>
     * 
     * There is a known limitation with the current JSGF support. Grammars that
     * contain non-speech loops currently cause the recognizer to hang.
     * <p/>
     * For example, in the following grammar
     * 
     * <pre>
     *  #JSGF V1.0
     *  grammar jsgf.nastygram;
     *  public &lt;nasty&gt; = I saw a ((cat* | dog* | mouse*)+)+;
     * </pre>
     * 
     * the production: ((cat* | dog* | mouse*)+)+ can result in a continuous loop,
     * since (cat* | dog* | mouse*) can represent no speech (i.e. zero cats, dogs
     * and mice), this is equivalent to ()+. To avoid this problem, the grammar
     * writer should ensure that there are no rules that could possibly match no
     * speech within a plus operator or kleene star operator.
     * 
     * <h3>Dynamic grammar behavior</h3> It is possible to modify the grammar of a
     * running application. Some rules and notes:
     * <ul>
     * <li>Unlike a JSAPI recognizer, the JSGF Grammar only maintains one Rule
     * Grammar. This restriction may be relaxed in the future.
     * <li>The grammar should not be modified while a recognition is in process
     * <li>The call to JSGFGrammar.loadJSGF will load in a completely new grammar,
     * tossing any old grammars or changes. No call to commitChanges is necessary
     * (although such a call would be harmless in this situation).
     * <li>RuleGrammars can be modified via calls to RuleGrammar.setEnabled and
     * RuleGrammar.setRule). In order for these changes to take place,
     * JSGFGrammar.commitChanges must be called after all grammar changes have been
     * made.
     * </ul>
     * 
     * <h3>Implementation Notes</h3>
     * <ol>
     * <li>All internal probabilities are maintained in LogMath log base.
     * </ol>
     */
    public class JSGFGrammar : Grammar
    {
        /// <summary>
        /// The property that defines the location of the JSGF grammar file.
        /// </summary>
        [S4String]
        public const String PropBaseGrammarUrl = "grammarLocation";

        /// <summary>
        /// The property that defines the location of the JSGF grammar file.
        /// </summary>
        [S4String(DefaultValue = "default.gram")]
        public const String PropGrammarName = "grammarName";

        // ---------------------
        // Configurable data
        // ---------------------
        private JSGFRuleGrammar _ruleGrammar;
        protected JSGFRuleGrammarManager Manager;
        protected RuleStack RuleStack;
        private readonly LogMath _logMath;

        protected bool LoadGrammar = true;
        protected GrammarNode FirstNode;
        //protected Logger logger;

        public JSGFGrammar(String location, String grammarName, bool showGrammar, bool optimizeGrammar, bool addSilenceWords, bool addFillerWords, IDictionary dictionary)
            : this(ConfigurationManagerUtils.ResourceToUrl(location), grammarName, showGrammar, optimizeGrammar, addSilenceWords, addFillerWords, dictionary)
        {

        }

        public JSGFGrammar(URL baseUrl, String grammarName, bool showGrammar, bool optimizeGrammar, bool addSilenceWords, bool addFillerWords, IDictionary dictionary)
            : base(showGrammar, optimizeGrammar, addSilenceWords, addFillerWords, dictionary)
        {

            _logMath = LogMath.GetLogMath();
            BaseUrl = baseUrl;
            GrammarName = grammarName;
            LoadGrammar = true;
            //logger = Logger.getLogger(getClass().getName());
        }

        public JSGFGrammar()
        {

        }

        /*
         * (non-Javadoc)
         * 
         * @see
         * edu.cmu.sphinx.util.props.Configurable#newProperties(edu.cmu.sphinx.util
         * .props.PropertySheet)
         */

        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            BaseUrl = ConfigurationManagerUtils.GetResource(PropBaseGrammarUrl, ps);
            //logger = ps.getLogger();
            GrammarName = ps.GetString(PropGrammarName);
            LoadGrammar = true;
        }

        /**
         * Returns the RuleGrammar of this JSGFGrammar.
         * 
         * @return the RuleGrammar
         */
        public JSGFRuleGrammar GetRuleGrammar()
        {
            return _ruleGrammar;
        }

        /**
         * Returns manager used to load grammars
         * 
         * @return manager with loaded grammars
         */
        public JSGFRuleGrammarManager GetGrammarManager()
        {
            if (Manager == null)
                Manager = new JSGFRuleGrammarManager();
            return Manager;
        }

        /**
         * Sets the URL context of the JSGF grammars.
         * 
         * @param url
         *            the URL context of the grammars
         */

        public URL BaseUrl { protected get; set; }

        /** Returns the name of this grammar. */

        public string GrammarName { get; private set; }

        /**
         * The JSGF grammar specified by grammarName will be loaded from the base
         * url (tossing out any previously loaded grammars)
         * 
         * @param grammarName
         *            the name of the grammar
         * @throws IOException
         *             if an error occurs while loading or compiling the grammar
         * @throws JSGFGrammarException
         * @throws JSGFGrammarParseException
         */
        public void LoadJSGF(String grammarName)
        {
            GrammarName = grammarName;
            LoadGrammar = true;
            CommitChanges();
        }

        /**
         * Creates the grammar.
         * 
         * @return the initial node of the Grammar
         */

        protected override GrammarNode CreateGrammar()
        {
            try
            {
                CommitChanges();
            }
            catch (JSGFGrammarException e)
            {
                throw new IOException(e.Message);
            }
            catch (JSGFGrammarParseException e)
            {
                throw new IOException(((Exception) e).Message);
            }
            return FirstNode;
        }

        /**
         * Returns the initial node for the grammar
         * 
         * @return the initial grammar node
         */

        public override GrammarNode InitialNode
        {
            get { return FirstNode; }
        }

        /**
         * Parses the given Rule into a network of GrammarNodes.
         * 
         * @param rule
         *            the Rule to parse
         * @return a grammar graph
         */
        protected GrammarGraph ProcessRule(JSGFRule rule)
        {
            GrammarGraph result;

            if (rule != null)
            {
                this.LogInfo("parseRule: " + rule);
            }

            if (rule is JSGFRuleAlternatives)
            {
                result = ProcessRuleAlternatives((JSGFRuleAlternatives)rule);
            }
            else if (rule is JSGFRuleCount)
            {
                result = ProcessRuleCount((JSGFRuleCount)rule);
            }
            else if (rule is JSGFRuleName)
            {
                result = ProcessRuleName((JSGFRuleName)rule);
            }
            else if (rule is JSGFRuleSequence)
            {
                result = ProcessRuleSequence((JSGFRuleSequence)rule);
            }
            else if (rule is JSGFRuleTag)
            {
                result = ProcessRuleTag((JSGFRuleTag)rule);
            }
            else if (rule is JSGFRuleToken)
            {
                result = ProcessRuleToken((JSGFRuleToken)rule);
            }
            else
            {
                throw new ArgumentException("Unsupported Rule type: " + rule);
            }
            return result;
        }

        /**
         * Parses the given RuleName into a network of GrammarNodes.
         * 
         * @param initialRuleName
         *            the RuleName rule to parse
         * @return a grammar graph
         */
        private GrammarGraph ProcessRuleName(JSGFRuleName initialRuleName)
        {
            this.LogInfo("parseRuleName: " + initialRuleName);
            GrammarGraph result = RuleStack.Contains(initialRuleName.GetRuleName());

            if (result != null)
            { // its a recursive call
                return result;
            }
            else
            {
                result = new GrammarGraph(this);
                RuleStack.Push(initialRuleName.GetRuleName(), result);
            }
            JSGFRuleName ruleName = _ruleGrammar.Resolve(initialRuleName);

            if (ruleName == JSGFRuleName.Null)
            {
                result.StartNode.Add(result.EndNode, 0.0f);
            }
            else if (ruleName == JSGFRuleName.Void)
            {
                // no connection for void
            }
            else
            {
                if (ruleName == null)
                {
                    throw new JSGFGrammarException("Can't resolve "
                            + initialRuleName + " g "
                            + initialRuleName.GetFullGrammarName());
                }
                JSGFRuleGrammar rg = Manager.RetrieveGrammar(ruleName
                        .GetFullGrammarName());
                if (rg == null)
                {
                    throw new JSGFGrammarException("Can't resolve grammar name "
                            + ruleName.GetFullGrammarName());
                }

                JSGFRule rule = rg.GetRule(ruleName.GetSimpleRuleName());
                if (rule == null)
                {
                    throw new JSGFGrammarException("Can't resolve rule: "
                            + ruleName.GetRuleName());
                }
                GrammarGraph ruleResult = ProcessRule(rule);
                if (result != ruleResult)
                {
                    result.StartNode.Add(ruleResult.StartNode, 0.0f);
                    ruleResult.EndNode.Add(result.EndNode, 0.0f);
                }
            }
            RuleStack.Pop();
            return result;
        }

        /**
         * Parses the given RuleCount into a network of GrammarNodes.
         * 
         * @param ruleCount
         *            the RuleCount object to parse
         * @return a grammar graph
         */
        private GrammarGraph ProcessRuleCount(JSGFRuleCount ruleCount)
        {
            this.LogInfo("parseRuleCount: " + ruleCount);
            GrammarGraph result = new GrammarGraph(this);
            int count = ruleCount.Count;
            GrammarGraph newNodes = ProcessRule(ruleCount.Rule);

            result.StartNode.Add(newNodes.StartNode, 0.0f);
            newNodes.EndNode.Add(result.EndNode, 0.0f);

            // if this is optional, add a bypass arc

            if (count == JSGFRuleCount.ZeroOrMore
                    || count == JSGFRuleCount.Optional)
            {
                result.StartNode.Add(result.EndNode, 0.0f);
            }

            // if this can possibly occur more than once, add a loopback

            if (count == JSGFRuleCount.OnceOrMore
                    || count == JSGFRuleCount.ZeroOrMore)
            {
                newNodes.EndNode.Add(newNodes.StartNode, 0.0f);
            }
            return result;
        }

        /**
         * Parses the given RuleAlternatives into a network of GrammarNodes.
         * 
         * @param ruleAlternatives
         *            the RuleAlternatives to parse
         * @return a grammar graph
         */
        private GrammarGraph ProcessRuleAlternatives(  JSGFRuleAlternatives ruleAlternatives)
        {
            this.LogInfo("parseRuleAlternatives: " + ruleAlternatives);
            GrammarGraph result = new GrammarGraph(this);

            List<JSGFRule> rules = ruleAlternatives.GetRules();
            List<Float> weights = GetNormalizedWeights(ruleAlternatives.GetWeights());

            // expand each alternative, and connect them in parallel
            for (int i = 0; i < rules.Count; i++)
            {
                JSGFRule rule = rules[i];
                float weight = 0.0f;
                if (weights != null)
                {
                    weight = weights[i];
                }
                this.LogInfo("Alternative: " + rule);
                GrammarGraph newNodes = ProcessRule(rule);
                result.StartNode.Add(newNodes.StartNode, weight);
                newNodes.EndNode.Add(result.EndNode, 0.0f);
            }

            return result;
        }

        /**
         * Normalize the weights. The weights should always be zero or greater. We
         * need to convert the weights to a log probability.
         * 
         * @param weights
         *            the weights to normalize
         */
        private List<Float> GetNormalizedWeights(List<Float> weights)
        {

            if (weights == null)
            {
                return null;
            }

            double sum = 0.0;
            foreach (float weight in weights)
            {
                if (weight < 0)
                {
                    throw new ArgumentException("Negative weight " + weight);
                }
                sum += weight;
            }

            var normalized = new List<Float>(weights);

            for (int i = 0; i < weights.Count; i++)
            {
                if (sum == 0.0f)
                {
                    normalized.Set(i, LogMath.LogZero);
                }
                else
                {
                    normalized.Set(i, _logMath.LinearToLog(weights[i] / sum));
                }
            }
            return normalized;
        }

        /**
         * Parses the given RuleSequence into a network of GrammarNodes.
         * 
         * @param ruleSequence
         *            the RuleSequence to parse
         * @return the first and last GrammarNodes of the network
         */
        private GrammarGraph ProcessRuleSequence(JSGFRuleSequence ruleSequence)
        {

            GrammarNode startNode = null;
            GrammarNode endNode = null;
            this.LogInfo("parseRuleSequence: " + ruleSequence);

            List<JSGFRule> rules = ruleSequence.Rules;

            GrammarNode lastGrammarNode = null;

            // expand and connect each rule in the sequence serially
            for (int i = 0; i < rules.Count; i++)
            {
                JSGFRule rule = rules[i];
                GrammarGraph newNodes = ProcessRule(rule);

                // first node
                if (i == 0)
                {
                    startNode = newNodes.StartNode;
                }

                // last node
                if (i == (rules.Count - 1))
                {
                    endNode = newNodes.EndNode;
                }

                if (i > 0)
                {
                    lastGrammarNode.Add(newNodes.StartNode, 0.0f);
                }
                lastGrammarNode = newNodes.EndNode;
            }

            return new GrammarGraph(startNode, endNode,this);
        }

        /**
         * Parses the given RuleTag into a network GrammarNodes.
         * 
         * @param ruleTag
         *            the RuleTag to parse
         * @return the first and last GrammarNodes of the network
         */
        private GrammarGraph ProcessRuleTag(JSGFRuleTag ruleTag)
        {
            this.LogInfo("parseRuleTag: " + ruleTag);
            JSGFRule rule = ruleTag.Rule;
            return ProcessRule(rule);
        }

        /**
         * Creates a GrammarNode with the word in the given RuleToken.
         * 
         * @param ruleToken
         *            the RuleToken that contains the word
         * @return a GrammarNode with the word in the given RuleToken
         */
        private GrammarGraph ProcessRuleToken(JSGFRuleToken ruleToken)
        {

            GrammarNode node = CreateGrammarNode(ruleToken.Text);
            return new GrammarGraph(node, node, this);
        }

        // ///////////////////////////////////////////////////////////////////
        // Loading part
        // //////////////////////////////////////////////////////////////////

        private static URL GrammarNameToUrl(URL baseUrl, String grammarName)
        {

            // Convert each period in the grammar name to a slash "/"
            // Append a slash and the converted grammar name to the base URL
            // Append the ".gram" suffix
            grammarName = grammarName.Replace('.', '/');
            StringBuilder sb = new StringBuilder();
            if (baseUrl != null)
            {
                sb.Append(baseUrl);
                if (sb[sb.Length - 1] != '/')
                    sb.Append('/');
            }
            sb.Append(grammarName).Append(".gram");
            String urlstr = sb.ToString();

            URL grammarUrl = null;
            try
            {
                grammarUrl = new URL(URLType.Path, urlstr);
            }
            catch (UriFormatException me)
            {
                //grammarURL = ClassLoader.getSystemResource(urlstr); //TODO: Check behaviour of comment
                if (grammarUrl == null)
                    throw new UriFormatException(urlstr);
            }

            return grammarUrl;
        }

        /**
         * Commit changes to all loaded grammars and all changes of grammar since
         * the last commitChange
         * 
         * @throws JSGFGrammarParseException
         * @throws JSGFGrammarException
         */
        public virtual void CommitChanges()
        {
            try
            {
                if (LoadGrammar)
                {
                    if (Manager == null)
                        GetGrammarManager();
                    _ruleGrammar = LoadNamedGrammar(GrammarName);
                    LoadImports(_ruleGrammar);
                    LoadGrammar = false;
                }

                Manager.LinkGrammars();
                RuleStack = new RuleStack();
                NewGrammar();

                FirstNode = CreateGrammarNode("<sil>");
                GrammarNode finalNode = CreateGrammarNode("<sil>");
                finalNode.SetFinalNode(true);

                // go through each rule and create a network of GrammarNodes
                // for each of them

                foreach (String ruleName in _ruleGrammar.GetRuleNames())
                {
                    if (_ruleGrammar.IsRulePublic(ruleName))
                    {
                        String fullName = GetFullRuleName(ruleName);
                        GrammarGraph publicRuleGraph = new GrammarGraph(this);
                        RuleStack.Push(fullName, publicRuleGraph);
                        JSGFRule rule = _ruleGrammar.GetRule(ruleName);
                        GrammarGraph graph = ProcessRule(rule);
                        RuleStack.Pop();

                        FirstNode.Add(publicRuleGraph.StartNode, 0.0f);
                        publicRuleGraph.EndNode.Add(finalNode, 0.0f);
                        publicRuleGraph.StartNode.Add(graph.StartNode,
                                0.0f);
                        graph.EndNode.Add(publicRuleGraph.EndNode, 0.0f);
                    }
                }
                PostProcessGrammar();
                if (Logger.Level == LogLevel.All)
                {
                    DumpGrammar();
                }
            }
            catch (UriFormatException mue)
            {
                throw new IOException("bad base grammar URL " + BaseUrl + ' ' + mue);
            }
        }

        /**
         * Load grammars imported by the specified RuleGrammar if they are not
         * already loaded.
         * 
         * @throws JSGFGrammarParseException
         */
        private void LoadImports(JSGFRuleGrammar grammar)
        {

            foreach (JSGFRuleName ruleName in grammar.Imports)
            {
                // System.out.println ("Checking import " + ruleName);
                String grammarName = ruleName.GetFullGrammarName();
                JSGFRuleGrammar importedGrammar = GetNamedRuleGrammar(grammarName);

                if (importedGrammar == null)
                {
                    // System.out.println ("Grammar " + grammarName +
                    // " not found. Loading.");
                    importedGrammar = LoadNamedGrammar(ruleName
                            .GetFullGrammarName());
                }
                if (importedGrammar != null)
                {
                    LoadImports(importedGrammar);
                }
            }
            LoadFullQualifiedRules(grammar);
        }

        private JSGFRuleGrammar GetNamedRuleGrammar(String grammarName)
        {
            return Manager.RetrieveGrammar(grammarName);
        }

        /**
         * Load named grammar from import rule
         * 
         * @param grammarName
         * @return already loaded grammar
         * @throws JSGFGrammarParseException
         * @throws IOException
         */
        private JSGFRuleGrammar LoadNamedGrammar(String grammarName)
        {

            var url = GrammarNameToUrl(BaseUrl, grammarName);
            JSGFRuleGrammar ruleGrammar = JSGFParser.NewGrammarFromJSGF(url,  new JSGFRuleGrammarFactory(Manager));
            ruleGrammar.SetEnabled(true);

            return ruleGrammar;
        }

        /**
         * Load grammars imported by a fully qualified Rule Token if they are not
         * already loaded.
         * 
         * @param grammar
         * @throws IOException
         * @throws GrammarException
         * @throws JSGFGrammarParseException
         */
        private void LoadFullQualifiedRules(JSGFRuleGrammar grammar)
        {

            // Go through every rule
            foreach (String ruleName in grammar.GetRuleNames())
            {
                String rule = grammar.GetRule(ruleName).ToString();
                // check for rule-Tokens
                int index = 0;
                while (index < rule.Length)
                {
                    index = rule.IndexOf('<', index);
                    if (index < 0)
                    {
                        break;
                    }
                    // Extract rule name
                    var endIndex = rule.IndexOf('>', index + 1);
                    JSGFRuleName extractedRuleName = new JSGFRuleName(rule
                            .Substring(index + 1, endIndex - (index + 1))
                            .Trim());
                    index = endIndex + 1;

                    // Check for full qualified rule name
                    if (extractedRuleName.GetFullGrammarName() != null)
                    {
                        String grammarName = extractedRuleName.GetFullGrammarName();
                        JSGFRuleGrammar importedGrammar = GetNamedRuleGrammar(grammarName);
                        if (importedGrammar == null)
                        {
                            importedGrammar = LoadNamedGrammar(grammarName);
                        }
                        if (importedGrammar != null)
                        {
                            LoadImports(importedGrammar);
                        }
                    }
                }
            }
        }

        /**
         * Gets the fully resolved rule name
         * 
         * @param ruleName
         *            the partial name
         * @return the fully resolved name
         * @throws JSGFGrammarException
         */
        private String GetFullRuleName(String ruleName)
        {
            JSGFRuleName rname = _ruleGrammar.Resolve(new JSGFRuleName(ruleName));
            return rname.GetRuleName();
        }

        /** Dumps interesting things about this grammar */
        protected void DumpGrammar()
        {
            Console.WriteLine("Imported rules { ");

            foreach (JSGFRuleName imp in _ruleGrammar.GetImports())
            {
                Console.WriteLine("  Import " + imp.GetRuleName());
            }
            Console.WriteLine("}");

            Console.WriteLine("Rulenames { ");

            foreach (String name in _ruleGrammar.GetRuleNames())
            {
                Console.WriteLine("  Name " + name);
            }
            Console.WriteLine("}");
        }
    }
}
