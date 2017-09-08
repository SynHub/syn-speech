using System.Globalization;
using System.Runtime.InteropServices;
//PATROLLED
namespace Syn.Speech.Alignment
{
  public class NumberExpander 
  {
 private static readonly string[] digit2num = {"zero", "one", "two", "three",
            "four", "five", "six", "seven", "eight", "nine"};

    private static readonly string[] digit2teen = {"ten", /* shouldn't get called */
    "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
            "seventeen", "eighteen", "nineteen"};

    private static readonly string[] digit2enty = {"zero", /* shouldn't get called */
    "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty",
            "ninety"};

    private static readonly string[] ord2num = {"zeroth", "first", "second",
            "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth"};

    private static readonly string[] ord2teen = {"tenth", /* shouldn't get called */
    "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth",
            "sixteenth", "seventeenth", "eighteenth", "nineteenth"};

    private static readonly string[] ord2enty = {"zeroth", /* shouldn't get called */
    "tenth", "twentieth", "thirtieth", "fortieth", "fiftieth", "sixtieth",
            "seventieth", "eightieth", "ninetieth"};

    private static readonly string[] digit2Numness = {
           "", "tens", "twenties", "thirties", "fourties", "fifties", 
           "sixties", "seventies", "eighties", "nineties" 
    };

    //Unconstructable
    private NumberExpander(){}

    public static void expandNumber(string numberString, WordRelation wordRelation)
    {
      int numDigits = numberString.Length;

        if (numDigits == 0) {
            // wordRelation = null;
        } else if (numDigits == 1) {
            expandDigits(numberString, wordRelation);
        } else if (numDigits == 2) {
            expand2DigitNumber(numberString, wordRelation);
        } else if (numDigits == 3) {
            expand3DigitNumber(numberString, wordRelation);
        } else if (numDigits < 7) {
            expandBelow7DigitNumber(numberString, wordRelation);
        } else if (numDigits < 10) {
            expandBelow10DigitNumber(numberString, wordRelation);
        } else if (numDigits < 13) {
            expandBelow13DigitNumber(numberString, wordRelation);
        } else {
            expandDigits(numberString, wordRelation);
        }
    }

    private static void expand2DigitNumber(string numberString, WordRelation wordRelation)
    {
      
              if (numberString[0] == '0') {
            // numberString is "0X"
            if (numberString[1] == '0') {
                // numberString is "00", do nothing
            } else {
                // numberString is "01", "02" ...
                string number = digit2num[numberString[1] - '0'];
                wordRelation.addWord(number);
            }
        } else if (numberString[1] == '0') {
            // numberString is "10", "20", ...
            string number = digit2enty[numberString[0] - '0'];
            wordRelation.addWord(number);
        } else if (numberString[0] == '1') {
            // numberString is "11", "12", ..., "19"
            string number = digit2teen[numberString[1] - '0'];
            wordRelation.addWord(number);
        } else {
            // numberString is "2X", "3X", ...
            string enty = digit2enty[numberString[0] - '0'];
            wordRelation.addWord(enty);
            expandDigits(numberString.Substring(1, numberString.Length),
                    wordRelation);
        }
    }

    private static void expand3DigitNumber(string numberString, WordRelation wordRelation)
    {
        if (numberString[0] == '0') {
            expandNumberAt(numberString, 1, wordRelation);
        } else {
            string hundredDigit = digit2num[numberString[0] - '0'];
            wordRelation.addWord(hundredDigit);
            wordRelation.addWord("hundred");
            expandNumberAt(numberString, 1, wordRelation);
        }
    }

    private static void expandBelow7DigitNumber(string numberString, WordRelation wordRelation)
    {
      expandLargeNumber(numberString, "thousand", 3, wordRelation);
    }

    private static void expandBelow10DigitNumber(string numberString, WordRelation wordRelation)
    {
      expandLargeNumber(numberString, "million", 6, wordRelation);
    }

    private static void expandBelow13DigitNumber([In] string numberString, WordRelation wordRelation)
    {
      expandLargeNumber(numberString, "billion", 9, wordRelation);
    }

    private static void expandLargeNumber(string numberString, string order, int numberZeroes,  WordRelation wordRelation)
    {
        int numberDigits = numberString.Length;

        // parse out the prefix, e.g., "113" in "113,000"
        int i = numberDigits - numberZeroes;
        string part = numberString.Substring(0, i);

        // get how many thousands/millions/billions
        Item oldTail = wordRelation.getTail();
        expandNumber(part, wordRelation);
        if (wordRelation.getTail() != oldTail) {
            wordRelation.addWord(order);
        }
        expandNumberAt(numberString, i, wordRelation);
    }

    private static void expandNumberAt(string numberString, int startIndex, WordRelation wordRelation)
    {
       expandNumber(numberString.Substring(startIndex, numberString.Length), wordRelation);
    }
      
