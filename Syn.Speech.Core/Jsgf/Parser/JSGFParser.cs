using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Jsgf.Rule;
//PATROLLED + REFACTORED
namespace Syn.Speech.Jsgf.Parser
{
    public class JSGFParser : JSGFParserConstants
    {
        // For now we create one global parser, if needed JavaCC can be set
        // to allow the creation of multiple parser instances
        //
        const String Version = "1.0";

        static JSGFParser _parser;

        // This main method simply checks the syntax of a jsgf Grammar
        //
        public static void Main(String[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("JSGF Parser Version " + Version + ":  Reading from standard input . . .");
                _parser = new JSGFParser(Console.In);
            }
            else if (args.Length > 0)
            {
                Console.WriteLine("JSGF Parser Version " + Version + ":  Reading from file " + args[0] + " . . .");
                try
                {
                    FileInfo codeBase = null;
                    String path = Directory.GetCurrentDirectory() + "/" + args[0];
                    try
                    {
                        codeBase = new FileInfo(path);
                    }
                    catch (UriFormatException e)
                    {
                        Console.WriteLine("Could not get URL for current directory " + e);
                        return;
                    }
                    BufferedStream i = new BufferedStream(codeBase.OpenRead(), 256);
                    JSGFEncoding encoding = GetJSGFEncoding(i);
                    TextReader rdr;
                    if ((encoding != null) && (encoding.Encoding != null))
                    {
                        Console.WriteLine("Grammar Character Encoding \"" + encoding.Encoding + "\"");
                        rdr = new StreamReader(i, encoding.GetEncoding);
                    }
                    else
                    {
                        if (encoding == null) Console.WriteLine("WARNING: Grammar missing self identifying header");
                        rdr = new StreamReader(i);
                    }
                    _parser = new JSGFParser(rdr);
                }
                catch (Exception e)
                {
                    Console.WriteLine("JSGF Parser Version " + Version + ":  File " + args[0] + " not found.");
                    return;
                }
            }
            else
            {
                Console.WriteLine("JSGF Parser Version " + Version + ":  Usage is one of:");
                Console.WriteLine("         java JSGFParser < inputfile");
                Console.WriteLine("OR");
                Console.WriteLine("         java JSGFParser inputfile");
                return;
            }
            try
            {
                _parser.GrammarUnit(new JSGFRuleGrammarFactory(new JSGFRuleGrammarManager()));
                Console.WriteLine("JSGF Parser Version " + Version + ":  JSGF Grammar parsed successfully.");
            }
            catch (ParseException e)
            {
                Console.WriteLine("JSGF Parser Version " + Version + ":  Encountered errors during parse." + e.Message);
            }
        }

        /**
         * newGrammarFromJSGF - Once JavaCC supports Readers we will change this
         */
        public static JSGFRuleGrammar NewGrammarFromJSGF(Stream i, JSGFRuleGrammarFactory factory)
        {
            JSGFRuleGrammar grammar = null;
            if (_parser == null)
            {
                _parser = new JSGFParser(i);
            }
            else
            {
                _parser.ReInit(i);
            }
            try
            {
                grammar = _parser.GrammarUnit(factory);
                return grammar;
            }
            catch (ParseException e)
            {
                Token etoken = e.CurrentToken;
                JSGFGrammarParseException ge = new JSGFGrammarParseException(etoken.BeginLine, etoken.BeginColumn, "Grammar Error", e.Message);
                throw ge;
            }
        }

        /**
         * newGrammarFromJSGF - Once JavaCC supports Readers we will change this
         */
        public static JSGFRuleGrammar NewGrammarFromJSGF(TextReader i, JSGFRuleGrammarFactory factory)
        {
            JSGFRuleGrammar grammar = null;
            if (_parser == null)
            {
                _parser = new JSGFParser(i);
            }
            else
            {
                _parser.ReInit(i);
            }
            try
            {
                grammar = _parser.GrammarUnit(factory);
                return grammar;
            }
            catch (ParseException e)
            {
                Token etoken = e.CurrentToken;
                JSGFGrammarParseException ge = new JSGFGrammarParseException(etoken.BeginLine, etoken.BeginColumn, "Grammar Error", e.Message);
                throw ge;
            }
        }

