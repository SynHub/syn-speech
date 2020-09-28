using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Syn.Speech.Alignment.Tokenizer;
using Syn.Speech.Helper;
using Syn.Speech.Properties;
//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    /// <summary>
    /// Converts the Tokens (in US English words) in an Utterance into a list of
    /// words. It puts the produced list back into the Utterance. Usually, the
    /// tokens that gets expanded are numbers like "23" (to "twenty" "three").
    /// <p/>
    /// /// It translates the following code from flite: <br/>
    /// <code>
    /// lang/usenglish/us_text.c
    /// </code>
    /// </summary>
    public class UsEnglishTokenizer : ITextTokenizer
    {
        private static readonly Pattern AlphabetPattern = Pattern.Compile(UsEnglish.RxAlphabet);
        private static readonly Pattern CommaIntPattern = Pattern.Compile(UsEnglish.RxCommaint);
        private static readonly Pattern Digits2DashPattern = Pattern.Compile("[0-9]+(-[0-9]+)(-[0-9]+)+");
        private static readonly Pattern DigitsPattern = Pattern.Compile(UsEnglish.RxDigits);
        private static readonly Pattern DigitsSlashDigitsPattern = Pattern.Compile("[0-9]+/[0-9]+");
        private static readonly Pattern DottedAbbrevPattern = Pattern.Compile(UsEnglish.RxDottedAbbrev);
        private static readonly Pattern DoublePattern = Pattern.Compile(UsEnglish.RxDouble);
        private static readonly Pattern DrStPattern = Pattern.Compile("([dD][Rr]|[Ss][Tt])");
        private static readonly Pattern FourDigitsPattern = Pattern.Compile("[0-9][0-9][0-9][0-9]");
        private static readonly Pattern IllionPattern;
        private static readonly Pattern NumberTimePattern;
        private static readonly Pattern NumessPattern;
        private static readonly Pattern OrdinalPattern;
        private static readonly Pattern RomanNumbersPattern;
        private static readonly Pattern SevenPhoneNumberPattern;
        private static readonly Pattern ThreeDigitsPattern;
        private static readonly Pattern UsMoneyPattern;

        private static readonly HashMap<string, string> KingSectionLikeMap = new HashMap<string, string>();
        private const string KingNames = "kingNames";
        private const string KingTitles = "kingTitles";
        private const string SectionTypes = "sectionTypes";

        private static readonly HashMap<string, string[]> UsStatesMap = new HashMap<string, string[]>();
        private WordRelation _wordRelation;
        private Item _tokenItem;
        private readonly DecisionTree _cart;

        // King-like words
        private static readonly string[] kingNames = {"louis", "henry", "charles",
            "philip", "george", "edward", "pius", "william", "richard",
            "ptolemy", "john", "paul", "peter", "nicholas", "frederick",
            "james", "alfonso", "ivan", "napoleon", "leo", "gregory",
            "catherine", "alexandria", "pierre", "elizabeth", "mary", "elmo",
            "erasmus"};

        private static readonly string[] kingTitles = {"king", "queen", "pope",
            "duke", "tsar", "emperor", "shah", "caesar", "duchess", "tsarina",
            "empress", "baron", "baroness", "sultan", "count", "countess"};

        // Section-like words
        private static readonly string[] sectionTypes = {"section", "chapter",
            "part", "phrase", "verse", "scene", "act", "book", "volume",
            "chap", "war", "apollo", "trek", "fortran"};

        private readonly PronounceableFsm prefixFSM;
        private readonly PronounceableFsm suffixFSM;
        // List of US states abbreviations and their full names
        private static readonly string[][] usStates = {
            new[] {"AL", "ambiguous", "alabama"}, new[] {"Al", "ambiguous", "alabama"}, 
            new[] {"Ala", "", "alabama"}, new[] {"AK", "", "alaska"}, new[] {"Ak", "", "alaska"},
            new[] {"AZ", "", "arizona"}, new[] {"Az", "", "arizona"}, new[] {"CA", "", "california"}, 
            new[] {"Ca", "", "california"}, new[] {"Cal", "ambiguous", "california"},
            new[] {"Calif", "", "california"}, new[] {"CO", "ambiguous", "colorado"}, new[] {"Co", "ambiguous", "colorado"}, 
            new[] {"Colo", "", "colorado"}, new[] {"DC", "", "d", "c"}, new[] {"DE", "", "delaware"}, 
            new[] {"De", "ambiguous", "delaware"}, new[] {"Del", "ambiguous", "delaware"}, 
            new[] {"FL", "", "florida"}, new[] {"Fl", "ambiguous", "florida"}, new[] {"Fla", "", "florida"},
            new[] {"GA", "", "georgia"}, new[] {"Ga", "", "georgia"}, new[] {"HI", "ambiguous", "hawaii"}, 
            new[] {"Hi", "ambiguous", "hawaii"}, new[] {"IA", "", "iowa"}, new[] {"Ia", "ambiguous", "iowa"}, 
            new[] {"IN", "ambiguous", "indiana"}, new[] {"In", "ambiguous", "indiana"}, new[] {"Ind", "ambiguous", "indiana"}, 
            new[] {"ID", "ambiguous", "idaho"}, new[] {"IL", "ambiguous", "illinois"}, 
            new[] {"Il", "ambiguous", "illinois"}, new[] {"ILL", "ambiguous", "illinois"}, 
            new[] {"KS", "", "kansas"}, new[] {"Ks", "", "kansas"}, 
            new[] {"Kans", "", "kansas"}, new[] {"KY", "ambiguous", "kentucky"},
            new[] {"Ky", "ambiguous", "kentucky"}, new[] {"LA", "ambiguous", "louisiana"},
            new[] {"La", "ambiguous", "louisiana"}, 
            new[] {"Lou", "ambiguous", "louisiana"},
            new[] {"Lous", "ambiguous", "louisiana"}, 
            new[] {"MA", "ambiguous", "massachusetts"}, 
            new[] {"Mass", "ambiguous", "massachusetts"},
            new[] {"Ma", "ambiguous", "massachusetts"}, new[] {"MD", "ambiguous", "maryland"}, 
            new[] {"Md", "ambiguous", "maryland"}, new[] {"ME", "ambiguous", "maine"}, 
            new[] {"Me", "ambiguous", "maine"}, new[] {"MI", "", "michigan"}, new[] {"Mi", "ambiguous", "michigan"}, 
            new[] {"Mich", "ambiguous", "michigan"}, new[] {"MN", "ambiguous", "minnestota"},
            new[] {"Minn", "ambiguous", "minnestota"}, new[] {"MS", "ambiguous", "mississippi"}, 
            new[] {"Miss", "ambiguous", "mississippi"}, new[] {"MT", "ambiguous", "montanna"}, 
            new[] {"Mt", "ambiguous", "montanna"}, new[] {"MO", "ambiguous", "missouri"},
            new[] {"Mo", "ambiguous", "missouri"}, new[] {"NC", "ambiguous", "north", "carolina"}, 
            new[] {"ND", "ambiguous", "north", "dakota"}, new[] {"NE", "ambiguous", "nebraska"}, 
            new[] {"Ne", "ambiguous", "nebraska"}, new[] {"Neb", "ambiguous", "nebraska"},
            new[] {"NH", "ambiguous", "new", "hampshire"}, new[] {"NV", "", "nevada"}, 
            new[] {"Nev", "", "nevada"}, new[] {"NY", "", "new", "york"}, 
            new[] {"OH", "ambiguous", "ohio"}, new[] {"OK", "ambiguous", "oklahoma"}, 
            new[] {"Okla", "", "oklahoma"}, new[] {"OR", "ambiguous", "oregon"}, 
            new[] {"Or", "ambiguous", "oregon"}, new[] {"Ore", "ambiguous", "oregon"}, 
            new[] {"PA", "ambiguous", "pennsylvania"}, new[] {"Pa", "ambiguous", "pennsylvania"}, 
            new[] {"Penn", "ambiguous", "pennsylvania"}, new[] {"RI", "ambiguous", "rhode", "island"}, 
            new[] {"SC", "ambiguous", "south", "carlolina"}, new[] {"SD", "ambiguous", "south", "dakota"}, 
            new[] {"TN", "ambiguous", "tennesee"}, new[] {"Tn", "ambiguous", "tennesee"}, 
            new[] {"Tenn", "ambiguous", "tennesee"}, new[] {"TX", "ambiguous", "texas"}, 
            new[] {"Tx", "ambiguous", "texas"}, new[] {"Tex", "ambiguous", "texas"}, 
            new[] {"UT", "ambiguous", "utah"}, new[] {"VA", "ambiguous", "virginia"}, 
            new[] {"WA", "ambiguous", "washington"}, new[] {"Wa", "ambiguous", "washington"}, 
            new[] {"Wash", "ambiguous", "washington"}, new[] {"WI", "ambiguous", "wisconsin"},
            new[] {"Wi", "ambiguous", "wisconsin"}, new[] {"WV", "ambiguous", "west", "virginia"}, 
            new[] {"WY", "ambiguous", "wyoming"}, new[] {"Wy", "ambiguous", "wyoming"}, 
            new[] {"Wyo", "", "wyoming"}, new[] {"PR", "ambiguous", "puerto", "rico"}
        };


        static UsEnglishTokenizer()
        {
            AlphabetPattern = Pattern.Compile(UsEnglish.RxAlphabet);
            CommaIntPattern = Pattern.Compile(UsEnglish.RxCommaint);
            Digits2DashPattern = Pattern.Compile(UsEnglish.RxDigits2Dash);
            DigitsPattern = Pattern.Compile(UsEnglish.RxDigits);
            DigitsSlashDigitsPattern = Pattern.Compile(UsEnglish.RxDigitsslashdigits);
            DottedAbbrevPattern = Pattern.Compile(UsEnglish.RxDottedAbbrev);
            DoublePattern = Pattern.Compile(UsEnglish.RxDouble);
            DrStPattern = Pattern.Compile(UsEnglish.RxDrst);
            FourDigitsPattern = Pattern.Compile(UsEnglish.RxFourDigit);
            Pattern.Compile(UsEnglish.RxHasVowel);
            IllionPattern = Pattern.Compile(UsEnglish.RxIllion);
            NumberTimePattern = Pattern.Compile(UsEnglish.RxNumberTime);
            NumessPattern = Pattern.Compile(UsEnglish.RxNumess);
            OrdinalPattern = Pattern.Compile(UsEnglish.RxOrdinalNumber);
            RomanNumbersPattern = Pattern.Compile(UsEnglish.RxRomanNumber);
            SevenPhoneNumberPattern = Pattern.Compile(UsEnglish.RxSevenDigitPhoneNumber);
            ThreeDigitsPattern = Pattern.Compile(UsEnglish.RxThreeDigit);
            UsMoneyPattern = Pattern.Compile(UsEnglish.RxUsMoney);

            for (int i = 0; i < kingNames.Length; i++)
            {
                KingSectionLikeMap.Put(kingNames[i], KingNames);
            }
            for (int i = 0; i < kingTitles.Length; i++)
            {
                KingSectionLikeMap.Put(kingTitles[i], KingTitles);
            }
            for (int i = 0; i < sectionTypes.Length; i++)
            {
                KingSectionLikeMap.Put(sectionTypes[i], SectionTypes);
            }

            // Again map for constant time searching.
            for (int i = 0; i < usStates.Length; i++)
            {
                UsStatesMap.Put(usStates[i][0], usStates[i]);
            }
        }

        /// <summary>
        /// Constructs a default USTokenWordProcessor. It uses the USEnglish regular
        /// expression set (USEngRegExp) by default.
        /// </summary>
        /// <exception cref="IllegalStateException">The cart to use to classify numbers.</exception>
        public UsEnglishTokenizer()
        {
            try
            {
                _cart = new DecisionTree(Resources.nums_cart);
                prefixFSM = new PrefixFsm(Resources.prefix_fsm);
                suffixFSM = new SuffixFsm(Resources.suffix_fsm);

                //cart = new DecisionTree(getClass().getResource("nums_cart.txt"));
                //prefixFSM = new PrefixFSM(getClass().getResource("prefix_fsm.txt"));
                //suffixFSM = new SuffixFSM(getClass().getResource("suffix_fsm.txt"));
            }
            catch (IOException)
            {
                throw new IllegalStateException("resources not found");
            }
        }

        /// <summary>
        /// Returns the currently processing token Item.
        /// </summary>
        /// <returns>The current token Item; null if no item</returns>
        public virtual Item GetTokenItem()
        {
            return _tokenItem;
        }

        /// <summary>
        /// process the utterance
        /// </summary>
        /// <param name="text">The text.</param>
        /// <exception cref="IllegalStateException"></exception>
        /// <returns>The utterance contain the tokens</returns>
        public virtual List<string> Expand(string text)
        {

            string simplifiedText = SimplifyChars(text);

            CharTokenizer tokenizer = new CharTokenizer();
            tokenizer.WhitespaceSymbols = UsEnglish.WhitespaceSymbols;
            tokenizer.SingleCharSymbols = UsEnglish.SingleCharSymbols;
            tokenizer.PrepunctuationSymbols = UsEnglish.PrePunctuationSymbols;
            tokenizer.PostpunctuationSymbols = UsEnglish.PunctuationSymbols;
            tokenizer.SetInputText(simplifiedText);
            Utterance utterance = new Utterance(tokenizer);

            Relation tokenRelation;
            if ((tokenRelation = utterance.GetRelation(Relation.Token)) == null)
            {
                throw new IllegalStateException("token relation does not exist");
            }

            _wordRelation = WordRelation.CreateWordRelation(utterance, this);

            for (_tokenItem = tokenRelation.Head; _tokenItem != null; _tokenItem =
                    _tokenItem.GetNext())
            {

                FeatureSet featureSet = _tokenItem.Features;
                string tokenVal = featureSet.GetString("name");

                // convert the token into a list of words
                TokenToWords(tokenVal);
            }

            List<string> words = new List<string>();
            for (Item item = utterance.GetRelation(Relation.Word).Head; item != null; item =
                    item.GetNext())
            {
                if (!string.IsNullOrEmpty(item.ToString()) && !item.ToString().Contains("#"))
                {
                    words.Add(item.ToString());
                }
            }
            return words;
        }

        private static string SimplifyChars(string text)
        {
            text = text.Replace('’', '\'');
            text = text.Replace('‘', '\'');
            text = text.Replace('”', '"');
            text = text.Replace('“', '"');
            text = text.Replace('»', '"');
            text = text.Replace('«', '"');
            text = text.Replace('–', '-');
            text = text.Replace('—', ' ');
            text = text.Replace('…', ' ');
            text = text.Replace((char)0xc, ' ');
            return text;
        }

        /// <summary>
        /// Returns true if the given token matches part of a phone number
        /// </summary>
        /// <param name="tokenVal">The token.</param>
        /// <returns>true or false</returns>
        private bool MatchesPartPhoneNumber(string tokenVal)
        {
            string n_name = (string)_tokenItem.FindFeature("n.name");
            string n_n_name = (string)_tokenItem.FindFeature("n.n.name");
            string p_name = (string)_tokenItem.FindFeature("p.name");
            string p_p_name = (string)_tokenItem.FindFeature("p.p.name");

            bool matches3DigitsP_name = Matches(ThreeDigitsPattern, p_name);

            return ((Matches(ThreeDigitsPattern, tokenVal) && ((!Matches(
                    DigitsPattern, p_name) && Matches(ThreeDigitsPattern, n_name) && Matches(
                        FourDigitsPattern, n_n_name))
                    || (Matches(SevenPhoneNumberPattern, n_name)) || (!Matches(
                    DigitsPattern, p_p_name) && matches3DigitsP_name && Matches(
                        FourDigitsPattern, n_name)))) || (Matches(
                    FourDigitsPattern, tokenVal) && (!Matches(DigitsPattern,
                    n_name) && matches3DigitsP_name && Matches(ThreeDigitsPattern,
                        p_p_name))));
        }

        /// <summary>
        /// Converts the given Token into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">the string value of the token, which may or may not be
        /// same as the one in called "name" in flite</param>
        private void TokenToWords(string tokenVal)
        {
            FeatureSet tokenFeatures = _tokenItem.Features;
            string itemName = tokenFeatures.GetString("name");
            int tokenLength = tokenVal.Length;

            if (tokenFeatures.IsPresent("phones"))
            {
                _wordRelation.AddWord(tokenVal);

            }
            else if ((tokenVal.Equals("a") || tokenVal.Equals("A"))
                  && ((_tokenItem.GetNext() == null)
                          || !(tokenVal.Equals(itemName)) || !(((string)_tokenItem
                              .FindFeature("punc")).Equals(""))))
            {
                /* if A is a sub part of a token, then its ey not ah */
                _wordRelation.AddWord("_a");

            }
            else if (Matches(AlphabetPattern, tokenVal))
            {

                if (Matches(RomanNumbersPattern, tokenVal))
                {

                    /* XVIII */
                    RomanToWords(tokenVal);

                }
                else if (Matches(IllionPattern, tokenVal)
                      && Matches(UsMoneyPattern,
                              (string)_tokenItem.FindFeature("p.name")))
                {
                    /* $ X -illion */
                    _wordRelation.AddWord(tokenVal);
                    _wordRelation.AddWord("dollars");

                }
                else if (Matches(DrStPattern, tokenVal))
                {
                    /* St Andrew's St, Dr King Dr */
                    DrStToWords(tokenVal);
                }
                else if (tokenVal.Equals("Mr"))
                {
                    _tokenItem.Features.SetString("punc", "");
                    _wordRelation.AddWord("mister");
                }
                else if (tokenVal.Equals("Mrs"))
                {
                    _tokenItem.Features.SetString("punc", "");
                    _wordRelation.AddWord("missus");
                }
                else if (tokenLength == 1
                      && char.IsUpper(tokenVal[0])
                      && ((string)_tokenItem.FindFeature("n.whitespace"))
                              .Equals(" ")
                      && char.IsUpper(((string)_tokenItem
                              .FindFeature("n.name"))[0]))
                {

                    tokenFeatures.SetString("punc", "");
                    string aaa = tokenVal.ToLower();
                    if (aaa.Equals("a"))
                    {
                        _wordRelation.AddWord("_a");
                    }
                    else
                    {
                        _wordRelation.AddWord(aaa);
                    }
                }
                else if (IsStateName(tokenVal))
                {
                    /*
                     * The name of a US state isStateName() has already added the
                     * full name of the state, so we're all set.
                     */
                }
                else if (tokenLength > 1 && !IsPronounceable(tokenVal))
                {
                    /* Need common exception list */
                    /* unpronouncable list of alphas */
                    NumberExpander.ExpandLetters(tokenVal, _wordRelation);

                }
                else
                {
                    /* just a word */
                    _wordRelation.AddWord(tokenVal.ToLower());
                }

            }
            else if (Matches(DottedAbbrevPattern, tokenVal))
            {

                /* U.S.A. */
                // remove all dots
                NumberExpander.ExpandLetters(tokenVal.Replace(".", ""),
                        _wordRelation);

            }
            else if (Matches(CommaIntPattern, tokenVal))
            {

                /* 99,999,999 */
                NumberExpander.ExpandReal(tokenVal.Replace(",", "").Replace("'", ""), _wordRelation);

            }
            else if (Matches(SevenPhoneNumberPattern, tokenVal))
            {

                /* 234-3434 telephone numbers */
                int dashIndex = tokenVal.IndexOf('-');
                string aaa = tokenVal.JSubString(0, dashIndex);
                string bbb = tokenVal.Substring(dashIndex + 1);

                NumberExpander.ExpandDigits(aaa, _wordRelation);
                _wordRelation.AddBreak();
                NumberExpander.ExpandDigits(bbb, _wordRelation);

            }
            else if (MatchesPartPhoneNumber(tokenVal))
            {

                /* part of a telephone number */
                var punctuation = (string)_tokenItem.FindFeature("punc");
                if (punctuation.Equals(""))
                {
                    _tokenItem.Features.SetString("punc", ",");
                }
                NumberExpander.ExpandDigits(tokenVal, _wordRelation);
                _wordRelation.AddBreak();

            }
            else if (Matches(NumberTimePattern, tokenVal))
            {
                /* 12:35 */
                int colonIndex = tokenVal.IndexOf(':');
                string aaa = tokenVal.JSubString(0, colonIndex);
                string bbb = tokenVal.Substring(colonIndex + 1);

                NumberExpander.ExpandNumber(aaa, _wordRelation);
                if (!(bbb.Equals("00")))
                {
                    NumberExpander.ExpandId(bbb, _wordRelation);
                }
            }
            else if (Matches(Digits2DashPattern, tokenVal))
            {
                /* 999-999-999 */
                DigitsDashToWords(tokenVal);
            }
            else if (Matches(DigitsPattern, tokenVal))
            {
                DigitsToWords(tokenVal);
            }
            else if (tokenLength == 1
                  && char.IsUpper(tokenVal[0])
                  && ((string)_tokenItem.FindFeature("n.whitespace"))
                          .Equals(" ")
                  && char.IsUpper(((string)_tokenItem
                          .FindFeature("n.name"))[0]))
            {

                tokenFeatures.SetString("punc", "");
                string aaa = tokenVal.ToLower();
                if (aaa.Equals("a"))
                {
                    _wordRelation.AddWord("_a");
                }
                else
                {
                    _wordRelation.AddWord(aaa);
                }
            }
            else if (Matches(DoublePattern, tokenVal))
            {
                NumberExpander.ExpandReal(tokenVal, _wordRelation);
            }
            else if (Matches(OrdinalPattern, tokenVal))
            {
                /* explicit ordinals */
                string aaa = tokenVal.JSubString(0, tokenLength - 2);
                NumberExpander.ExpandOrdinal(aaa, _wordRelation);
            }
            else if (Matches(UsMoneyPattern, tokenVal))
            {
                /* US money */
                UsMoneyToWords(tokenVal);
            }
            else if (tokenLength > 0 && tokenVal[tokenLength - 1] == '%')
            {
                /* Y% */
                TokenToWords(tokenVal.JSubString(0, tokenLength - 1));
                _wordRelation.AddWord("percent");
            }
            else if (Matches(NumessPattern, tokenVal))
            {
                NumberExpander.ExpandNumess(tokenVal.JSubString(0, tokenLength - 1), _wordRelation);
            }
            else if (Matches(DigitsSlashDigitsPattern, tokenVal) && tokenVal.Equals(itemName))
            {
                DigitsSlashDigitsToWords(tokenVal);
            }
            else if (tokenVal.IndexOf('-') != -1)
            {
                DashToWords(tokenVal);
            }
            else if (tokenLength > 1 && !Matches(AlphabetPattern, tokenVal))
            {
                NotJustAlphasToWords(tokenVal);
            }
            else if (tokenVal.Equals("&"))
            {
                // &
                _wordRelation.AddWord("and");
            }
            else if (tokenVal.Equals("-"))
            {
                // Skip it
            }
            else
            {
                // Just a word.
                _wordRelation.AddWord(tokenVal.ToLower());
            }
        }

        /// <summary>
        ///  Convert the given digit token with dashes (e.g. 999-999-999) into (word)
        /// Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The digit string.</param>
        private void DigitsDashToWords([In] string tokenVal)
        {
            int tokenLength = tokenVal.Length;
            int a = 0;
            for (int p = 0; p <= tokenLength; p++)
            {
                if (p == tokenLength || tokenVal[p] == '-')
                {
                    string aaa = tokenVal.JSubString(a, p);
                    NumberExpander.ExpandDigits(aaa, _wordRelation);
                    _wordRelation.AddBreak();
                    a = p + 1;
                }
            }
        }

        /// <summary>
        /// Convert the given digit token into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The digit string.</param>
        private void DigitsToWords(string tokenVal)
        {
            FeatureSet featureSet = _tokenItem.Features;
            string nsw = "";
            if (featureSet.IsPresent("nsw"))
            {
                nsw = featureSet.GetString("nsw");
            }

            if (nsw.Equals("nide"))
            {
                NumberExpander.ExpandId(tokenVal, _wordRelation);
            }
            else
            {
                string rName = featureSet.GetString("name");
                string digitsType = null;

                if (tokenVal.Equals(rName))
                {
                    digitsType = (string)_cart.Interpret(_tokenItem);
                }
                else
                {
                    featureSet.SetString("name", tokenVal);
                    digitsType = (string)_cart.Interpret(_tokenItem);
                    featureSet.SetString("name", rName);
                }

                if (digitsType.Equals("ordinal"))
                {
                    NumberExpander.ExpandOrdinal(tokenVal, _wordRelation);
                }
                else if (digitsType.Equals("digits"))
                {
                    NumberExpander.ExpandDigits(tokenVal, _wordRelation);
                }
                else if (digitsType.Equals("year"))
                {
                    NumberExpander.ExpandId(tokenVal, _wordRelation);
                }
                else
                {
                    NumberExpander.ExpandNumber(tokenVal, _wordRelation);
                }
            }
        }

        /// <summary>
        /// Converts the given Roman numeral string into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="romanString">The roman numeral string.</param>
        private void RomanToWords(string romanString)
        {
            string punctuation = (string)_tokenItem.FindFeature("p.punc");

            if (punctuation.Equals(""))
            {
                /* no preceeding punctuation */
                //string n = String.valueOf(NumberExpander.expandRoman(romanString));
                var n = NumberExpander.ExpandRoman(romanString).ToString(CultureInfo.InvariantCulture);

                if (KingLike(_tokenItem))
                {
                    _wordRelation.AddWord("the");
                    NumberExpander.ExpandOrdinal(n, _wordRelation);
                }
                else if (SectionLike(_tokenItem))
                {
                    NumberExpander.ExpandNumber(n, _wordRelation);
                }
                else
                {
                    NumberExpander.ExpandLetters(romanString, _wordRelation);
                }
            }
            else
            {
                NumberExpander.ExpandLetters(romanString, _wordRelation);
            }
        }

        /// <summary>
        /// Returns true if the given key is in the {@link #kingSectionLikeMap} map,
        /// and the value is the same as the given value.
        /// </summary>
        /// <param name="key">key to look for in the map.</param>
        /// <param name="value">the value to match.</param>
        /// <returns>true if it matches, or false if it does not or if the key is not
        /// mapped to any value in the map.</returns>
        private static bool InKingSectionLikeMap(string key, string value)
        {
            if (KingSectionLikeMap.ContainsKey(key))
            {
                return KingSectionLikeMap.Get(key).Equals(value);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given token item contains a token that is in a
        /// king-like context, e.g., "King" or "Louis".
        /// </summary>
        /// <param name="tokenItem">the token item to check.</param>
        /// <returns>true or false</returns>
        public static bool KingLike(Item tokenItem)
        {
            string kingName = ((string)tokenItem.FindFeature("p.name")).ToLower();
            if (InKingSectionLikeMap(kingName, KingNames))
            {
                return true;
            }
            else
            {
                string kingTitle = ((string)tokenItem.FindFeature("p.p.name")).ToLower();
                return InKingSectionLikeMap(kingTitle, KingTitles);
            }
        }

        /// <summary>
        /// Returns true if the given token item contains a token that is in a
        /// section-like context, e.g., "chapter" or "act".
        /// </summary>
        /// <param name="tokenItem">the token item to check.</param>
        /// <returns>true or false</returns>
        public static bool SectionLike(Item tokenItem)
        {
            string sectionType = ((string)tokenItem.FindFeature("p.name")).ToLower();
            return InKingSectionLikeMap(sectionType, SectionTypes);
        }

        /// <summary>
        /// Converts the given string containing "St" and "Dr" to (word) Items in the WordRelation.
        /// </summary>
        /// <param name="drStString">The string with "St" and "Dr".</param>
        private void DrStToWords(string drStString)
        {
            string street = null;
            string saint = null;
            char c0 = drStString[0];

            if (c0 == 's' || c0 == 'S')
            {
                street = "street";
                saint = "saint";
            }
            else
            {
                street = "drive";
                saint = "doctor";
            }

            FeatureSet featureSet = _tokenItem.Features;
            string punctuation = featureSet.GetString("punc");

            string featPunctuation = (string)_tokenItem.FindFeature("punc");

            if (_tokenItem.GetNext() == null || punctuation.IndexOf(',') != -1)
            {
                _wordRelation.AddWord(street);
            }
            else if (featPunctuation.Equals(","))
            {
                _wordRelation.AddWord(saint);
            }
            else
            {
                string pName = (string)_tokenItem.FindFeature("p.name");
                string nName = (string)_tokenItem.FindFeature("n.name");

                char p0 = pName[0];
                char n0 = nName[0];

                if (char.IsUpper(p0) && char.IsLower(n0))
                {
                    _wordRelation.AddWord(street);
                }
                else if (char.IsDigit(p0) && char.IsLower(n0))
                {
                    _wordRelation.AddWord(street);
                }
                else if (char.IsLower(p0) && char.IsUpper(n0))
                {
                    _wordRelation.AddWord(saint);
                }
                else
                {
                    string whitespace = (string)_tokenItem.FindFeature("n.whitespace");
                    if (whitespace.Equals(" "))
                    {
                        _wordRelation.AddWord(saint);
                    }
                    else
                    {
                        _wordRelation.AddWord(street);
                    }
                }
            }

            if (punctuation != null && punctuation.Equals("."))
            {
                featureSet.SetString("punc", "");
            }
        }

        /// <summary>
        /// Converts US money string into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The US money string.</param>
        private void UsMoneyToWords(string tokenVal)
        {
            int dotIndex = tokenVal.IndexOf('.');
            if (Matches(IllionPattern, (string)_tokenItem.FindFeature("n.name")))
            {
                NumberExpander.ExpandReal(tokenVal.Substring(1), _wordRelation);
            }
            else if (dotIndex == -1)
            {
                string aaa = tokenVal.Substring(1);
                TokenToWords(aaa);
                if (aaa.Equals("1"))
                {
                    _wordRelation.AddWord("dollar");
                }
                else
                {
                    _wordRelation.AddWord("dollars");
                }
            }
            else if (dotIndex == (tokenVal.Length - 1)
                  || (tokenVal.Length - dotIndex) > 3)
            {
                // Simply read as mumble point mumble.
                NumberExpander.ExpandReal(tokenVal.Substring(1), _wordRelation);
                _wordRelation.AddWord("dollars");
            }
            else
            {
                string aaa = tokenVal.JSubString(1, dotIndex).Replace(",", "");
                string bbb = tokenVal.Substring(dotIndex + 1);

                NumberExpander.ExpandNumber(aaa, _wordRelation);

                if (aaa.Equals("1"))
                {
                    _wordRelation.AddWord("dollar");
                }
                else
                {
                    _wordRelation.AddWord("dollars");
                }

                if (bbb.Equals("00"))
                {
                    // Add nothing to the word list.
                }
                else
                {
                    NumberExpander.ExpandNumber(bbb, _wordRelation);
                    if (bbb.Equals("01"))
                    {
                        _wordRelation.AddWord("cent");
                    }
                    else
                    {
                        _wordRelation.AddWord("cents");
                    }
                }
            }
        }

        /// <summary>
        /// Convert the given digits/digits string into word (Items) in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The digits/digits string.</param>
        private void DigitsSlashDigitsToWords([In] string tokenVal)
        {
            /* might be fraction, or not */
            int index = tokenVal.IndexOf('/');
            string aaa = tokenVal.JSubString(0, index);
            string bbb = tokenVal.Substring(index + 1);
            int a;

            // if the previous token is a number, add an "and"
            if (Matches(DigitsPattern, (string)_tokenItem.FindFeature("p.name"))
                    && _tokenItem.GetPrevious() != null)
            {
                _wordRelation.AddWord("and");
            }

            if (aaa.Equals("1") && bbb.Equals("2"))
            {
                _wordRelation.AddWord("a");
                _wordRelation.AddWord("half");
            }
            else if ((a = int.Parse(aaa, CultureInfo.InvariantCulture.NumberFormat)) < (int.Parse(bbb, CultureInfo.InvariantCulture.NumberFormat)))
            {
                NumberExpander.ExpandNumber(aaa, _wordRelation);
                NumberExpander.ExpandOrdinal(bbb, _wordRelation);
                if (a > 1)
                {
                    _wordRelation.AddWord("'s");
                }
            }
            else
            {
                NumberExpander.ExpandNumber(aaa, _wordRelation);
                _wordRelation.AddWord("slash");
                NumberExpander.ExpandNumber(bbb, _wordRelation);
            }
        }

        /// <summary>
        /// Convert the given dashed string (e.g. "aaa-bbb") into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The dashed string.</param>
        private void DashToWords([In] string tokenVal)
        {
            int index = tokenVal.IndexOf('-');
            string aaa = tokenVal.JSubString(0, index);
            string bbb = tokenVal.JSubString(index + 1, tokenVal.Length);

            if (Matches(DigitsPattern, aaa) && Matches(DigitsPattern, bbb))
            {
                FeatureSet featureSet = _tokenItem.Features;
                featureSet.SetString("name", aaa);
                TokenToWords(aaa);
                _wordRelation.AddWord("to");
                featureSet.SetString("name", bbb);
                TokenToWords(bbb);
                featureSet.SetString("name", "");
            }
            else
            {
                TokenToWords(aaa);
                TokenToWords(bbb);
            }
        }

        /// <summary>
        /// Convert the given string (which does not only consist of alphabet) into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The string.</param>
        private void NotJustAlphasToWords(string tokenVal)
        {
            /* its not just alphas */
            int index = 0;
            int tokenLength = tokenVal.Length;

            for (; index < tokenLength - 1; index++)
            {
                if (IsTextSplitable(tokenVal, index))
                {
                    break;
                }
            }
            if (index == tokenLength - 1)
            {
                _wordRelation.AddWord(tokenVal.ToLower());
                return;
            }

            string aaa = tokenVal.JSubString(0, index + 1);
            string bbb = tokenVal.JSubString(index + 1, tokenLength);

            FeatureSet featureSet = _tokenItem.Features;
            featureSet.SetString("nsw", "nide");
            TokenToWords(aaa);
            TokenToWords(bbb);
        }

        /// <summary>
        /// Returns true if the given word is pronounceable. This method is 
        /// originally called us_aswd() in Flite 1.1.
        /// </summary>
        /// <param name="word">The word to test.</param>
        /// <returns>true if the word is pronounceable, false otherwise</returns>
        public virtual bool IsPronounceable(string word)
        {
            string lcWord = word.ToLower();
            return prefixFSM.Accept(lcWord) && suffixFSM.Accept(lcWord);
        }

        /// <summary>
        /// Returns true if the given token is the name of a US state. If it is, it
        /// will add the name of the state to (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The token string.</param>
        private bool IsStateName([In] string tokenVal)
        {
            string[] state = UsStatesMap.Get(tokenVal);
            if (state != null)
            {
                bool expandState = false;

                // check to see if the state initials are ambiguous
                // in the English language
                if (state[1].Equals("ambiguous"))
                {
                    string previous = (string)_tokenItem.FindFeature("p.name");
                    string next = (string)_tokenItem.FindFeature("n.name");

                    int nextLength = next.Length;
                    FeatureSet featureSet = _tokenItem.Features;

                    // check if the previous word starts with a capital letter,
                    // is at least 3 letters long, is an alphabet sequence,
                    // and has a comma.
                    bool previousIsCity =
                            (char.IsUpper(previous[0])
                                    && previous.Length > 2
                                    && Matches(AlphabetPattern, previous) && _tokenItem
                                    .FindFeature("p.punc").Equals(","));

                    // check if next token starts with a lower case, or
                    // this is the end of sentence, or if next token
                    // is a period (".") or a zip code (5 or 10 digits).
                    bool nextIsGood =
                            (char.IsLower(next[0]))
                                    || _tokenItem.GetNext() == null
                                    || featureSet.GetString("punc").Equals(".") || ((nextLength == 5 || nextLength == 10) && Matches(
                                    DigitsPattern, next));

                    if (previousIsCity && nextIsGood)
                    {
                        expandState = true;
                    }
                    else
                    {
                        expandState = false;
                    }
                }
                else
                {
                    expandState = true;
                }
                if (expandState)
                {
                    for (int j = 2; j < state.Length; j++)
                    {
                        if (state[j] != null)
                        {
                            _wordRelation.AddWord(state[j]);
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if the given input matches the given Pattern.
        /// </summary>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="input">the string to test.</param>
        /// <returns><code>true</code> if the input string matches the given Pattern;
        /// <code>false</code> otherwise</returns>
        private static bool Matches(Pattern pattern, string input)
        {
            var matcher = pattern.Matcher(input);
            return matcher.Matches();
        }

        /// <summary>
        /// Determines if the character at the given position of the given input
        /// text is splittable. A character is splittable if:
        /// 1) the character and the following character are not letters in the
        /// English alphabet (A-Z and a-z)
        ///  2) the character and the following character are not digits (0-9)
        /// </summary>
        /// <param name="text">The text containing the character of interest.</param>
        /// <param name="index">The index of the character of interest.</param>
        /// <returns>true if the position of the given text is splittable false otherwise</returns>
        private static bool IsTextSplitable(string text, int index)
        {
            char c0 = text[index];
            char c1 = text[index + 1];

            if (char.IsLetter(c0) && char.IsLetter(c1))
            {
                return false;
            }
            else if (char.IsDigit(c0) && char.IsDigit(c1))
            {
                return false;
            }
            else if (c0 == '\'' || char.IsLetter(c1))
            {
                return false;
            }
            else if (c1 == '\'' || char.IsLetter(c0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
