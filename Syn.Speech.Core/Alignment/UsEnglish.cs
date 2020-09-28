//PATROLLED + REFACTORED
namespace Syn.Speech.Alignment
{
    /// <summary>
    /// Provides the definitions for US English whitespace, punctuations,
    /// prepunctuation, and postpunctuation symbols. It also contains a set of
    /// Regular Expressions for the US English language. With regular expressions,
    /// it specifies what are whitespace, letters in the alphabet, uppercase and
    /// lowercase letters, alphanumeric characters, identifiers, integers, doubles,
    /// digits, and 'comma and int'.
    /// It translates the following code from flite: src/regex/cst_regex.c
    /// lang/usenglish/us_text.c
    /// </summary>
    internal class UsEnglish 
    {
        /// <summary>
        /// default whitespace regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnWhitespace = "[ \n\t\r]+";
        /// <summary>
        /// default letter regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnAlphabet = "[A-Za-z]+";
        /// <summary>
        /// default uppercase regular expression pattern 
        /// </summary>
        public const string RxDefaultUsEnUppercase = "[A-Z]+";
        /// <summary>
        /// default lowercase regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnLowercase = "[a-z]+";
        /// <summary>
        /// default alpha-numeric regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnAlphanumeric = "[0-9A-Za-z]+";
        /// <summary>
        /// default identifier regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnIdentifier = "[A-Za-z_][0-9A-Za-z_]+";
        /// <summary>
        /// default integer regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnInt = "-?[0-9]+";
        /// <summary>
        /// default double regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnDouble = "-?(([0-9]+\\.[0-9]*)|([0-9]+)|(\\.[0-9]+))([eE][---+]?[0-9]+)?";
        /// <summary>
        /// default integer with commas regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnCommaint = "[0-9][0-9]?[0-9]?[,']([0-9][0-9][0-9][,'])*[0-9][0-9][0-9](\\.[0-9]+)?";

        /// <summary>
        /// default digits regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnDigits =  "[0-9][0-9]*"; //TODO: CHECK BEHAVIOUR
        /// <summary>
        /// default dotted abbreviation regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnDottedAbbrev = "([A-Za-z]\\.)*[A-Za-z]";
        /// <summary>
        /// default ordinal number regular expression pattern
        /// </summary>
        public const string RxDefaultUsEnOrdinalNumber = "[0-9][0-9,]*(th|TH|st|ST|nd|ND|rd|RD)";
        /// <summary>
        /// default has-vowel regular expression
        /// </summary>
        public const string RxDefaultHasVowel = ".*[aeiouAEIOU].*";
        /// <summary>
        /// default US money regular expression
        /// </summary>
        public const string RxDefaultUsMoney = "\\$[0-9,]+(\\.[0-9]+)?";
        /// <summary>
        /// default -illion regular expression
        /// </summary>
        public const string RxDefaultIllion = ".*illion";
        /// <summary>
        /// default digits2dash (e.g. 999-999-999) regular expression
        /// </summary>
        public const string RxDefaultDigits2Dash = "[0-9]+(-[0-9]+)(-[0-9]+)+";
        /// <summary>
        /// default digits/digits (e.g. 999/999) regular expression
        /// </summary>
        public const string RxDefaultDigitsslashdigits = "[0-9]+/[0-9]+";
        /// <summary>
        /// default number time regular expression
        /// </summary>
        public const string RxDefaultNumberTime = "((0[0-2])|(1[0-9])):([0-5][0-9])";
        /// <summary>
        /// default Roman numerals regular expression
        /// </summary>
        public const string RxDefaultRomanNumber = "(II?I?|IV|VI?I?I?|IX|X[VIX]*)";
        /// <summary>
        /// default drst "Dr. St" regular expression
        /// </summary>
        public const string RxDefaultDrst = "([dD][Rr]|[Ss][Tt])";
        /// <summary>
        /// default numess
        /// </summary>
        public const string RxDefaultNumess = "[0-9]+s";
        /// <summary>
        /// default 7-digit phone number
        /// </summary>
        public const string RxDefaultSevenDigitPhoneNumber = "[0-9][0-9][0-9]-[0-9][0-9][0-9][0-9]";
        /// <summary>
        /// default 4-digit number
        /// </summary>
        public const string RxDefaultFourDigit = "[0-9][0-9][0-9][0-9]";
        /// <summary>
        /// default 3-digit number
        /// </summary>
        public const string RxDefaultThreeDigit = "[0-9][0-9][0-9]";

