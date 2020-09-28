using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Jsgf.Rule;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf
{

    /**
     * @author Paul Lamere
     * @author Peter Wolf
     * @author Francisco Aguilera <falven@uw.edu>
     */
    public class JSGFRuleGrammar
    {

        private readonly String _lineSeparator = Environment.NewLine;

        protected readonly HashMap<String, JSGFRuleState> Rules = new HashMap<String, JSGFRuleState>();
        protected internal readonly List<JSGFRuleName> Imports = new List<JSGFRuleName>();
        protected readonly List<String> ImportedRules = new List<String>();

        protected readonly HashMap<String, ICollection<String>> RuleTags = new HashMap<String, ICollection<String>>();

        private readonly String _name;
        private readonly JSGFRuleGrammarManager _manager;

        /** Storage for documentation comments for rules for JSGF doc. */
        readonly JProperties _ruleDocComments = new JProperties();

        /** Storage for documentation comments for imports for JSGF doc. */
        readonly JProperties _importDocComments = new JProperties();

        /** Storage for documentation comments for the grammar for JSGF doc. */
        String _grammarDocComment;

        /* Holds the state of the rule in grammar */

        protected class JSGFRuleState
        {

            public bool IsPublic;
            protected internal bool _isEnabled;
            public JSGFRule rule;
            public List<String> Samples;
            public bool IsChanged;

            public JSGFRuleState(JSGFRule rule, bool isEnabled, bool isPublic)
            {
                this.rule = rule;
                this.IsPublic = isPublic;
                this._isEnabled = isEnabled;
                Samples = new List<String>();
            }
        }

        /**
         * Create a new RuleGrammar
         * 
         * @param name
         *            the name of this Grammar.
         * @param manager
         *            the manager for the created Grammars
         */
        public JSGFRuleGrammar(String name, JSGFRuleGrammarManager manager)
        {
            this._name = name;
            this._manager = manager;
        }

        /** Add the Grammar comment. */
        public void AddGrammarDocComment(String comment)
        {
            _grammarDocComment = comment;
        }

        /**
         * Import all rules or a specified rule from another grammar.
         * 
         * @param importName
         *            the name of the rule(s) to import.
         */
        public void AddImport(JSGFRuleName importName)
        {
            if (!Imports.Contains(importName))
            {
                Imports.Add(importName);
            }
        }

        /** Add a new import comment. */
        public void AddImportDocComment(JSGFRuleName imp, String comment)
        {
            Java.Put(_importDocComments, imp.ToString(), comment);
        }

        /** Add a new RuleGrammar comment. */
        public void AddRuleDocComment(String rname, String comment)
        {
            Java.Put(_ruleDocComments, rname, comment);
        }

        /**
         * add a sample sentence to the list of sample sentences that go with the
         * specified rule
         */
        public void AddSampleSentence(String ruleName, String sample)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            if (state == null)
            {
                return;
            }
            state.Samples.Add(sample);
        }

        /**
         * Delete a rule from the grammar.
         * 
         * @param ruleName
         *            the name of the rule.
         */
        public void DeleteRule(String ruleName)
        {
            Rules.Remove(GetKnownRule(ruleName).RuleName);
        }

        /** Retrieve the Grammar comment. */
        public String GetGrammarDocComment()
        {
            return _grammarDocComment;
        }

        /** Retrieve an import comment. */
        public String GetImportDocComment(JSGFRuleName imp)
        {
            return _importDocComments.getProperty(imp.ToString(), null);
        }

        /**
         * Returns the jsgf tags associated to the given rule. Cf.
         * jsgf-specification for details.
         */
        public ICollection<String> GetJSGFTags(String ruleName)
        {
            return RuleTags.Get(ruleName);
        }

        /**
         * Gets the Rule with the given name after it has been stripped, or throws
         * an Exception if it is unknown.
         */
        private JSGFRule GetKnownRule(String ruleName)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            if (state == null)
            {
                throw new ArgumentException("Unknown Rule: " + ruleName);
            }
            return state.rule;
        }

        public String GetName()
        {
            return _name;
        }

        /**
         * Return the data structure for the named rule.
         * 
         * @param ruleName
         *            the name of the rule.
         */
        public JSGFRule GetRule(String ruleName)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            if (state == null)
            {
                return null;
            }
            return state.rule;
        }

        /** Retrieve a RuleGrammar comment. */
        public String GetRuleDocComment(String rname)
        {
            return _ruleDocComments.getProperty(rname, null);
        }

        /**
         * Test whether the specified rule is public.
         * 
         * @param ruleName
         *            the name of the rule.
         */
        public bool IsRulePublic(String ruleName)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            if (state == null)
            {
                return false;
            }
            return state.IsPublic;
        }

        /** List the current imports. */
        public List<JSGFRuleName> GetImports()
        {
            return Imports;
        }

        /** List the names of all rules define in this Grammar. */
        public HashSet<String> GetRuleNames()
        {
            return Rules.KeySet();
        }

        /**
         * Remove an import.
         * 
         * @param importName
         *            the name of the rule(s) to remove.
         */
        public void RemoveImport(JSGFRuleName importName)
        {
            if (Imports.Contains(importName))
            {
                Imports.Remove(importName);
            }
        }

        /**
         * Resolve a simple or qualified rule name as a full rule name.
         * 
         * @param ruleName
         *            the name of the rule.
         */
        public JSGFRuleName Resolve(JSGFRuleName ruleName)
        {
            // System.out.println ("Resolving " + ruleName);
            JSGFRuleName rn = new JSGFRuleName(ruleName.GetRuleName());

            String simpleName = rn.GetSimpleRuleName();
            String grammarName = rn.GetSimpleGrammarName();
            String packageName = rn.GetPackageName();
            String fullGrammarName = rn.GetFullGrammarName();

            // Check for badly formed RuleName
            if (packageName != null && grammarName == null)
            {
                throw new JSGFGrammarException("Error: badly formed rulename " + rn);
            }

            if (ruleName.GetSimpleRuleName().Equals("NULL"))
            {
                return JSGFRuleName.Null;
            }

            if (ruleName.GetSimpleRuleName().Equals("VOID"))
            {
                return JSGFRuleName.Void;
            }

            // Check simple case: a local rule reference
            if (fullGrammarName == null && GetRule(simpleName) != null)
            {
                return new JSGFRuleName(_name + '.' + simpleName);
            }

            // Check for fully-qualified reference
            if (fullGrammarName != null)
            {
                JSGFRuleGrammar g = _manager.RetrieveGrammar(fullGrammarName);
                if (g != null)
                {
                    if (g.GetRule(simpleName) != null)
                    {
                        // we have a successful resolution
                        return new JSGFRuleName(fullGrammarName + '.' + simpleName);
                    }
                }
            }

            // Collect all matching imports into a list. After trying to
            // match rn to each import statement the vec will have
            // size()=0 if rn is unresolvable
            // size()=1 if rn is properly resolvable
            // size()>1 if rn is an ambiguous reference
            List<JSGFRuleName> matches = new List<JSGFRuleName>();

            // Get list of imports
            // Add local grammar to simply the case of checking for
            // a qualified or fully-qualified local reference.
            List<JSGFRuleName> imports = new List<JSGFRuleName>(this.Imports);
            imports.Add(new JSGFRuleName(_name + ".*"));

            // Check each import statement for a possible match
            foreach (JSGFRuleName importName in imports)
            {
                // TO-DO: update for JSAPI 1.0
                String importSimpleName = importName.GetSimpleRuleName();
                String importGrammarName = importName.GetSimpleGrammarName();
                String importFullGrammarName = importName.GetFullGrammarName();

                // Check for badly formed import name
                if (importFullGrammarName == null)
                {
                    throw new JSGFGrammarException("Error: badly formed import " + ruleName);
                }

                // Get the imported grammar
                JSGFRuleGrammar gref = _manager.RetrieveGrammar(importFullGrammarName);
                if (gref == null)
                {
                    Console.WriteLine("Warning: import of unknown grammar " + ruleName + " in " + _name);
                    continue;
                }

                // If import includes simpleName, test that it really exists
                if (!importSimpleName.Equals("*") && gref.GetRule(importSimpleName) == null)
                {
                    Console.WriteLine("Warning: import of undefined rule " + ruleName + " in " + _name);
                    continue;
                }

                // Check for fully-qualified or qualified reference
                if (importFullGrammarName.Equals(fullGrammarName) || importGrammarName.Equals(fullGrammarName))
                {
                    // Know that either
                    // import <ipkg.igram.???> matches <pkg.gram.???>
                    // OR
                    // import <ipkg.igram.???> matches <gram.???>
                    // (ipkg may be null)

                    if (importSimpleName.Equals("*"))
                    {
                        if (gref.GetRule(simpleName) != null)
                        {
                            // import <pkg.gram.*> matches <pkg.gram.rulename>
                            matches.Add(new JSGFRuleName(importFullGrammarName + '.' + simpleName));
                        }
                        continue;
                    }
                    else
                    {
                        // Now testing
                        // import <ipkg.igram.iRuleName> against <??.gram.ruleName>
                        //
                        if (importSimpleName.Equals(simpleName))
                        {
                            // import <pkg.gram.rulename> exact match for
                            // <???.gram.rulename>
                            matches.Add(new JSGFRuleName(importFullGrammarName + '.' + simpleName));
                        }
                        continue;
                    }
                }

                // If we get here and rulename is qualified or fully-qualified
                // then the match failed - try the next import statement
                if (fullGrammarName != null)
                {
                    continue;
                }

                // Now test
                // import <ipkg.igram.*> against <simpleName>

                if (importSimpleName.Equals("*"))
                {
                    if (gref.GetRule(simpleName) != null)
                    {
                        // import <pkg.gram.*> matches <simpleName>
                        matches.Add(new JSGFRuleName(importFullGrammarName + '.' + simpleName));
                    }
                    continue;
                }

                // Finally test
                // import <ipkg.igram.iSimpleName> against <simpleName>

                if (importSimpleName.Equals(simpleName))
                {
                    matches.Add(new JSGFRuleName(importFullGrammarName + '.' + simpleName));
                    continue;
                }
            }

            // The return behavior depends upon number of matches
            switch (matches.Count)
            {
                case 0: // Return null if rulename is unresolvable
                    return null;
                case 1: // Return successfully
                    return matches[0];
                default: // Throw exception if ambiguous reference
                    StringBuilder b = new StringBuilder();
                    b.Append("Warning: ambiguous reference ").Append(rn).Append(" in ").Append(_name).Append(" to ");
                    foreach (JSGFRuleName tmp in matches)
                    {
                        b.Append(tmp).Append(" and ");
                    }
                    b.Length = (b.Length - 5);
                    throw new JSGFGrammarException(b.ToString());
            }
        }

        /** Resolve and link up all rule references contained in all rules. */
        public void ResolveAllRules()
        {
            StringBuilder b = new StringBuilder();

            // First make sure that all imports are resolvable
            foreach (JSGFRuleName ruleName in Imports)
            {
                String grammarName = ruleName.GetFullGrammarName();
                JSGFRuleGrammar GI = _manager.RetrieveGrammar(grammarName);
                if (GI == null)
                {
                    b.Append("Undefined grammar ").Append(grammarName).Append(" imported in ").Append(_name).Append('\n');
                }
            }
            if (b.Length > 0)
            {
                throw new JSGFGrammarException(b.ToString());
            }

            foreach (JSGFRuleState state in Rules.Values)
            {
                ResolveRule(state.rule);
            }
        }

        /** Resolve the given rule. */
        protected void ResolveRule(JSGFRule r)
        {

            if (r is JSGFRuleToken)
            {
                return;
            }

            if (r is JSGFRuleAlternatives)
            {
                foreach (JSGFRule rule in ((JSGFRuleAlternatives)r).GetRules())
                {
                    ResolveRule(rule);
                }
                return;
            }

            if (r is JSGFRuleSequence)
            {
                foreach (JSGFRule rule in ((JSGFRuleSequence)r).Rules)
                {
                    ResolveRule(rule);
                }
                return;
            }

            if (r is JSGFRuleCount)
            {
                ResolveRule(((JSGFRuleCount)r).Rule);
                return;
            }

            if (r is JSGFRuleTag)
            {
                JSGFRuleTag rt = (JSGFRuleTag)r;

                JSGFRule rule = rt.Rule;
                String ruleStr = rule.ToString();

                // add the tag the tag-table
                var tags = RuleTags.Get(ruleStr);
                if (tags == null)
                {
                    tags = new HashSet<String>();
                    RuleTags.Put(ruleStr, tags);
                }
                tags.Add(rt.Tag);

                ResolveRule(rule);
                return;
            }

            if (r is JSGFRuleName)
            {
                JSGFRuleName rn = (JSGFRuleName)r;
                JSGFRuleName resolved = Resolve(rn);

                if (resolved == null)
                {
                    throw new JSGFGrammarException("Unresolvable rulename in grammar " + _name + ": " + rn);
                }
                else
                {
                    // TODO: This forces all rule names to be fully resolved.
                    // This should be changed.
                    rn.ResolvedRuleName = resolved.GetRuleName();
                    rn.SetRuleName(resolved.GetRuleName());
                    return;
                }
            }

            throw new JSGFGrammarException("Unknown rule type");
        }

        /**
         * Set the enabled property of the Grammar.
         * 
         * @param enabled
         *            the new desired state of the enabled property.
         */
        public void SetEnabled(bool enabled)
        {
            foreach (JSGFRuleState state in Rules.Values)
            {
                state._isEnabled = enabled;
            }
        }

        public bool IsEnabled(String ruleName)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            if (state != null)
            {
                return state._isEnabled;
            }
            return false;
        }

        /**
         * Set the enabled state of the listed rule.
         * 
         * @param ruleName
         *            the name of the rule.
         * @param enabled
         *            the new enabled state.
         */
        public void SetEnabled(String ruleName, bool enabled)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            if (state._isEnabled != enabled)
            {
                state._isEnabled = enabled;
            }
        }

        /**
         * Set a rule in the grammar either by creating a new rule or updating an
         * existing rule.
         * 
         * @param ruleName
         *            the name of the rule.
         * @param rule
         *            the definition of the rule.
         * @param isPublic
         *            whether this rule is public or not.
         */
        public void SetRule(String ruleName, JSGFRule rule, bool isPublic)
        {
            JSGFRuleState state = new JSGFRuleState(rule, true, isPublic);
            Rules.Put(ruleName, state);
        }

        /**
         * Returns a string containing the specification for this grammar.
         * 
         * @return specification for this grammar.
         */

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("#JSGF V1.0;").Append(_lineSeparator);
            sb.Append(_lineSeparator);
            sb.Append(FormatComment(_grammarDocComment));
            sb.Append(_lineSeparator);
            sb.Append("grammar ").Append(_name).Append(';').Append(_lineSeparator);
            sb.Append(_lineSeparator);
            // Set of comment keys (The import such comment belongs to).
            var docComments = _importDocComments.keySet();
            for (int i = 0; i < Imports.Count; i++)
            {
                String curImport = '<' + Imports[i].GetRuleName() + '>';
                if (docComments.Contains(curImport))
                {
                    sb.Append(FormatComment(_importDocComments.get(curImport)));
                    sb.Append(_lineSeparator);
                    sb.Append("import ").Append(curImport + ';').Append(_lineSeparator);
                    sb.Append(_lineSeparator);
                }
            }
            docComments = _ruleDocComments.keySet();
            foreach (var entry in Rules)
            {
                var rule = entry.Key;
                if ((docComments.Count > 0) && docComments.Contains(rule))
                {
                    sb.Append(FormatComment(_ruleDocComments.get(rule))).Append(_lineSeparator);
                }
                JSGFRuleState state = entry.Value;
                if (state.IsPublic)
                {
                    sb.Append("public ");
                }
                sb.Append('<').Append(rule).Append("> = ").Append(state.rule).Append(';').Append(_lineSeparator);
                sb.Append(_lineSeparator);
            }
            return sb.ToString();
        }

        /**
         * Expands the given String comment into: A. a multi-line comment if the
         * provided String contains any newline characters. B. a single-line comment
         * if comment does not contain any newline characters.
         * 
         * @param comment
         *            The String to expand into a multi or single line comment.
         * @return If the provided string is not null, the multi or single line
         *         representation of the provided comment, otherwise an empty string
         *         ("").
         */
        private String FormatComment(String comment)
        {
            StringBuilder sb = new StringBuilder("");
            if (comment == null)
            {
                return sb.ToString();
            }
            else if (Pattern.Compile("[\\n\\r\\f]+").Matcher(comment).Find())
            {
                String[] tokens = comment.Split('[' + _lineSeparator + "]+");
                sb.Append("/**").Append(_lineSeparator);
                sb.Append("  *").Append(tokens[0]).Append(_lineSeparator);
                for (int i = 1; i < tokens.Length; i++)
                {
                    sb.Append("  *").Append(tokens[i]).Append(_lineSeparator);
                }
                sb.Append("  */");
                return sb.ToString();
            }
            else
            {
                return "//" + comment;
            }
        }

        /**
         * This JSGFRule grammar will be saved to the file in the provided URL,
         * Overwriting any contents in the provided file, or creating a new one if
         * it does not exist.
         * 
         * @param url
         *            The URL to save this JSGFRuleGrammar to.
         * @throws URISyntaxException
         *             If there was a problem converting the given url to uri.
         * @throws IOException
         *             if an error occurs while saving or compiling the grammar
         */
        public void SaveJSGF(FileInfo url)
        {
            StreamWriter @out = new StreamWriter(url.FullName);
            @out.Write(ToString());
            @out.Flush();
            @out.Close();
        }

        public bool IsRuleChanged(String ruleName)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            return state.IsChanged;
        }

        public void SetRuleChanged(String ruleName, bool changed)
        {
            JSGFRuleState state = Rules.Get(ruleName);
            state.IsChanged = changed;
        }
    }
}