        private static JSGFEncoding GetJSGFEncoding(BufferedStream bufferedStream)
        {
            int i = 0;
            byte[] b = new byte[2];
            byte[] c = new byte[80];
            int _markPosition = 256;
            //@is.mark(256);
            /* read 2 bytes */
            try
            {
                if (bufferedStream.Read(b, 0, 2) != 2)
                {
                    bufferedStream.Reset(_markPosition);
                    return null;
                }
                if ((b[0] == 0x23) && (b[1] == 0x4A))
                {
                    // UTF-8
                    i = 0;
                    c[i++] = b[0];
                    c[i++] = b[1];
                    while (i < 80)
                    {
                        if (bufferedStream.Read(b, 0, 1) != 1)
                        {
                            bufferedStream.Reset(_markPosition);
                            return null;
                        }
                        if ((b[0] == 0x0A) || (b[0] == 0x0D)) break;
                        c[i++] = b[0];
                    }
                }
                else if ((b[0] == 0x23) && (b[1] == 0x00))
                {
                    // UTF-16 BE
                    i = 0;
                    c[i++] = b[0];
                    while (i < 80)
                    {
                        if (bufferedStream.Read(b, 0, 2) != 2)
                        {
                            bufferedStream.Reset(_markPosition);
                            return null;
                        }
                        if (b[1] != 0) return null;
                        if ((b[0] == 0x0A) || (b[0] == 0x0D)) break;
                        c[i++] = b[0];
                    }
                }
                else if ((b[0] == 0x00) && (b[1] == 0x23))
                {
                    // UTF-16 LE
                    i = 0;
                    c[i++] = b[1];
                    while (i < 80)
                    {
                        if (bufferedStream.Read(b, 0, 2) != 2)
                        {
                            bufferedStream.Reset(_markPosition);
                            return null;
                        }
                        if (b[0] != 0) return null;
                        if ((b[1] == 0x0A) || (b[1] == 0x0D)) break;
                        c[i++] = b[1];
                    }
                }
            }
            catch (IOException ioe)
            {
                try
                {
                    bufferedStream.Reset(_markPosition);
                }
                catch (IOException ioe2)
                {
                }
                return null;
            }
            if (i == 0)
            {
                try
                {
                    bufferedStream.Reset(_markPosition);
                }
                catch (IOException ioe2)
                {
                }
                return null;
            }
            //
            // Now c[] should have first line of text in UTF-8 format
            //
            var cCharArray = Encoding.UTF8.GetString(c).ToCharArray();
            String estr = new String(cCharArray, 0, i);
            StringTokenizer st = new StringTokenizer(estr, " \u005ct\u005cn\u005cr\u005cf;");
            String id = null;
            String ver = null;
            String enc = null;
            String loc = null;
            if (st.hasMoreTokens()) id = st.nextToken();
            if (!id.Equals("#JSGF"))
            {
                try
                {
                    bufferedStream.Reset(_markPosition);
                }
                catch (IOException ioe2)
                {
                }
                return null;
            }
            if (st.hasMoreTokens()) ver = st.nextToken();
            if (st.hasMoreTokens()) enc = st.nextToken();
            if (st.hasMoreTokens()) loc = st.nextToken();
            return new JSGFEncoding(ver, enc, loc);
        }

        /**
         * newGrammarFromURL
         */
        public static JSGFRuleGrammar NewGrammarFromJSGF(URL url, JSGFRuleGrammarFactory factory)
        {
            TextReader reader;
            BufferedStream stream = new BufferedStream(url.OpenStream(), 256);
            JSGFEncoding encoding = GetJSGFEncoding(stream);
            if ((encoding != null) && (encoding.Encoding != null))
            {
                Console.WriteLine("Grammar Character Encoding \"" + encoding.Encoding + "\"");
                reader = new StreamReader(stream, encoding.GetEncoding);
            }
            else
            {
                if (encoding == null) Console.WriteLine("WARNING: Grammar missing self identifying header");
                reader = new StreamReader(stream);
            }
            return NewGrammarFromJSGF(reader, factory);
        }

