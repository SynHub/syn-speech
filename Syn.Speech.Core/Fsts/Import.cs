using System;
using System.IO;
//PATROLLED + REFACTORED
using Syn.Speech.Fsts.Semirings;

namespace Syn.Speech.Fsts
{
    /// <summary>
    /// Provides a command line utility to convert an Fst in openfst's text format to java binary fst model.
    /// @author John Salatas "jsalatas@users.sourceforge.net"
    /// </summary>
    public class Import
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="Import"/> class from being created.
        /// </summary>
        private Import() { }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Input and output files not provided");
                Console.WriteLine("You need to provide both the input binary openfst model");
                Console.WriteLine("and the output serialized java fst model.");
                Environment.Exit(1);
            }

            //Serialize the java fst model to disk
            Fst fst = Convert.ImportFst(args[0], new TropicalSemiring());

            Console.WriteLine("Saving as binar java fst model...");
            try
            {
                fst.SaveModel(args[1]);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Cannot write to file " + args[1]);
                Environment.Exit(1);
            }
        }
    }
}
