using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using Syn.Speech.Helper;

//PATROLLED
using Syn.Speech.Properties;
using Syn.Speech.Recognizers;

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
    public class UsEnglishWordExpander : Object, IWordExpander
    {
        private static readonly Pattern alphabetPattern = Pattern.Compile(UsEnglish.RX_ALPHABET);
        private static readonly Pattern commaIntPattern = Pattern.Compile(UsEnglish.RX_COMMAINT);
        private static readonly Pattern digits2DashPattern = Pattern.Compile("[0-9]+(-[0-9]+)(-[0-9]+)+");
        private static readonly Pattern digitsPattern = Pattern.Compile(UsEnglish.RX_DIGITS);
        private static readonly Pattern digitsSlashDigitsPattern = Pattern.Compile("[0-9]+/[0-9]+");
        private static readonly Pattern dottedAbbrevPattern = Pattern.Compile(UsEnglish.RX_DOTTED_ABBREV);
        private static readonly Pattern doublePattern = Pattern.Compile(UsEnglish.RX_DOUBLE);
        private static readonly Pattern drStPattern = Pattern.Compile("([dD][Rr]|[Ss][Tt])");
        private static readonly Pattern fourDigitsPattern = Pattern.Compile("[0-9][0-9][0-9][0-9]");
        private static readonly Pattern illionPattern;
        private static readonly Pattern numberTimePattern;
        private static readonly Pattern numessPattern;
        private static readonly Pattern ordinalPattern;
        private static readonly Pattern romanNumbersPattern;
        private static readonly Pattern sevenPhoneNumberPattern;
        private static readonly Pattern threeDigitsPattern;
        private static readonly Pattern usMoneyPattern;

        private static readonly HashMap<string, string> kingSectionLikeMap = new HashMap<string, string>();
        private const string KING_NAMES = "kingNames";
        private const string KING_TITLES = "kingTitles";
        private const string SECTION_TYPES = "sectionTypes";

        private static readonly HashMap<string, string[]> usStatesMap = new HashMap<string, string[]>();
        private WordRelation wordRelation;
        private Item tokenItem;
        private readonly DecisionTree cart;

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

        private readonly PronounceableFSM prefixFSM;
        private readonly PronounceableFSM suffixFSM;
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


        static UsEnglishWordExpander()
        {
            alphabetPattern = Pattern.Compile(UsEnglish.RX_ALPHABET);
            commaIntPattern = Pattern.Compile(UsEnglish.RX_COMMAINT);
            digits2DashPattern = Pattern.Compile(UsEnglish.RX_DIGITS2DASH);
            digitsPattern = Pattern.Compile(UsEnglish.RX_DIGITS);
            digitsSlashDigitsPattern =
                    Pattern.Compile(UsEnglish.RX_DIGITSSLASHDIGITS);
            dottedAbbrevPattern = Pattern.Compile(UsEnglish.RX_DOTTED_ABBREV);
            doublePattern = Pattern.Compile(UsEnglish.RX_DOUBLE);
            drStPattern = Pattern.Compile(UsEnglish.RX_DRST);
            fourDigitsPattern = Pattern.Compile(UsEnglish.RX_FOUR_DIGIT);
            Pattern.Compile(UsEnglish.RX_HAS_VOWEL);
            illionPattern = Pattern.Compile(UsEnglish.RX_ILLION);
            numberTimePattern = Pattern.Compile(UsEnglish.RX_NUMBER_TIME);
            numessPattern = Pattern.Compile(UsEnglish.RX_NUMESS);
            ordinalPattern = Pattern.Compile(UsEnglish.RX_ORDINAL_NUMBER);
            romanNumbersPattern = Pattern.Compile(UsEnglish.RX_ROMAN_NUMBER);
            sevenPhoneNumberPattern =
                    Pattern.Compile(UsEnglish.RX_SEVEN_DIGIT_PHONE_NUMBER);
            threeDigitsPattern = Pattern.Compile(UsEnglish.RX_THREE_DIGIT);
            usMoneyPattern = Pattern.Compile(UsEnglish.RX_US_MONEY);

            for (int i = 0; i < kingNames.Length; i++)
            {
                kingSectionLikeMap.put(kingNames[i], KING_NAMES);
            }
            for (int i = 0; i < kingTitles.Length; i++)
            {
                kingSectionLikeMap.put(kingTitles[i], KING_TITLES);
            }
            for (int i = 0; i < sectionTypes.Length; i++)
            {
                kingSectionLikeMap.put(sectionTypes[i], SECTION_TYPES);
            }

            // Again map for constant time searching.
            for (int i = 0; i < usStates.Length; i++)
            {
                usStatesMap.put(usStates[i][0], usStates[i]);
            }
        }

        /// <summary>
        /// Constructs a default USTokenWordProcessor. It uses the USEnglish regular
        /// expression set (USEngRegExp) by default.
        /// </summary>
        /// <exception cref="IllegalStateException">The cart to use to classify numbers.</exception>
        public UsEnglishWordExpander()
        {
            try
            {
                cart = new DecisionTree(Resources.nums_cart);
                prefixFSM = new PrefixFSM(Resources.prefix_fsm);
                suffixFSM = new SuffixFSM(Resources.suffix_fsm);

                //cart = new DecisionTree(getClass().getResource("nums_cart.txt"));
                //prefixFSM = new PrefixFSM(getClass().getResource("prefix_fsm.txt"));
                //suffixFSM = new SuffixFSM(getClass().getResource("suffix_fsm.txt"));
            }
            catch (IOException e)
            {
                throw new IllegalStateException("resources not found");
            }
        }

        /// <summary>
        /// Returns the currently processing token Item.
        /// </summary>
        /// <returns>The current token Item; null if no item</returns>
        public virtual Item getTokenItem()
        {
            return tokenItem;
        }

        /// <summary>
        /// process the utterance
        /// </summary>
        /// <param name="text">The text.</param>
        /// <exception cref="IllegalStateException"></exception>
        /// <returns>The utterance contain the tokens</returns>
        public virtual List<string> expand(string text)
        {

            string simplifiedText = simplifyChars(text);

            CharTokenizer tokenizer = new CharTokenizer();
            tokenizer.setWhitespaceSymbols(UsEnglish.WHITESPACE_SYMBOLS);
            tokenizer.setSingleCharSymbols(UsEnglish.SINGLE_CHAR_SYMBOLS);
            tokenizer.setPrepunctuationSymbols(UsEnglish.PREPUNCTUATION_SYMBOLS);
            tokenizer.setPostpunctuationSymbols(UsEnglish.PUNCTUATION_SYMBOLS);
            tokenizer.setInputText(simplifiedText);
            Utterance utterance = new Utterance(tokenizer);

            Relation tokenRelation;
            if ((tokenRelation = utterance.getRelation(Relation.TOKEN)) == null)
            {
                throw new IllegalStateException("token relation does not exist");
            }

            wordRelation = WordRelation.createWordRelation(utterance, this);

            for (tokenItem = tokenRelation.getHead(); tokenItem != null; tokenItem =
                    tokenItem.getNext())
            {

                FeatureSet featureSet = tokenItem.getFeatures();
                string tokenVal = featureSet.getString("name");

                // convert the token into a list of words
                tokenToWords(tokenVal);
            }

            List<string> words = new List<string>();
            for (Item item = utterance.getRelation(Relation.WORD).getHead(); item != null; item =
                    item.getNext())
            {
                if (!string.IsNullOrEmpty(item.ToString()) && !item.ToString().Contains("#"))
                {
                    words.Add(item.ToString());
                }
            }
            return words;
        }

        private string simplifyChars(string text)
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
        private bool matchesPartPhoneNumber(string tokenVal)
        {
            string n_name = (string)tokenItem.findFeature("n.name");
            string n_n_name = (string)tokenItem.findFeature("n.n.name");
            string p_name = (string)tokenItem.findFeature("p.name");
            string p_p_name = (string)tokenItem.findFeature("p.p.name");

            bool matches3DigitsP_name = matches(threeDigitsPattern, p_name);

            return ((matches(threeDigitsPattern, tokenVal) && ((!matches(
                    digitsPattern, p_name) && matches(threeDigitsPattern, n_name) && matches(
                        fourDigitsPattern, n_n_name))
                    || (matches(sevenPhoneNumberPattern, n_name)) || (!matches(
                    digitsPattern, p_p_name) && matches3DigitsP_name && matches(
                        fourDigitsPattern, n_name)))) || (matches(
                    fourDigitsPattern, tokenVal) && (!matches(digitsPattern,
                    n_name) && matches3DigitsP_name && matches(threeDigitsPattern,
                        p_p_name))));
        }

        /// <summary>
        /// Converts the given Token into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">the string value of the token, which may or may not be
        /// same as the one in called "name" in flite</param>
        private void tokenToWords(string tokenVal)
        {
            FeatureSet tokenFeatures = tokenItem.getFeatures();
            string itemName = tokenFeatures.getString("name");
            int tokenLength = tokenVal.Length;

            if (tokenFeatures.isPresent("phones"))
            {
                wordRelation.addWord(tokenVal);

            }
            else if ((tokenVal.Equals("a") || tokenVal.Equals("A"))
                  && ((tokenItem.getNext() == null)
                          || !(tokenVal.Equals(itemName)) || !(((string)tokenItem
                              .findFeature("punc")).Equals(""))))
            {
                /* if A is a sub part of a token, then its ey not ah */
                wordRelation.addWord("_a");

            }
            else if (matches(alphabetPattern, tokenVal))
            {

                if (matches(romanNumbersPattern, tokenVal))
                {

                    /* XVIII */
                    romanToWords(tokenVal);

                }
                else if (matches(illionPattern, tokenVal)
                      && matches(usMoneyPattern,
                              (string)tokenItem.findFeature("p.name")))
                {
                    /* $ X -illion */
                    wordRelation.addWord(tokenVal);
                    wordRelation.addWord("dollars");

                }
                else if (matches(drStPattern, tokenVal))
                {
                    /* St Andrew's St, Dr King Dr */
                    drStToWords(tokenVal);
                }
                else if (tokenVal.Equals("Mr"))
                {
                    tokenItem.getFeatures().setString("punc", "");
                    wordRelation.addWord("mister");
                }
                else if (tokenVal.Equals("Mrs"))
                {
                    tokenItem.getFeatures().setString("punc", "");
                    wordRelation.addWord("missus");
                }
                else if (tokenLength == 1
                      && char.IsUpper(tokenVal[0])
                      && ((string)tokenItem.findFeature("n.whitespace"))
                              .Equals(" ")
                      && char.IsUpper(((string)tokenItem
                              .findFeature("n.name"))[0]))
                {

                    tokenFeatures.setString("punc", "");
                    string aaa = tokenVal.ToLower();
                    if (aaa.Equals("a"))
                    {
                        wordRelation.addWord("_a");
                    }
                    else
                    {
                        wordRelation.addWord(aaa);
                    }
                }
                else if (isStateName(tokenVal))
                {
                    /*
                     * The name of a US state isStateName() has already added the
                     * full name of the state, so we're all set.
                     */
                }
                else if (tokenLength > 1 && !isPronounceable(tokenVal))
                {
                    /* Need common exception list */
                    /* unpronouncable list of alphas */
                    NumberExpander.expandLetters(tokenVal, wordRelation);

                }
                else
                {
                    /* just a word */
                    wordRelation.addWord(tokenVal.ToLower());
                }

            }
            else if (matches(dottedAbbrevPattern, tokenVal))
            {

                /* U.S.A. */
                // remove all dots
                NumberExpander.expandLetters(tokenVal.Replace(".", ""),
                        wordRelation);

            }
            else if (matches(commaIntPattern, tokenVal))
            {

                /* 99,999,999 */
                NumberExpander.expandReal(tokenVal.Replace(",", "").Replace("'", ""), wordRelation);

            }
            else if (matches(sevenPhoneNumberPattern, tokenVal))
            {

                /* 234-3434 telephone numbers */
                int dashIndex = tokenVal.IndexOf('-');
                string aaa = tokenVal.Substring(0, dashIndex);
                string bbb = tokenVal.Substring(dashIndex + 1);

                NumberExpander.expandDigits(aaa, wordRelation);
                wordRelation.addBreak();
                NumberExpander.expandDigits(bbb, wordRelation);

            }
            else if (matchesPartPhoneNumber(tokenVal))
            {

                /* part of a telephone number */
                string punctuation = (string)tokenItem.findFeature("punc");
                if (punctuation.Equals(""))
                {
                    tokenItem.getFeatures().setString("punc", ",");
                }
                NumberExpander.expandDigits(tokenVal, wordRelation);
                wordRelation.addBreak();

            }
            else if (matches(numberTimePattern, tokenVal))
            {
                /* 12:35 */
                int colonIndex = tokenVal.IndexOf(':');
                string aaa = tokenVal.Substring(0, colonIndex);
                string bbb = tokenVal.Substring(colonIndex + 1);

                NumberExpander.expandNumber(aaa, wordRelation);
                if (!(bbb.Equals("00")))
                {
                    NumberExpander.expandID(bbb, wordRelation);
                }
            }
            else if (matches(digits2DashPattern, tokenVal))
            {
                /* 999-999-999 */
                digitsDashToWords(tokenVal);
            }
            else if (matches(digitsPattern, tokenVal))
            {
                digitsToWords(tokenVal);
            }
            else if (tokenLength == 1
                  && char.IsUpper(tokenVal[0])
                  && ((string)tokenItem.findFeature("n.whitespace"))
                          .Equals(" ")
                  && char.IsUpper(((string)tokenItem
                          .findFeature("n.name"))[0]))
            {

                tokenFeatures.setString("punc", "");
                string aaa = tokenVal.ToLower();
                if (aaa.Equals("a"))
                {
                    wordRelation.addWord("_a");
                }
                else
                {
                    wordRelation.addWord(aaa);
                }
            }
            else if (matches(doublePattern, tokenVal))
            {
                NumberExpander.expandReal(tokenVal, wordRelation);
            }
            else if (matches(ordinalPattern, tokenVal))
            {
                /* explicit ordinals */
                string aaa = tokenVal.Substring(0, tokenLength - 2);
                NumberExpander.expandOrdinal(aaa, wordRelation);
            }
            else if (matches(usMoneyPattern, tokenVal))
            {
                /* US money */
                usMoneyToWords(tokenVal);
            }
            else if (tokenLength > 0 && tokenVal[tokenLength - 1] == '%')
            {
                /* Y% */
                tokenToWords(tokenVal.Substring(0, tokenLength - 1));
                wordRelation.addWord("percent");
            }
            else if (matches(numessPattern, tokenVal))
            {
                NumberExpander.expandNumess(tokenVal.Substring(0, tokenLength - 1), wordRelation);
            }
            else if (matches(digitsSlashDigitsPattern, tokenVal)
                  && tokenVal.Equals(itemName))
            {
                digitsSlashDigitsToWords(tokenVal);
            }
            else if (tokenVal.IndexOf('-') != -1)
            {
                dashToWords(tokenVal);
            }
            else if (tokenLength > 1 && !matches(alphabetPattern, tokenVal))
            {
                notJustAlphasToWords(tokenVal);
            }
            else if (tokenVal.Equals("&"))
            {
                // &
                wordRelation.addWord("and");
            }
            else if (tokenVal.Equals("-"))
            {
                // Skip it
            }
            else
            {
                // Just a word.
                wordRelation.addWord(tokenVal.ToLower());
            }
        }

        /// <summary>
        ///  Convert the given digit token with dashes (e.g. 999-999-999) into (word)
        /// Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The digit string.</param>
        private void digitsDashToWords([In] string tokenVal)
        {
            int tokenLength = tokenVal.Length;
            int a = 0;
            for (int p = 0; p <= tokenLength; p++)
            {
                if (p == tokenLength || tokenVal[p] == '-')
                {
                    string aaa = tokenVal.Substring(a, p);
                    NumberExpander.expandDigits(aaa, wordRelation);
                    wordRelation.addBreak();
                    a = p + 1;
                }
            }
        }

        /// <summary>
        /// Convert the given digit token into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The digit string.</param>
        private void digitsToWords(string tokenVal)
        {
            FeatureSet featureSet = tokenItem.getFeatures();
            string nsw = "";
            if (featureSet.isPresent("nsw"))
            {
                nsw = featureSet.getString("nsw");
            }

            if (nsw.Equals("nide"))
            {
                NumberExpander.expandID(tokenVal, wordRelation);
            }
            else
            {
                string rName = featureSet.getString("name");
                string digitsType = null;

                if (tokenVal.Equals(rName))
                {
                    digitsType = (string)cart.interpret(tokenItem);
                }
                else
                {
                    featureSet.setString("name", tokenVal);
                    digitsType = (string)cart.interpret(tokenItem);
                    featureSet.setString("name", rName);
                }

                if (digitsType.Equals("ordinal"))
                {
                    NumberExpander.expandOrdinal(tokenVal, wordRelation);
                }
                else if (digitsType.Equals("digits"))
                {
                    NumberExpander.expandDigits(tokenVal, wordRelation);
                }
                else if (digitsType.Equals("year"))
                {
                    NumberExpander.expandID(tokenVal, wordRelation);
                }
                else
                {
                    NumberExpander.expandNumber(tokenVal, wordRelation);
                }
            }
        }

        /// <summary>
        /// Converts the given Roman numeral string into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="romanString">The roman numeral string.</param>
        private void romanToWords(string romanString)
        {
            string punctuation = (string)tokenItem.findFeature("p.punc");

            if (punctuation.Equals(""))
            {
                /* no preceeding punctuation */
                //string n = String.valueOf(NumberExpander.expandRoman(romanString));
                var n = NumberExpander.expandRoman(romanString).ToString(CultureInfo.InvariantCulture);

                if (kingLike(tokenItem))
                {
                    wordRelation.addWord("the");
                    NumberExpander.expandOrdinal(n, wordRelation);
                }
                else if (sectionLike(tokenItem))
                {
                    NumberExpander.expandNumber(n, wordRelation);
                }
                else
                {
                    NumberExpander.expandLetters(romanString, wordRelation);
                }
            }
            else
            {
                NumberExpander.expandLetters(romanString, wordRelation);
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
        private static bool inKingSectionLikeMap(string key, string value)
        {
            if (kingSectionLikeMap.ContainsKey(key))
            {
                return kingSectionLikeMap.get(key).Equals(value);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given token item contains a token that is in a
        /// king-like context, e.g., "King" or "Louis".
        /// </summary>
        /// <param name="tokenItem">the token item to check.</param>
        /// <returns>true or false</returns>
        public static bool kingLike(Item tokenItem)
        {
            string kingName = ((string)tokenItem.findFeature("p.name")).ToLower();
            if (inKingSectionLikeMap(kingName, KING_NAMES))
            {
                return true;
            }
            else
            {
                string kingTitle =
                    ((string)tokenItem.findFeature("p.p.name")).ToLower();
                return inKingSectionLikeMap(kingTitle, KING_TITLES);
            }
        }

        /// <summary>
        /// Returns true if the given token item contains a token that is in a
        /// section-like context, e.g., "chapter" or "act".
        /// </summary>
        /// <param name="tokenItem">the token item to check.</param>
        /// <returns>true or false</returns>
        public static bool sectionLike(Item tokenItem)
        {
            string sectionType = ((string)tokenItem.findFeature("p.name")).ToLower();
            return inKingSectionLikeMap(sectionType, SECTION_TYPES);
        }

        /// <summary>
        /// Converts the given string containing "St" and "Dr" to (word) Items in the WordRelation.
        /// </summary>
        /// <param name="drStString">The string with "St" and "Dr".</param>
        private void drStToWords(string drStString)
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

            FeatureSet featureSet = tokenItem.getFeatures();
            string punctuation = featureSet.getString("punc");

            string featPunctuation = (string)tokenItem.findFeature("punc");

            if (tokenItem.getNext() == null || punctuation.IndexOf(',') != -1)
            {
                wordRelation.addWord(street);
            }
            else if (featPunctuation.Equals(","))
            {
                wordRelation.addWord(saint);
            }
            else
            {
                string pName = (string)tokenItem.findFeature("p.name");
                string nName = (string)tokenItem.findFeature("n.name");

                char p0 = pName[0];
                char n0 = nName[0];

                if (char.IsUpper(p0) && char.IsLower(n0))
                {
                    wordRelation.addWord(street);
                }
                else if (char.IsDigit(p0) && char.IsLower(n0))
                {
                    wordRelation.addWord(street);
                }
                else if (char.IsLower(p0) && char.IsUpper(n0))
                {
                    wordRelation.addWord(saint);
                }
                else
                {
                    string whitespace = (string)tokenItem.findFeature("n.whitespace");
                    if (whitespace.Equals(" "))
                    {
                        wordRelation.addWord(saint);
                    }
                    else
                    {
                        wordRelation.addWord(street);
                    }
                }
            }

            if (punctuation != null && punctuation.Equals("."))
            {
                featureSet.setString("punc", "");
            }
        }

        /// <summary>
        /// Converts US money string into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The US money string.</param>
        private void usMoneyToWords(string tokenVal)
        {
            int dotIndex = tokenVal.IndexOf('.');
            if (matches(illionPattern, (string)tokenItem.findFeature("n.name")))
            {
                NumberExpander.expandReal(tokenVal.Substring(1), wordRelation);
            }
            else if (dotIndex == -1)
            {
                string aaa = tokenVal.Substring(1);
                tokenToWords(aaa);
                if (aaa.Equals("1"))
                {
                    wordRelation.addWord("dollar");
                }
                else
                {
                    wordRelation.addWord("dollars");
                }
            }
            else if (dotIndex == (tokenVal.Length - 1)
                  || (tokenVal.Length - dotIndex) > 3)
            {
                // Simply read as mumble point mumble.
                NumberExpander.expandReal(tokenVal.Substring(1), wordRelation);
                wordRelation.addWord("dollars");
            }
            else
            {
                string aaa = tokenVal.Substring(1, dotIndex).Replace(",", "");
                string bbb = tokenVal.Substring(dotIndex + 1);

                NumberExpander.expandNumber(aaa, wordRelation);

                if (aaa.Equals("1"))
                {
                    wordRelation.addWord("dollar");
                }
                else
                {
                    wordRelation.addWord("dollars");
                }

                if (bbb.Equals("00"))
                {
                    // Add nothing to the word list.
                }
                else
                {
                    NumberExpander.expandNumber(bbb, wordRelation);
                    if (bbb.Equals("01"))
                    {
                        wordRelation.addWord("cent");
                    }
                    else
                    {
                        wordRelation.addWord("cents");
                    }
                }
            }
        }

        /// <summary>
        /// Convert the given digits/digits string into word (Items) in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The digits/digits string.</param>
        private void digitsSlashDigitsToWords([In] string tokenVal)
        {
            /* might be fraction, or not */
            int index = tokenVal.IndexOf('/');
            string aaa = tokenVal.Substring(0, index);
            string bbb = tokenVal.Substring(index + 1);
            int a;

            // if the previous token is a number, add an "and"
            if (matches(digitsPattern, (string)tokenItem.findFeature("p.name"))
                    && tokenItem.getPrevious() != null)
            {
                wordRelation.addWord("and");
            }

            if (aaa.Equals("1") && bbb.Equals("2"))
            {
                wordRelation.addWord("a");
                wordRelation.addWord("half");
            }
            else if ((a = int.Parse(aaa)) < (int.Parse(bbb)))
            {
                NumberExpander.expandNumber(aaa, wordRelation);
                NumberExpander.expandOrdinal(bbb, wordRelation);
                if (a > 1)
                {
                    wordRelation.addWord("'s");
                }
            }
            else
            {
                NumberExpander.expandNumber(aaa, wordRelation);
                wordRelation.addWord("slash");
                NumberExpander.expandNumber(bbb, wordRelation);
            }
        }

        /// <summary>
        /// Convert the given dashed string (e.g. "aaa-bbb") into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The dashed string.</param>
        private void dashToWords([In] string tokenVal)
        {
            int index = tokenVal.IndexOf('-');
            string aaa = tokenVal.Substring(0, index);
            string bbb = tokenVal.Substring(index + 1, tokenVal.Length);

            if (matches(digitsPattern, aaa) && matches(digitsPattern, bbb))
            {
                FeatureSet featureSet = tokenItem.getFeatures();
                featureSet.setString("name", aaa);
                tokenToWords(aaa);
                wordRelation.addWord("to");
                featureSet.setString("name", bbb);
                tokenToWords(bbb);
                featureSet.setString("name", "");
            }
            else
            {
                tokenToWords(aaa);
                tokenToWords(bbb);
            }
        }

        /// <summary>
        /// Convert the given string (which does not only consist of alphabet) into (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The string.</param>
        private void notJustAlphasToWords(string tokenVal)
        {
            /* its not just alphas */
            int index = 0;
            int tokenLength = tokenVal.Length;

            for (; index < tokenLength - 1; index++)
            {
                if (isTextSplitable(tokenVal, index))
                {
                    break;
                }
            }
            if (index == tokenLength - 1)
            {
                wordRelation.addWord(tokenVal.ToLower());
                return;
            }

            string aaa = tokenVal.Substring(0, index + 1);
            string bbb = tokenVal.Substring(index + 1, tokenLength);

            FeatureSet featureSet = tokenItem.getFeatures();
            featureSet.setString("nsw", "nide");
            tokenToWords(aaa);
            tokenToWords(bbb);
        }

        /// <summary>
        /// Returns true if the given word is pronounceable. This method is 
        /// originally called us_aswd() in Flite 1.1.
        /// </summary>
        /// <param name="word">The word to test.</param>
        /// <returns>true if the word is pronounceable, false otherwise</returns>
        public virtual bool isPronounceable(string word)
        {
            string lcWord = word.ToLower();
            return prefixFSM.accept(lcWord) && suffixFSM.accept(lcWord);
        }

        /// <summary>
        /// Returns true if the given token is the name of a US state. If it is, it
        /// will add the name of the state to (word) Items in the WordRelation.
        /// </summary>
        /// <param name="tokenVal">The token string.</param>
        private bool isStateName([In] string tokenVal)
        {
            string[] state = usStatesMap.get(tokenVal);
            if (state != null)
            {
                bool expandState = false;

                // check to see if the state initials are ambiguous
                // in the English language
                if (state[1].Equals("ambiguous"))
                {
                    string previous = (string)tokenItem.findFeature("p.name");
                    string next = (string)tokenItem.findFeature("n.name");

                    int nextLength = next.Length;
                    FeatureSet featureSet = tokenItem.getFeatures();

                    // check if the previous word starts with a capital letter,
                    // is at least 3 letters long, is an alphabet sequence,
                    // and has a comma.
                    bool previousIsCity =
                            (char.IsUpper(previous[0])
                                    && previous.Length > 2
                                    && matches(alphabetPattern, previous) && tokenItem
                                    .findFeature("p.punc").Equals(","));

                    // check if next token starts with a lower case, or
                    // this is the end of sentence, or if next token
                    // is a period (".") or a zip code (5 or 10 digits).
                    bool nextIsGood =
                            (char.IsLower(next[0]))
                                    || tokenItem.getNext() == null
                                    || featureSet.getString("punc").Equals(".") || ((nextLength == 5 || nextLength == 10) && matches(
                                    digitsPattern, next));

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
                            wordRelation.addWord(state[j]);
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
        private static bool matches(Pattern pattern, string input)
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
        private static bool isTextSplitable(string text, int index)
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
