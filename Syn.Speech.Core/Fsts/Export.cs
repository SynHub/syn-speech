using System;
//PATROLLED + REFACTORED
namespace Syn.Speech.Fsts
{
    public class Export
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="Export"/> class from being created.
        /// </summary>
        private Export() { }

        public static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Input and output files are not provided");
                Console.WriteLine("You need to provide both the input serialized java fst model");
                Console.WriteLine("and the output binary openfst model.");
                Environment.Exit(1);
            }

            Fst fst = Fst.LoadModel(args[0]);
            Console.WriteLine("Saving as openfst text model...");
            Convert.Export(fst, args[1]);
        }
    }
}
