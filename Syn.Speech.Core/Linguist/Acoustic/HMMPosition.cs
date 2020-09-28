using System;
using System.Collections.Generic;
using System.Globalization;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic
{
    public enum HMMPosition
    {
        Begin   =  ('b'), // HMM is at the beginning position of the word
        End     =  ('e'), // HMM is at the end position of the word
        Single   = ('s'), // HMM is at the beginning and the end of the word
        Internal = ('i'), // HMM is completely internal to the word
        Undefined= ('-')// HMM is at an undefined position in the word
    }

    public static class HMMPositionExtension
    {
        private static readonly Dictionary<string, HMMPosition> PosByRep = new Dictionary<string, HMMPosition>();

        static HMMPositionExtension()
        {
            foreach (var item in Enum.GetNames(typeof(HMMPosition)))
            {
                var enumType = (HMMPosition)Enum.Parse(typeof(HMMPosition), item);
                var character = ((char)enumType).ToString(CultureInfo.InvariantCulture);
                PosByRep.Add(character, enumType);
            }
        }

        public static HMMPosition Lookup(string rep)
        {
            return PosByRep[rep];
        }

        public static bool IsWordEnd(this HMMPosition source)
        {
            return source == HMMPosition.Single || source == HMMPosition.End;
        }

        public static bool IsWordBeginning(this HMMPosition source)
        {
            return source == HMMPosition.Single || source == HMMPosition.Begin;
        }

        public static string ToString(this HMMPosition source)
        {
            return ((char)source).ToString(CultureInfo.InvariantCulture);
        }
    }
   
}
