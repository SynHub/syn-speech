using System.Diagnostics;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Helper.Mathematics.Linear;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
//PATROLLED + REFACTORED
namespace Syn.Speech.Decoders.Adaptation
{
    public class Transform
    {
        private readonly Sphinx3Loader _loader;
        private readonly int _nrOfClusters;

        public Transform(Sphinx3Loader loader, int nrOfClusters)
        {
            _loader = loader;
            _nrOfClusters = nrOfClusters;
        }

        /// <summary>
        /// Used for access to A matrix.
        /// </summary>
        /// <value>A matrix (representing A from A*x + B = C)</value>
        public float[][][][] As { get; private set; }

        /// <summary>
        /// Used for access to B matrix.
        /// </summary>
        /// <value>B matrix (representing B from A*x + B = C)</value>
        public float[][][] Bs { get; private set; }

        /// <summary>
        /// Writes the transformation to file in a format that could further be used in Sphinx3 and Sphinx4.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="index">The index.</param>
        public void Store(string filePath, int index)
        {
            //PrintWriter writer = new PrintWriter(filePath, "UTF-8");
            var writer = new StreamWriter(filePath, false, Encoding.UTF8);

            // nMllrClass
            writer.WriteLine("1");
            writer.WriteLine(_loader.NumStreams);

            for (var i = 0; i < _loader.NumStreams; i++)
            {
                writer.WriteLine(_loader.VectorLength[i]);

                for (var j = 0; j < _loader.VectorLength[i]; j++)
                {
                    for (var k = 0; k < _loader.VectorLength[i]; ++k)
                    {
                        writer.Write(As[index][i][j][k]);
                        writer.Write(" ");
                    }
                    writer.WriteLine();
                }

                for (var j = 0; j < _loader.VectorLength[i]; j++)
                {
                    writer.Write(Bs[index][i][j]);
                    writer.Write(" ");

                }
                writer.WriteLine();

                for (var j = 0; j < _loader.VectorLength[i]; j++)
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
        private void ComputeMllrTransforms(double[][][][][] regLs, double[][][][] regRs)
        {
            int len;
          
            for (int c = 0; c < _nrOfClusters; c++)
            {
                As[c] = new float[_loader.NumStreams][][];
                Bs[c] = new float[_loader.NumStreams][];

                for (int i = 0; i < _loader.NumStreams; i++)
                {
                    len = _loader.VectorLength[i];
                    As[c][i] = Java.CreateArray<float[][]>(len, len); //this.As[c][i] = new float[len][len];
                    Bs[c][i] = new float[len];

                    for (int j = 0; j < len; ++j)
                    {
                        var coef = new Array2DRowRealMatrix(regLs[c][i][j], false);
                        var solver = new LUDecomposition(coef).getSolver();
                        var vect = new ArrayRealVector(regRs[c][i][j], false);
                        var aBloc = solver.solve(vect);

                        for (int k = 0; k < len; ++k)
                        {
                            As[c][i][j][k] = (float)aBloc.getEntry(k);
                        }

                        Bs[c][i][j] = (float)aBloc.getEntry(len);
                    }
                }
            }
        }

        /// <summary>
        ///Read the transformation from a file
        /// </summary>
        /// <param name="filePath">The file path.</param>
        public void Load(string filePath)
        {
            //TODO: IMPLEMENT A LESS MEMORY CONSUMING METHOD
            var input = new Scanner(File.ReadAllText(filePath));
            int numStreams, nMllrClass;
            int[] vectorLength = new int[1];

            nMllrClass = input.NextInt();

            Debug.Assert(nMllrClass == 1);

            numStreams = input.NextInt();

            As = new float[nMllrClass][][][];
            Bs = new float[nMllrClass][][];

            for (int i = 0; i < numStreams; i++)
            {
                vectorLength[i] = input.NextInt();

                int length = vectorLength[i];
                As[0] = Java.CreateArray<float[][][]>(numStreams, length, length);//this.As[0] = new float[numStreams][length][length];
                Bs[0] = Java.CreateArray<float[][]>(numStreams, length);//this.Bs[0] = new float[numStreams][length];

                for (int j = 0; j < length; j++)
                {
                    for (int k = 0; k < length; ++k)
                    {
                        As[0][i][j][k] = input.NextFloat();
                    }
                }

                for (int j = 0; j < length; j++)
                {
                    Bs[0][i][j] = input.NextFloat();
                }
            }
            //input.close();
        }

        /// <summary>
        /// Stores in current object a transform generated on the provided stats.
        /// </summary>
        /// <param name="stats">Provided stats that were previously collected from Result objects..</param>
        public void Update(Stats stats)
        {
            stats.FillRegLowerPart();
            As = new float[_nrOfClusters][][][];
            Bs = new float[_nrOfClusters][][];
            ComputeMllrTransforms(stats.RegLs, stats.RegRs);
        }
    }
}