        /// <summary>
        /// whitespace regular expression pattern
        /// </summary>
        public static string RxWhitespace = RxDefaultUsEnWhitespace;
        /// <summary>
        /// letter regular expression pattern
        /// </summary>
        public static string RxAlphabet = RxDefaultUsEnAlphabet;
        /// <summary>
        /// uppercase regular expression pattern
        /// </summary>
        public static string RxUppercase = RxDefaultUsEnUppercase;
        /// <summary>
        /// lowercase regular expression pattern
        /// </summary>
        public static string RxLowercase = RxDefaultUsEnLowercase;
        /// <summary>
        /// alphanumeric regular expression pattern 
        /// </summary>
        public static string RxAlphanumeric = RxDefaultUsEnAlphanumeric;
        /// <summary>
        ///identifier regular expression pattern
        /// </summary>
        public static string RxIdentifier = RxDefaultUsEnIdentifier;
        /// <summary>
        /// integer regular expression pattern
        /// </summary>
        public static string RxInt = RxDefaultUsEnInt;
        /// <summary>
        /// double regular expression pattern
        /// </summary>
        public static string RxDouble = RxDefaultUsEnDouble;
        /// <summary>
        /// comma separated integer regular expression pattern
        /// </summary>
        public static string RxCommaint = RxDefaultUsEnCommaint;
        /// <summary>
        /// digits regular expression pattern
        /// </summary>
        public static string RxDigits = RxDefaultUsEnDigits;
        /// <summary>
        /// dotted abbreviation regular expression pattern
        /// </summary>
        public static string RxDottedAbbrev = RxDefaultUsEnDottedAbbrev;
        /// <summary>
        /// ordinal number regular expression pattern
        /// </summary>
        public static string RxOrdinalNumber = RxDefaultUsEnOrdinalNumber;
        /// <summary>
        /// has-vowel regular expression
        /// </summary>
        public const string RxHasVowel = RxDefaultHasVowel;
        /// <summary>
        /// US money regular expression
        /// </summary>
        public const string RxUsMoney = RxDefaultUsMoney;
        /// <summary>
        /// -illion regular expression
        /// </summary>
        public const string RxIllion = RxDefaultIllion;
        /// <summary>
        /// digits2dash (e.g. 999-999-999) regular expression
        /// </summary>
        public const string RxDigits2Dash = RxDefaultDigits2Dash;
        /// <summary>
        /// digits/digits (e.g. 999/999) regular expression
        /// </summary>
        public const string RxDigitsslashdigits = RxDefaultDigitsslashdigits;
        /// <summary>
        /// number time regular expression
        /// </summary>
        public const string RxNumberTime = RxDefaultNumberTime;
        /// <summary>
        /// Roman numerals regular expression
        /// </summary>
        public const string RxRomanNumber = RxDefaultRomanNumber;
        /// <summary>
        /// drst "Dr. St" regular expression
        /// </summary>
        public const string RxDrst = RxDefaultDrst;
        /// <summary>
        /// default numess
        /// </summary>
        public const string RxNumess = RxDefaultNumess;
        /// <summary>
        /// 7-digit phone number
        /// </summary>
        public const string RxSevenDigitPhoneNumber = RxDefaultSevenDigitPhoneNumber;
        /// <summary>
        /// 4-digit number
        /// </summary>
        public const string RxFourDigit = RxDefaultFourDigit;
        /// <summary>
        /// 3-digit number
        /// </summary>
        public const string RxThreeDigit = RxDefaultThreeDigit;

        // the following symbols are from lang/usenglish/us_text.c

        /// <summary>
        /// punctuation regular expression pattern
        /// </summary>
        public const string PunctuationSymbols = "\"'`.,:;!?(){}[]";
        /// <summary>
        /// pre-punctuation regular expression pattern
        /// </summary>
        public const string PrePunctuationSymbols = "\"'`({[";
        /// <summary>
        /// single char symbols regular expression pattern
        /// </summary>
        public const string SingleCharSymbols = "";
        /// <summary>
        /// whitespace symbols regular expression pattern
        /// </summary>
        public const string WhitespaceSymbols = " \t\n\r";

        /// <summary>
        /// Prevents a default instance of the <see cref="UsEnglish"/> class from being created.
        /// </summary>
        private UsEnglish() { }
    }
}
