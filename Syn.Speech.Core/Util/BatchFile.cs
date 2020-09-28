using System;
using System.Collections.Generic;
using System.IO;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Provides a set of utilities methods for manipulating batch files
    /// </summary>
    public class BatchFile
    {
            /**
        /// Returns a List of the lines in a batch file.
         *
        /// @param batchFile the batch file to read
        /// @return a List of the lines in a batch file
         */
        public static List<String> GetLines(String batchFile)
        {
            return GetLines(batchFile, 0);
        }


        /**
        /// Returns a List of the lines in a batch file.
         *
        /// @param batchFile the batch file to read
        /// @param skip      the number of lines to skip between items
        /// @return a List of the lines in a batch file
         */
        public static List<String> GetLines(String batchFile, int skip)
        {
            var curCount = skip;
            var list = new List<String>();
            var reader = new StreamReader(batchFile);

            string line;

            while ((line = reader.ReadLine()) != null) 
            {
                if (line.Length!=0) 
                {
                    if (++curCount >= skip) 
                    {
                        list.Add(line);
                        curCount = 0;
                    }
                }
            }
            reader.Close();
            return list;
        }


        /**
        /// Returns the file name portion of a line in a batch file. This is the portion of the line before the first space.
         *
        /// @return the file name portion of a line in a batch file.
         */
        public static string GetFilename(String batchFileLine) 
        {
            var firstSpace = batchFileLine.IndexOf(' ');
            return batchFileLine.Substring(0, firstSpace).Trim();
        }


        /**
        /// Returns the reference string portion of a line in a batch file. This is the portion of the line after the first
        /// space
         *
        /// @return the reference string portion of a line in a batch file.
         */
        public static string GetReference(String batchFileLine) 
        {
            var firstSpace = batchFileLine.IndexOf(' ');
            return batchFileLine.Substring(firstSpace + 1).Trim();
        }

    }
}