        /**
         * ruleForJSGF
         */
        public static JSGFRule RuleForJSGF(String text)
        {
            JSGFRule r = null;
            try
            {
                StringReader sread = new StringReader(text);
                if (_parser == null) _parser = new JSGFParser(sread);
                else _parser.ReInit(sread);
                r = _parser.Alternatives();
                // Console.WriteLine("JSGF Parser Version " + version
                //                    + ":  JSGF RHS parsed successfully.");
            }
            catch (ParseException e)
            {
                Console.WriteLine("JSGF Parser Version " + Version + ":  Encountered errors during parse.");
            }
            return r;
        }

        /**
        * extract @keywords from documentation comments
        */
        static void ExtractKeywords(JSGFRuleGrammar grammar, String rname, String comment)
        {
            int i = 0;
            while ((i = comment.IndexOf("@example ", i) + 9) > 9)
            {
                int j = Math.Max(comment.IndexOf('\r', i), comment.IndexOf('\n', i));
                if (j < 0)
                {
                    j = comment.Length;
                    if (comment.EndsWith(("*/")))
                        j -= 2;
                }
                grammar.AddSampleSentence(rname, comment.Substring(i, j).Trim());
                i = j + 1;
            }
        }

        public JSGFRuleGrammar GrammarUnit(JSGFRuleGrammarFactory factory)
        {
            JSGFRuleGrammar grammar = null;
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Identifier:
                    IdentHeader();
                    break;
                default:
                    jj_la1[0] = jj_gen;
                    break;
            }
            grammar = GrammarDeclaration(factory);
        //label_1:
            var label_1 = false;
            while (true)
            {
                switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                {
                    case Import:
                        break;
                    default:
                        jj_la1[1] = jj_gen;
                        label_1 = true;break;
                }
                if (label_1) break;
                ImportDeclaration(grammar);
            }
        //label_2:
            var label_2 = false;
            while (true)
            {
                switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                {
                    case Public:
                    case 28:
                        ;
                        break;
                    default:
                        jj_la1[2] = jj_gen;
                        label_2 = true;break;
                }
                if (label_2) break;
                RuleDeclaration(grammar);
            }
            jj_consume_token(0);
            { if (true) return grammar; }
        }


        public JSGFRuleGrammar GrammarDeclaration(JSGFRuleGrammarFactory factory)
        {
            String s;
            JSGFRuleGrammar grammar = null;
            Token t = null;
            t = jj_consume_token(Grammar);
            s = Name();
            jj_consume_token(26);
            grammar = factory.NewGrammar(s);
            if (grammar != null && t != null && t.SpecialToken != null)
            {
                if (t.SpecialToken.Image != null && t.SpecialToken.Image.StartsWith("/**"))
                {
                    JSGFRuleGrammar JG = grammar;
                    JG.AddGrammarDocComment(t.SpecialToken.Image);
                }
            }
            { if (true) return grammar; }
        }

