using System;
using System.Collections.Generic;
using System.IO;

namespace Syn.Speech.Test
{
    public static class Helper
    {

        public static string FilesDirectory
        {
            get { return Path.Combine(Directory.GetCurrentDirectory(), "TestFiles"); }
        }

        public static bool Contains<T>(List<T> source, params T[] result)
        {
            foreach (var item in result)
            {
                if (source.Contains(item) == false) return false;
            }
            return true;
        }

        public static bool IsOneOf(string source, params string[] options)
        {
            foreach (var item in options)
            {
                if (source == item) return true;
            }
            return false;
        }

        public static bool CloseTo(double first, double second, double epsilon)
        {
            return Math.Abs(first - second) <= epsilon;
        }
    }
}
