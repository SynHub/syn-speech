using System.Diagnostics;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Helper.Math;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
//PATROLLED
namespace Syn.Speech.Decoder.Adaptation
{
    public class Transform
    {
        private float[][][][] As;
        private float[][][] Bs;
        private Sphinx3Loader loader;
        private int nrOfClusters;

        public Transform(Sphinx3Loader loader, int nrOfClusters)
        {
            this.loader = loader;
            this.nrOfClusters = nrOfClusters;
        }

        /// <summary>
        /// Used for access to A matrix.
        /// </summary>
        /// <returns>A matrix (representing A from A*x + B = C)</returns>
        public float[][][][] getAs()
        {
            return As;
        }

        /// <summary>
        /// Used for access to B matrix.
        /// </summary>
        /// <returns>B matrix (representing B from A*x + B = C)</returns>
        public float[][][] getBs()
        {
            return Bs;
        }

        /// <summary>
        /// Writes the transformation to file in a format that could further be used in Sphinx3 and Sphinx4.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="index">The index.</param>
        public void store(string filePath, int index)
        {
            //PrintWriter writer = new PrintWriter(filePath, "UTF-8");
            var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // nMllrClass
            writer.WriteLine("1");
            writer.WriteLine(loader.getNumStreams());

            for (var i = 0; i < loader.getNumStreams(); i++)
            {
                writer.WriteLine(loader.getVectorLength()[i]);

                for (var j = 0; j < loader.getVectorLength()[i]; j++)
                {
                    for (var k = 0; k < loader.getVectorLength()[i]; ++k)
                    {
                        writer.Write(As[index][i][j][k]);
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                }

                for (var j = 0; j < loader.getVectorLength()[i]; j++)
                {
                    writer.Write(Bs[index][i][j]);
                    writer.Write(" ");

                }
                writer.WriteLine();

                for (var j = 0; j < loader.getVectorLength()[i]; j++)
                {
                    writer.Write("1.0 ");

                }
                writer.WriteLine();
            }
            writer.Close();
        }

        /// <summary>
        /// Used for computing the actual transformations (A and B matrices). These are stored in As and Bs.
        /// </summary>
        /// <param name="regLs">The reg ls.</param>
        /// <param name="regRs">The reg rs.</param>
        private void computeMllrTransforms(double[][][][][] regLs,
                double[][][][] regRs)
        {
            int len;
          
            for (int c = 0; c < nrOfClusters; c++)
            {
                this.As[c] = new float[loader.getNumStreams()][][];
                this.Bs[c] = new float[loader.getNumStreams()][];

                for (int i = 0; i < loader.getNumStreams(); i++)
                {
                    len = loader.getVectorLength()[i];
                    //TODO: CHECK SEMANTICS
                    //this.As[c][i] = new float[len][len];
                    this.As[c][i] = new float[len][];
                    this.Bs[c][i] = new float[len];

                    for (int j = 0; j < len; ++j)
                    {
                        var coef = new Array2DRowRealMatrix(regLs[c][i][j], false);
                        var solver = new LUDecomposition(coef).getSolver();
                        var vect = new ArrayRealVector(regRs[c][i][j], false);
                        var ABloc = solver.solve(vect);

                        for (int k = 0; k < len; ++k)
                        {
                            this.As[c][i][j][k] = (float)ABloc.getEntry(k);
                        }

                        this.Bs[c][i][j] = (float)ABloc.getEntry(len);
                    }
                }
            }
        }

        /// <summary>
        ///Read the transformation from a file
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void load(string filePath)
        {
            //TODO: IMPLEMENT A LESS MEMORY CONSUMING METHOD
            var input = new Scanner(File.ReadAllText(filePath));
            int numStreams, nMllrClass;
            int[] vectorLength = new int[1];

            nMllrClass = input.nextInt();

            Trace.Assert(nMllrClass == 1);

            numStreams = input.nextInt();

            this.As = new float[nMllrClass][][][];
            this.Bs = new float[nMllrClass][][];

            for (int i = 0; i < numStreams; i++)
            {
                vectorLength[i] = input.nextInt();

                int length = vectorLength[i];

                //TODO: CHECK SEMANTICS
                //this.As[0] = new float[numStreams][length][length];
                //this.Bs[0] = new float[numStreams][length];
                this.As[0] = new float[numStreams][][];
                this.Bs[0] = new float[numStreams][];

                for (int j = 0; j < length; j++)
                {
                    for (int k = 0; k < length; ++k)
                    {
                        As[0][i][j][k] = input.nextFloat();
                    }
                }

                for (int j = 0; j < length; j++)
                {
                    Bs[0][i][j] = input.nextFloat();
                }
            }
            //input.close();
        }

        /// <summary>
        /// tores in current object a transform generated on the provided stats.
        /// </summary>
        /// <param name="stats">Provided stats that were previously collected from Result objects..</param>
        public void update(Stats stats)
        {
            stats.fillRegLowerPart();
            As = new float[nrOfClusters][][][];
            Bs = new float[nrOfClusters][][];
            this.computeMllrTransforms(stats.getRegLs(), stats.getRegRs());
        }
    }
}