        public void IdentHeader()
        {
            jj_consume_token(Identifier);
            jj_consume_token(27);
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Identifier:
                    jj_consume_token(Identifier);
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case Identifier:
                            jj_consume_token(Identifier);
                            break;
                        default:
                            jj_la1[3] = jj_gen;
                            break;
                    }
                    break;
                default:
                    jj_la1[4] = jj_gen;
                    break;
            }
            jj_consume_token(26);
        }

        public void ImportDeclaration(JSGFRuleGrammar grammar)
        {
            bool all = false;
            String name;
            Token t = null;
            t = jj_consume_token(Import);
            jj_consume_token(28);
            name = Name();
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case 29:
                    jj_consume_token(29);
                    jj_consume_token(30);
                    all = true;
                    break;
                default:
                    jj_la1[5] = jj_gen;
                    break;
            }
            jj_consume_token(31);
            jj_consume_token(26);
            // import all rules if .*
            if (all) name = name + ".*";
            JSGFRuleName r = new JSGFRuleName(name);
            if (grammar != null)
            {
                grammar.AddImport(r);
                if (grammar is JSGFRuleGrammar && t != null && t.SpecialToken != null)
                {
                    if (t.SpecialToken.Image != null && t.SpecialToken.Image.StartsWith("/**"))
                    {
                        JSGFRuleGrammar JG = grammar;
                        JG.AddImportDocComment(r, t.SpecialToken.Image);
                    }
                }
            }
        }


        public String Name()
        {
            Token t1, t2;
            StringBuilder sb = new StringBuilder();
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Identifier:
                    t1 = jj_consume_token(Identifier);
                    break;
                case Public:
                    t1 = jj_consume_token(Public);
                    break;
                case Import:
                    t1 = jj_consume_token(Import);
                    break;
                case Grammar:
                    t1 = jj_consume_token(Grammar);
                    break;
                default:
                    jj_la1[6] = jj_gen;
                    jj_consume_token(-1);
                    throw new ParseException();
            }
            sb.Append(t1.Image);
        label_3:
            while (true)
            {
                if (jj_2_1(2))
                {

                }
                else
                {
                    break;
                }
                jj_consume_token(29);
                t2 = jj_consume_token(Identifier);
                sb.Append('.');
                sb.Append(t2.Image);
            }
            { if (true) return sb.ToString(); }
        }

        public void RuleDeclaration(JSGFRuleGrammar grammar)
        {
            bool pub = false;
            String s;
            JSGFRule r;
            Token t = null;
            Token t1 = null;
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Public:
                    t = jj_consume_token(Public);
                    pub = true;
                    break;
                default:
                    jj_la1[7] = jj_gen;
                    break;
            }
            t1 = jj_consume_token(28);
            s = RuleDef();
            jj_consume_token(31);
            jj_consume_token(32);
            r = Alternatives();
            jj_consume_token(26);
            try
            {
                if (grammar != null)
                {
                    grammar.SetRule(s, r, pub);
                    String docComment = null;
                    if ((t != null) && (t.SpecialToken != null) && (t.SpecialToken.Image != null)) docComment = t.SpecialToken.Image;
                    else if ((t1 != null) && (t1.SpecialToken != null) && (t1.SpecialToken.Image != null)) docComment = t1.SpecialToken.Image;
                    if (docComment != null && docComment.StartsWith("/**"))
                    {
                        ExtractKeywords(grammar, s, docComment);
                        grammar.AddRuleDocComment(s, docComment);
                    }
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("ERROR SETTING JSGFRule " + s);
            }
        }


        public JSGFRuleAlternatives Alternatives()
        {
            var ruleList = new List<JSGFRule>();
            JSGFRule r;
            float w;
            List<Float> weights = new List<Float>();
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Grammar:
                case Import:
                case Public:
                case IntegerLiteral:
                case FloatingPointLiteral:
                case StringLiteral:
                case Identifier:
                case 28:
                case 36:
                case 38:
                    r = Sequence();
                    ruleList.Add(r);
                //label_4:
                    var label_4 = false;
                    while (true)
                    {
                        switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                        {
                            case 33:
                                ;
                                break;
                            default:
                                jj_la1[8] = jj_gen;
                                label_4 = true; break; //goto label_4;
                        }
                        if (label_4) break;
                        jj_consume_token(33);
                        r = Sequence();
                        ruleList.Add(r);
                    }
                    break;
                case 34:
                    w = Weight();
                    r = Sequence();
                    ruleList.Add(r);
                    weights.Add(w);
                //label_5:
                    var label_5 = false;
                    while (true)
                    {
                        jj_consume_token(33);
                        w = Weight();
                        r = Sequence();
                        ruleList.Add(r);
                        weights.Add(w);
                        switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                        {
                            case 33:
                                ;
                                break;
                            default:
                                jj_la1[9] = jj_gen;
                                label_5 = true; break;
                        }
                        if (label_5) break;
                    }
                    break;
                default:
                    jj_la1[10] = jj_gen;
                    jj_consume_token(-1);
                    throw new ParseException();
            }
            JSGFRuleAlternatives ra = new JSGFRuleAlternatives(ruleList);
            if (weights.Count > 0)
            {
                ra.SetWeights(weights);
            }
            { if (true) return ra; }
        }


        public String RuleDef()
        {
            Token t;
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Identifier:
                    t = jj_consume_token(Identifier);
                    break;
                case IntegerLiteral:
                    t = jj_consume_token(IntegerLiteral);
                    break;
                case Public:
                    t = jj_consume_token(Public);
                    break;
                case Import:
                    t = jj_consume_token(Import);
                    break;
                case Grammar:
                    t = jj_consume_token(Grammar);
                    break;
                default:
                    jj_la1[11] = jj_gen;
                    jj_consume_token(-1);
                    throw new ParseException();
            }
            { if (true) return t.Image; }
        }


        public JSGFRuleSequence Sequence()
        {
            JSGFRule JSGFRule;
            var ruleList = new List<JSGFRule>();
        //label_6:
            var label_6 = false;
            while (true)
            {
                JSGFRule = Item();
                ruleList.Add(JSGFRule);
                switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                {
                    case Grammar:
                    case Import:
                    case Public:
                    case IntegerLiteral:
                    case FloatingPointLiteral:
                    case StringLiteral:
                    case Identifier:
                    case 28:
                    case 36:
                    case 38:
                        ;
                        break;
                    default:
                        jj_la1[12] = jj_gen;
                        label_6 = true; break; //goto label_6;
                }
                if (label_6) break;
            }
            { if (true) return new JSGFRuleSequence(ruleList); }
        }


        public float Weight()
        {
            Token t;
            jj_consume_token(34);
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case FloatingPointLiteral:
                    t = jj_consume_token(FloatingPointLiteral);
                    break;
                case IntegerLiteral:
                    t = jj_consume_token(IntegerLiteral);
                    break;
                default:
                    jj_la1[13] = jj_gen;
                    jj_consume_token(-1);
                    throw new ParseException();
            }
            jj_consume_token(34);
            { if (true) return Convert.ToSingle(t.Image, CultureInfo.InvariantCulture.NumberFormat); }
        }


        public JSGFRule Item()
        {
            JSGFRule r;
            List<String> _tags = null;
            int count = -1;
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Grammar:
                case Import:
                case Public:
                case IntegerLiteral:
                case FloatingPointLiteral:
                case StringLiteral:
                case Identifier:
                case 28:
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case Grammar:
                        case Import:
                        case Public:
                        case IntegerLiteral:
                        case FloatingPointLiteral:
                        case StringLiteral:
                        case Identifier:
                            r = Terminal();
                            break;
                        case 28:
                            r = RuleRef();
                            break;
                        default:
                            jj_la1[14] = jj_gen;
                            jj_consume_token(-1);
                            throw new ParseException();
                    }
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case 30:
                        case 35:
                            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                            {
                                case 30:
                                    jj_consume_token(30);
                                    count = JSGFRuleCount.ZeroOrMore;
                                    break;
                                case 35:
                                    jj_consume_token(35);
                                    count = JSGFRuleCount.OnceOrMore;
                                    break;
                                default:
                                    jj_la1[15] = jj_gen;
                                    jj_consume_token(-1);
                                    throw new ParseException();
                            }
                            break;
                        default:
                            jj_la1[16] = jj_gen;
                            break;
                    }
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case Tag:
                            _tags = Tags();
                            break;
                        default:
                            jj_la1[17] = jj_gen;
                            break;
                    }
                    break;
                case 36:
                    jj_consume_token(36);
                    r = Alternatives();
                    jj_consume_token(37);
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case 30:
                        case 35:
                            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                            {
                                case 30:
                                    jj_consume_token(30);
                                    count = JSGFRuleCount.ZeroOrMore;
                                    break;
                                case 35:
                                    jj_consume_token(35);
                                    count = JSGFRuleCount.OnceOrMore;
                                    break;
                                default:
                                    jj_la1[18] = jj_gen;
                                    jj_consume_token(-1);
                                    throw new ParseException();
                            }
                            break;
                        default:
                            jj_la1[19] = jj_gen;
                            break;
                    }
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case Tag:
                            _tags = Tags();
                            break;
                        default:
                            jj_la1[20] = jj_gen;
                            break;
                    }
                    break;
                case 38:
                    jj_consume_token(38);
                    r = Alternatives();
                    jj_consume_token(39);
                    count = JSGFRuleCount.Optional;
                    switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                    {
                        case Tag:
                            _tags = Tags();
                            break;
                        default:
                            jj_la1[21] = jj_gen;
                            break;
                    }
                    break;
                default:
                    jj_la1[22] = jj_gen;
                    jj_consume_token(-1);
                    throw new ParseException();
            }
            if (count != -1) r = new JSGFRuleCount(r, count);
            if (_tags != null)
            {
                for (int i = 0; i < _tags.Count; i++)
                {
                    //TODO: Check Behaviour
                    var tag = _tags[i];
                    if (tag[0] == '{')
                    {
                        tag = tag.Substring(1, tag.Length - 1);
                        tag = tag.Replace('\\', ' ');
                    }
                    r = new JSGFRuleTag(r, tag);
                }
            }
            { if (true) return r; }
        }


        public List<String> Tags()
        {
            Token token;
            List<String> tags = new List<String>();
        //label_7:
            var label_7 = false;
            while (true)
            {
                token = jj_consume_token(Tag);
                tags.Add(token.Image);
                switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
                {
                    case Tag:
                        ;
                        break;
                    default:
                        jj_la1[23] = jj_gen;
                        label_7=true;break;
                }
                if(label_7)break;
            }
            { if (true) return tags; }
        }

        public JSGFRule Terminal()
        {
            Token t;
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case Identifier:
                    t = jj_consume_token(Identifier);
                    break;
                case StringLiteral:
                    t = jj_consume_token(StringLiteral);
                    break;
                case IntegerLiteral:
                    t = jj_consume_token(IntegerLiteral);
                    break;
                case FloatingPointLiteral:
                    t = jj_consume_token(FloatingPointLiteral);
                    break;
                case Public:
                    t = jj_consume_token(Public);
                    break;
                case Import:
                    t = jj_consume_token(Import);
                    break;
                case Grammar:
                    t = jj_consume_token(Grammar);
                    break;
                default:
                    jj_la1[24] = jj_gen;
                    jj_consume_token(-1);
                    throw new ParseException();
            }
            String tn = t.Image;
            if (tn.StartsWith("\"") && tn.EndsWith("\"")) tn = tn.Substring(1, tn.Length - 1);
            JSGFRuleToken rt = new JSGFRuleToken(tn);
            { if (true) return rt; }
        }


        public JSGFRuleName RuleRef()
        {
            String s;
            jj_consume_token(28);
            s = Name();
            jj_consume_token(31);
            JSGFRuleName rn = new JSGFRuleName(s);
            { if (true) return rn; }
        }


        public JSGFRuleName ImportRef()
        {
            String s;
            bool all = false;
            jj_consume_token(28);
            s = Name();
            switch ((_jj_ntk == -1) ? jj_ntk() : _jj_ntk)
            {
                case 29:
                    jj_consume_token(29);
                    jj_consume_token(30);
                    all = true;
                    break;
                default:
                    jj_la1[25] = jj_gen;
                    break;
            }
            jj_consume_token(31);
            if (all) s = s + ".*";
            JSGFRuleName rn = new JSGFRuleName(s);
            { if (true) return rn; }
        }

        private bool jj_2_1(int xla)
        {
            jj_la = xla; jj_lastpos = jj_scanpos = token;
            try { return !jj_3_1(); }
            catch (LookaheadSuccess ls) { return true; }
            finally { jj_save(0, xla); }
        }

        private bool jj_3_1()
        {
            if (jj_scan_token(29)) return true;
            if (jj_scan_token(Identifier)) return true;
            return false;
        }

        /** Generated Token Manager. */
        public JSGFParserTokenManager token_source;
        readonly JavaCharStream jj_input_stream;
        /** Current token. */
        public Token token;
        /** Next token. */
        public Token jj_nt;
        private int _jj_ntk;
        private Token jj_scanpos, jj_lastpos;
        private int jj_la;
        private int jj_gen;
        readonly private int[] jj_la1 = new int[26];
        static private int[] jj_la1_0;
        static private int[] jj_la1_1;
        static JSGFParser()
        {
            jj_la1_init_0();
            jj_la1_init_1();
        }
        private static void jj_la1_init_0()
        {
            jj_la1_0 = new int[] { 0x800000, 0x4000, 0x10008000, 0x800000, 0x800000, 0x20000000, 0x80e000, 0x8000, 0x0, 0x0, 0x10a5e000, 0x81e000, 0x10a5e000, 0x50000, 0x10a5e000, 0x40000000, 0x40000000, 0x400000, 0x40000000, 0x40000000, 0x400000, 0x400000, 0x10a5e000, 0x400000, 0xa5e000, 0x20000000, };
        }
        private static void jj_la1_init_1()
        {
            jj_la1_1 = new int[] { 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x2, 0x2, 0x54, 0x0, 0x50, 0x0, 0x0, 0x8, 0x8, 0x0, 0x8, 0x8, 0x0, 0x0, 0x50, 0x0, 0x0, 0x0, };
        }
        readonly private JJCalls[] jj_2_rtns = new JJCalls[1];
        private bool jj_rescan;
        private int jj_gc;

        /** Constructor with InputStream. */
        public JSGFParser(Stream stream)
            : this(stream, null)
        {
        }
        /** Constructor with InputStream and supplied encoding */
        public JSGFParser(Stream stream, String encoding)
        {
            try { jj_input_stream = new JavaCharStream(stream, encoding, 1, 1); }
            catch (UnsupportedEncodingException e) { throw new RuntimeException(e); }
            token_source = new JSGFParserTokenManager(jj_input_stream);
            token = new Token();
            _jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 26; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /** Reinitialize. */
        public void ReInit(Stream stream)
        {
            ReInit(stream, null);
        }
        /** Reinitialize. */
        public void ReInit(Stream stream, String encoding)
        {
            try { jj_input_stream.ReInit(stream, encoding, 1, 1); }
            catch (UnsupportedEncodingException e) { throw new RuntimeException(e); }
            token_source.ReInit(jj_input_stream);
            token = new Token();
            _jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 26; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /** Constructor. */
        public JSGFParser(TextReader stream)
        {
            jj_input_stream = new JavaCharStream(stream, 1, 1);
            token_source = new JSGFParserTokenManager(jj_input_stream);
            token = new Token();
            _jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 26; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /** Reinitialize. */
        public void ReInit(TextReader stream)
        {
            jj_input_stream.ReInit(stream, 1, 1);
            token_source.ReInit(jj_input_stream);
            token = new Token();
            _jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 26; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /** Constructor with generated Token Manager. */
        public JSGFParser(JSGFParserTokenManager tm)
        {
            token_source = tm;
            token = new Token();
            _jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 26; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        /** Reinitialize. */
        public void ReInit(JSGFParserTokenManager tm)
        {
            token_source = tm;
            token = new Token();
            _jj_ntk = -1;
            jj_gen = 0;
            for (int i = 0; i < 26; i++) jj_la1[i] = -1;
            for (int i = 0; i < jj_2_rtns.Length; i++) jj_2_rtns[i] = new JJCalls();
        }

        private Token jj_consume_token(int kind)
        {
            Token oldToken;
            if ((oldToken = token).Next != null) token = token.Next;
            else token = token.Next = token_source.GetNextToken();
            _jj_ntk = -1;
            if (token.Kind == kind)
            {
                jj_gen++;
                if (++jj_gc > 100)
                {
                    jj_gc = 0;
                    for (int i = 0; i < jj_2_rtns.Length; i++)
                    {
                        JJCalls c = jj_2_rtns[i];
                        while (c != null)
                        {
                            if (c.Gen < jj_gen) c.First = null;
                            c = c.Next;
                        }
                    }
                }
                return token;
            }
            token = oldToken;
            _jjKind = kind;
            throw GenerateParseException();
        }


        private class LookaheadSuccess : Error { }
        readonly private LookaheadSuccess jj_ls = new LookaheadSuccess();
        private bool jj_scan_token(int kind)
        {
            if (jj_scanpos == jj_lastpos)
            {
                jj_la--;
                if (jj_scanpos.Next == null)
                {
                    jj_lastpos = jj_scanpos = jj_scanpos.Next = token_source.GetNextToken();
                }
                else
                {
                    jj_lastpos = jj_scanpos = jj_scanpos.Next;
                }
            }
            else
            {
                jj_scanpos = jj_scanpos.Next;
            }
            if (jj_rescan)
            {
                int i = 0; Token tok = token;
                while (tok != null && tok != jj_scanpos) { i++; tok = tok.Next; }
                if (tok != null) jj_add_error_token(kind, i);
            }
            if (jj_scanpos.Kind != kind) return true;
            if (jj_la == 0 && jj_scanpos == jj_lastpos) throw jj_ls;
            return false;
        }


        /** Get the next Token. */
        public Token GetNextToken()
        {
            if (token.Next != null) token = token.Next;
            else token = token.Next = token_source.GetNextToken();
            _jj_ntk = -1;
            jj_gen++;
            return token;
        }

        /** Get the specific Token. */
        public Token GetToken(int index)
        {
            Token t = token;
            for (int i = 0; i < index; i++)
            {
                if (t.Next != null) t = t.Next;
                else t = t.Next = token_source.GetNextToken();
            }
            return t;
        }

        private int jj_ntk()
        {
            if ((jj_nt = token.Next) == null)
                return (_jj_ntk = (token.Next = token_source.GetNextToken()).Kind);
            else
                return (_jj_ntk = jj_nt.Kind);
        }

        private readonly List<int[]> _jjExpentries = new List<int[]>();
        private int[] _jjExpentry;
        private int _jjKind = -1;
        private readonly int[] _jjLasttokens = new int[100];
        private int _jjEndpos;

        private void jj_add_error_token(int kind, int pos)
        {
            if (pos >= 100) return;
            if (pos == _jjEndpos + 1)
            {
                _jjLasttokens[_jjEndpos++] = kind;
            }
            else if (_jjEndpos != 0)
            {
                _jjExpentry = new int[_jjEndpos];
                for (int i = 0; i < _jjEndpos; i++)
                {
                    _jjExpentry[i] = _jjLasttokens[i];
                }
            jj_entries_loop: for (var it = _jjExpentries.GetEnumerator(); it.MoveNext(); )
                {
                    var continueFlag = false;
                    int[] oldentry = it.Current;
                    if (oldentry.Length == _jjExpentry.Length)
                    {
                        for (int i = 0; i < _jjExpentry.Length; i++)
                        {
                            if (oldentry[i] != _jjExpentry[i])
                            {
                                continueFlag = true;
                                break;
                                //continue jj_entries_loop;
                            }
                        }
                        if (continueFlag) continue;

                        _jjExpentries.Add(_jjExpentry);
                        goto jj_entries_loop;
                    }
                }
                if (pos != 0) _jjLasttokens[(_jjEndpos = pos) - 1] = kind;
            }
        }

        /** Generate ParseException. */
        public ParseException GenerateParseException()
        {
            _jjExpentries.Clear();
            bool[] la1tokens = new bool[40];
            if (_jjKind >= 0)
            {
                la1tokens[_jjKind] = true;
                _jjKind = -1;
            }
            for (int i = 0; i < 26; i++)
            {
                if (jj_la1[i] == jj_gen)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        if ((jj_la1_0[i] & (1 << j)) != 0)
                        {
                            la1tokens[j] = true;
                        }
                        if ((jj_la1_1[i] & (1 << j)) != 0)
                        {
                            la1tokens[32 + j] = true;
                        }
                    }
                }
            }
            for (int i = 0; i < 40; i++)
            {
                if (la1tokens[i])
                {
                    _jjExpentry = new int[1];
                    _jjExpentry[0] = i;
                    _jjExpentries.Add(_jjExpentry);
                }
            }
            _jjEndpos = 0;
            jj_rescan_token();
            jj_add_error_token(0, 0);
            int[][] exptokseq = new int[_jjExpentries.Count][];
            for (int i = 0; i < _jjExpentries.Count; i++)
            {
                exptokseq[i] = _jjExpentries[i];
            }
            return new ParseException(token, exptokseq, TokenImage);
        }

        /** Enable tracing. */
        public void enable_tracing()
        {
        }

        /** Disable tracing. */
        public void disable_tracing()
        {
        }

        private void jj_rescan_token()
        {
            jj_rescan = true;
            for (int i = 0; i < 1; i++)
            {
                try
                {
                    JJCalls p = jj_2_rtns[i];
                    do
                    {
                        if (p.Gen > jj_gen)
                        {
                            jj_la = p.Arg; jj_lastpos = jj_scanpos = p.First;
                            switch (i)
                            {
                                case 0: jj_3_1(); break;
                            }
                        }
                        p = p.Next;
                    } while (p != null);
                }
                catch (LookaheadSuccess ls) { }
            }
            jj_rescan = false;
        }

        private void jj_save(int index, int xla)
        {
            JJCalls p = jj_2_rtns[index];
            while (p.Gen > jj_gen)
            {
                if (p.Next == null) { p = p.Next = new JJCalls(); break; }
                p = p.Next;
            }
            p.Gen = jj_gen + xla - jj_la; p.First = token; p.Arg = xla;
        }

        class JJCalls
        {
            internal int Gen;
            internal Token First;
            internal int Arg;
            internal JJCalls Next;
        }

    }

}
