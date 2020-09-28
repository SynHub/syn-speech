using System;
using System.Collections.Generic;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Fsts.Utils
{
    /// <summary>
    /// Several general use utility functions needed by the fst framework
    /// 
    /// @author John Salatas <jsalatas@users.sourceforge.net>
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// Searches an ArrayList of Strings starting from a specific position for a pattern
        /// </summary>
        /// <param name="src">The input ArrayList of Strings.</param>
        /// <param name="pattern">The pattern to search for.</param>
        /// <param name="start">The starting position.</param>
        /// <returns>The index of the first occurrence or -1 if no matches found.</returns>
        public static int Search(List<String> src, List<String> pattern, int start) 
        {
            int index = -1;
            int pos = -1;
            int startpos = 0;
            if (start > src.Count - pattern.Count) 
            {
                return -1;
            }

            do {
                pos = src.GetRange(startpos + start,src.Count - pattern.Count + 1)
                        .IndexOf(pattern[0]);
                if (pos == -1) {
                    return pos;
                }

                Boolean flag = true;
                for (int i = 1; i < pattern.Count; i++) 
                {
                    if (!src[startpos + start + pos + i].Equals(pattern[i])) 
                    {
                        index = -1;
                        flag = false;
                        break;
                    }
                }

                if (flag) {
                    index = startpos + pos;
                    break;
                } 
                else {
                    startpos += pos + 1;
                }
            } while (startpos + start < src.Count);

            return index;
        }

        /// <summary>
        /// Get the position (index) of a particular string in a Strings array.
        /// The search is case insensitive.
        /// </summary>
        /// <param name="strings">The Strings array.</param>
        /// <param name="_string">The string to search.</param>
        /// <returns>The index of the first occurrence or -1 if no matches found.</returns>
        public static int GetIndex(String[] strings, string _string) 
        {
            for (int i = 0; i < strings.Length; i++) 
            {
                if (_string.ToLower().Equals(strings[i].ToLower())) 
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Convert a HashMap<String, Integer> to Strings array
        /// </summary>
        /// <param name="syms">The input HashMap.</param>
        /// <returns>The Strings array</returns>
        public static string[] ToStringArray(HashMap<String, Integer> syms) 
        {
            String[] res = new String[syms.Count];
            foreach (String sym in syms.Keys) 
            {
                res[syms.Get(sym)] = sym;
            }
            return res;
        }
    }
}
