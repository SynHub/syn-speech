//PATROLLED + REFACTORED
using System.Globalization;
using System.Runtime.InteropServices;
using Syn.Speech.Helper;

namespace Syn.Speech.Alignment.Tokenizer
{
  public class NumberExpander 
  {
 private static readonly string[] Digit2Num = {"zero", "one", "two", "three",
            "four", "five", "six", "seven", "eight", "nine"};

    private static readonly string[] Digit2Teen = {"ten", /* shouldn't get called */
    "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
            "seventeen", "eighteen", "nineteen"};

    private static readonly string[] Digit2Enty = {"zero", /* shouldn't get called */
    "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty",
            "ninety"};

    private static readonly string[] Ord2Num = {"zeroth", "first", "second",
            "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth"};

    private static readonly string[] Ord2Teen = {"tenth", /* shouldn't get called */
    "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth",
            "sixteenth", "seventeenth", "eighteenth", "nineteenth"};

    private static readonly string[] Ord2Enty = {"zeroth", /* shouldn't get called */
    "tenth", "twentieth", "thirtieth", "fortieth", "fiftieth", "sixtieth",
            "seventieth", "eightieth", "ninetieth"};

    private static readonly string[] Digit2Numness = {
           "", "tens", "twenties", "thirties", "fourties", "fifties", 
           "sixties", "seventies", "eighties", "nineties" 
    };

    //Unconstructable
    private NumberExpander(){}

    public static void ExpandNumber(string numberString, WordRelation wordRelation)
    {
      int numDigits = numberString.Length;

        if (numDigits == 0) {
            // wordRelation = null;
        } else if (numDigits == 1) {
            ExpandDigits(numberString, wordRelation);
        } else if (numDigits == 2) {
            Expand2DigitNumber(numberString, wordRelation);
        } else if (numDigits == 3) {
            Expand3DigitNumber(numberString, wordRelation);
        } else if (numDigits < 7) {
            ExpandBelow7DigitNumber(numberString, wordRelation);
        } else if (numDigits < 10) {
            ExpandBelow10DigitNumber(numberString, wordRelation);
        } else if (numDigits < 13) {
            ExpandBelow13DigitNumber(numberString, wordRelation);
        } else {
            ExpandDigits(numberString, wordRelation);
        }
    }

    private static void Expand2DigitNumber(string numberString, WordRelation wordRelation)
    {
      
              if (numberString[0] == '0') {
            // numberString is "0X"
            if (numberString[1] == '0') {
                // numberString is "00", do nothing
            } else {
                // numberString is "01", "02" ...
                string number = Digit2Num[numberString[1] - '0'];
                wordRelation.AddWord(number);
            }
        } else if (numberString[1] == '0') {
            // numberString is "10", "20", ...
            string number = Digit2Enty[numberString[0] - '0'];
            wordRelation.AddWord(number);
        } else if (numberString[0] == '1') {
            // numberString is "11", "12", ..., "19"
            string number = Digit2Teen[numberString[1] - '0'];
            wordRelation.AddWord(number);
        } else {
            // numberString is "2X", "3X", ...
            string enty = Digit2Enty[numberString[0] - '0'];
            wordRelation.AddWord(enty);
            ExpandDigits(numberString.JSubString(1, numberString.Length), wordRelation);
        }
    }

    private static void Expand3DigitNumber(string numberString, WordRelation wordRelation)
    {
        if (numberString[0] == '0') {
            ExpandNumberAt(numberString, 1, wordRelation);
        } else {
            string hundredDigit = Digit2Num[numberString[0] - '0'];
            wordRelation.AddWord(hundredDigit);
            wordRelation.AddWord("hundred");
            ExpandNumberAt(numberString, 1, wordRelation);
        }
    }

    private static void ExpandBelow7DigitNumber(string numberString, WordRelation wordRelation)
    {
      ExpandLargeNumber(numberString, "thousand", 3, wordRelation);
    }

    private static void ExpandBelow10DigitNumber(string numberString, WordRelation wordRelation)
    {
      ExpandLargeNumber(numberString, "million", 6, wordRelation);
    }

    private static void ExpandBelow13DigitNumber([In] string numberString, WordRelation wordRelation)
    {
      ExpandLargeNumber(numberString, "billion", 9, wordRelation);
    }

    private static void ExpandLargeNumber(string numberString, string order, int numberZeroes,  WordRelation wordRelation)
    {
        int numberDigits = numberString.Length;

        // parse out the prefix, e.g., "113" in "113,000"
        int i = numberDigits - numberZeroes;
        string part = numberString.JSubString(0, i);

        // get how many thousands/millions/billions
        Item oldTail = wordRelation.Tail;
        ExpandNumber(part, wordRelation);
        if (wordRelation.Tail != oldTail) {
            wordRelation.AddWord(order);
        }
        ExpandNumberAt(numberString, i, wordRelation);
    }

    private static void ExpandNumberAt(string numberString, int startIndex, WordRelation wordRelation)
    {
       ExpandNumber(numberString.JSubString(startIndex, numberString.Length), wordRelation);
    }
      
    public static void ExpandDigits(string numberString, WordRelation wordRelation)
    {
       int numberDigits = numberString.Length;
        for (int i = 0; i < numberDigits; i++) {
            char digit = numberString[i];
            if (char.IsDigit(digit)) {
                wordRelation.AddWord(Digit2Num[numberString[i] - '0']);
            } else {
                wordRelation.AddWord("umpty");
            }
        }
    }