    public static void expandDigits(string numberString, WordRelation wordRelation)
    {
       int numberDigits = numberString.Length;
        for (int i = 0; i < numberDigits; i++) {
            char digit = numberString[i];
            if (char.IsDigit(digit)) {
                wordRelation.addWord(digit2num[numberString[i] - '0']);
            } else {
                wordRelation.addWord("umpty");
            }
        }
    }

    public static void expandOrdinal(string rawNumberString, WordRelation wordRelation)
    {
        expandNumber(rawNumberString.Replace(",", ""), wordRelation);

        // get the last in the list of number strings
        Item lastItem = wordRelation.getTail();

        if (lastItem != null)
        {

            FeatureSet featureSet = lastItem.getFeatures();
            string lastNumber = featureSet.getString("name");
            string ordinal = findMatchInArray(lastNumber, digit2num, ord2num);

            if (ordinal == null)
            {
                ordinal = findMatchInArray(lastNumber, digit2teen, ord2teen);
            }
            if (ordinal == null)
            {
                ordinal = findMatchInArray(lastNumber, digit2enty, ord2enty);
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
                wordRelation.setLastWord(ordinal);
            }
        }
    }

    public static void expandNumess(string rawString, WordRelation wordRelation)
    {
        if (rawString.Length == 4)
        {
            expand2DigitNumber(rawString.Substring(0, 2), wordRelation);
            expandNumess(rawString.Substring(2), wordRelation);
        }
        else
        {
            wordRelation.addWord(digit2Numness[rawString[0] - '0']);
        }
    }

    private static string findMatchInArray( string strToMatch, string[] matchInArray,  string[] returnInArray)
    {
        for (int i = 0; i < matchInArray.Length; i++)
        {
            if (strToMatch.Equals(matchInArray[i]))
            {
                if (i < returnInArray.Length)
                {
                    return returnInArray[i];
                }
                else
                {
                    return null;
                }
            }
        }
        return null;
    }

    public static void expandID(string numberString, WordRelation wordRelation)
    {
        int numberDigits = numberString.Length;

        if ((numberDigits == 4) && (numberString[2] == '0')
                && (numberString[3] == '0'))
        {
            if (numberString[1] == '0')
            { // e.g. 2000, 3000
                expandNumber(numberString, wordRelation);
            }
            else
            {
                expandNumber(numberString.Substring(0, 2), wordRelation);
                wordRelation.addWord("hundred");
            }
        }
        else if ((numberDigits == 2) && (numberString[0] == '0'))
        {
            wordRelation.addWord("oh");
            expandDigits(numberString.Substring(1, 2), wordRelation);
        }
        else if ((numberDigits == 4 && numberString[1] == '0')
              || numberDigits < 3)
        {
            expandNumber(numberString, wordRelation);
        }
        else if (numberDigits % 2 == 1)
        {
            string firstDigit = digit2num[numberString[0] - '0'];
            wordRelation.addWord(firstDigit);
            expandID(numberString.Substring(1, numberDigits), wordRelation);
        }
        else
        {
            expandNumber(numberString.Substring(0, 2), wordRelation);
            expandID(numberString.Substring(2, numberDigits), wordRelation);
        }
   
    }

    public static void expandReal(string numberString, WordRelation wordRelation)
    {
        int stringLength = numberString.Length;
        int position;

        if (numberString[0] == '-')
        {
            // negative real numbers
            wordRelation.addWord("minus");
            expandReal(numberString.Substring(1, stringLength), wordRelation);
        }
        else if (numberString[0] == '+')
        {
            // prefixed with a '+'
            wordRelation.addWord("plus");
            expandReal(numberString.Substring(1, stringLength), wordRelation);
        }
        else if ((position = numberString.IndexOf('e')) != -1
              || (position = numberString.IndexOf('E')) != -1)
        {
            // numbers with 'E' or 'e'
            expandReal(numberString.Substring(0, position), wordRelation);
            wordRelation.addWord("e");
            expandReal(numberString.Substring(position + 1), wordRelation);
        }
        else if ((position = numberString.IndexOf('.')) != -1)
        {
            // numbers with '.'
            string beforeDot = numberString.Substring(0, position);
            if (beforeDot.Length > 0)
            {
                expandReal(beforeDot, wordRelation);
            }
            wordRelation.addWord("point");
            string afterDot = numberString.Substring(position + 1);
            if (afterDot.Length > 0)
            {
                expandDigits(afterDot, wordRelation);
            }
        }
        else
        {
            // everything else
            expandNumber(numberString, wordRelation);
        }
    }

    public static void expandLetters(string letters, WordRelation wordRelation)
    {
        letters = letters.ToLower();
        char c;

        for (int i = 0; i < letters.Length; i++)
        {
            // if this is a number
            c = letters[i];
            if (char.IsDigit(c))
            {
                wordRelation.addWord(digit2num[c - '0']);
            }
            else if (letters.Equals("a"))
            {
                wordRelation.addWord("_a");
            }
            else
            {
                wordRelation.addWord(c.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public static int expandRoman(string roman)
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