    public static void ExpandOrdinal(string rawNumberString, WordRelation wordRelation)
    {
        ExpandNumber(rawNumberString.Replace(",", ""), wordRelation);

        // get the last in the list of number strings
        Item lastItem = wordRelation.Tail;

        if (lastItem != null)
        {

            FeatureSet featureSet = lastItem.Features;
            string lastNumber = featureSet.GetString("name");
            string ordinal = FindMatchInArray(lastNumber, Digit2Num, Ord2Num);

            if (ordinal == null)
            {
                ordinal = FindMatchInArray(lastNumber, Digit2Teen, Ord2Teen);
            }
            if (ordinal == null)
            {
                ordinal = FindMatchInArray(lastNumber, Digit2Enty, Ord2Enty);
            }

            if (lastNumber.Equals("hundred"))
            {
                ordinal = "hundredth";
            }
            else if (lastNumber.Equals("thousand"))
            {
                ordinal = "thousandth";
            }
            else if (lastNumber.Equals("billion"))
            {
                ordinal = "billionth";
            }

            // if there was an ordinal, set the last element of the list
            // to that ordinal; otherwise, don't do anything
            if (ordinal != null)
            {
                wordRelation.SetLastWord(ordinal);
            }
        }
    }

    public static void ExpandNumess(string rawString, WordRelation wordRelation)
    {
        if (rawString.Length == 4)
        {
            Expand2DigitNumber(rawString.JSubString(0, 2), wordRelation);
            ExpandNumess(rawString.Substring(2), wordRelation);
        }
        else
        {
            wordRelation.AddWord(Digit2Numness[rawString[0] - '0']);
        }
    }

    private static string FindMatchInArray( string strToMatch, string[] matchInArray,  string[] returnInArray)
    {
        for (int i = 0; i < matchInArray.Length; i++)
        {
            if (strToMatch.Equals(matchInArray[i]))
            {
                if (i < returnInArray.Length)
                {
                    return returnInArray[i];
                }
                return null;
            }
        }
        return null;
    }

    public static void ExpandId(string numberString, WordRelation wordRelation)
    {
        int numberDigits = numberString.Length;

        if ((numberDigits == 4) && (numberString[2] == '0')
                && (numberString[3] == '0'))
        {
            if (numberString[1] == '0')
            { // e.g. 2000, 3000
                ExpandNumber(numberString, wordRelation);
            }
            else
            {
                ExpandNumber(numberString.JSubString(0, 2), wordRelation);
                wordRelation.AddWord("hundred");
            }
        }
        else if ((numberDigits == 2) && (numberString[0] == '0'))
        {
            wordRelation.AddWord("oh");
            ExpandDigits(numberString.JSubString(1, 2), wordRelation);
        }
        else if ((numberDigits == 4 && numberString[1] == '0')
              || numberDigits < 3)
        {
            ExpandNumber(numberString, wordRelation);
        }
        else if (numberDigits % 2 == 1)
        {
            string firstDigit = Digit2Num[numberString[0] - '0'];
            wordRelation.AddWord(firstDigit);
            ExpandId(numberString.JSubString(1, numberDigits), wordRelation);
        }
        else
        {
            ExpandNumber(numberString.JSubString(0, 2), wordRelation);
            ExpandId(numberString.JSubString(2, numberDigits), wordRelation);
        }
   
    }

    public static void ExpandReal(string numberString, WordRelation wordRelation)
    {
        int stringLength = numberString.Length;
        int position;

        if (numberString[0] == '-')
        {
            // negative real numbers
            wordRelation.AddWord("minus");
            ExpandReal(numberString.JSubString(1, stringLength), wordRelation);
        }
        else if (numberString[0] == '+')
        {
            // prefixed with a '+'
            wordRelation.AddWord("plus");
            ExpandReal(numberString.JSubString(1, stringLength), wordRelation);
        }
        else if ((position = numberString.IndexOf('e')) != -1
              || (position = numberString.IndexOf('E')) != -1)
        {
            // numbers with 'E' or 'e'
            ExpandReal(numberString.JSubString(0, position), wordRelation);
            wordRelation.AddWord("e");
            ExpandReal(numberString.Substring(position + 1), wordRelation);
        }
        else if ((position = numberString.IndexOf('.')) != -1)
        {
            // numbers with '.'
            string beforeDot = numberString.JSubString(0, position);
            if (beforeDot.Length > 0)
            {
                ExpandReal(beforeDot, wordRelation);
            }
            wordRelation.AddWord("point");
            string afterDot = numberString.Substring(position + 1);
            if (afterDot.Length > 0)
            {
                ExpandDigits(afterDot, wordRelation);
            }
        }
        else
        {
            // everything else
            ExpandNumber(numberString, wordRelation);
        }
    }

    public static void ExpandLetters(string letters, WordRelation wordRelation)
    {
        letters = letters.ToLower();
        char c;

        for (int i = 0; i < letters.Length; i++)
        {
            // if this is a number
            c = letters[i];
            if (char.IsDigit(c))
            {
                wordRelation.AddWord(Digit2Num[c - '0']);
            }
            else if (letters.Equals("a"))
            {
                wordRelation.AddWord("_a");
            }
            else
            {
                wordRelation.AddWord(c.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public static int ExpandRoman(string roman)
    {
        int value = 0;

        for (int p = 0; p < roman.Length; p++)
        {
            char c = roman[p];
            if (c == 'X')
            {
                value += 10;
            }
            else if (c == 'V')
            {
                value += 5;
            }
            else if (c == 'I')
            {
                if (p + 1 < roman.Length)
                {
                    char p1 = roman[p + 1];
                    if (p1 == 'V')
                    {
                        value += 4;
                        p++;
                    }
                    else if (p1 == 'X')
                    {
                        value += 9;
                        p++;
                    }
                    else
                    {
                        value += 1;
                    }
                }
                else
                {
                    value += 1;
                }
            }
        }
        return value;
    }
  }
}
