using Syn.Speech.Decoders.Adaptation;
using Syn.Speech.Helper;
using Syn.Speech.Util.Props;
//REFACTORED
namespace Syn.Speech.Linguist.Acoustic.Tiedstate
{
    public interface ILoader : IConfigurable
    {
        /// <summary>
        /// Loads the acoustic model.
        /// </summary>
        void Load();

        /// <summary>
        /// ets the pool of means for this loader.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[]> MeansPool { get; }

        /// <summary>
        /// Gets the means transformation matrix pool.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[][]> MeansTransformationMatrixPool { get; }


        /// <summary>
        /// Gets the means transformation vector pool.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[]> MeansTransformationVectorPool { get; }


        /// <summary>
        /// Gets the variance pool.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[]> VariancePool { get; }


        /// <summary>
        /// Gets the variance transformation matrix pool.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[][]> VarianceTransformationMatrixPool { get; }

        /// <summary>
        /// Gets the variance transformation vector pool.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[]> VarianceTransformationVectorPool { get; }

        /// <summary>
        /// Gets the mixture weights pool.
        /// </summary>
        /// <value>The pool.</value>
        GaussianWeights MixtureWeightsPool { get; }

        /// <summary>
        /// Gets the transition matrix pool.
        /// </summary>
        /// <value>The pool.</value>
        Pool<float[][]> TransitionMatrixPool { get; }

        /// <summary>
        /// Gets the transformation matrix.
        /// </summary>
        /// <value>The matrix.</value>
        float[][] TransformMatrix { get; }

        /// <summary>
        /// Gets the senone pool for this loader.
        /// </summary>
        /// <value>The pool.</value>
        Pool<ISenone> SenonePool { get; }

        /// <summary>
        /// Gets the HMM Manager for this loader.
        /// </summary>
        /// <value>The HMM Manager.</value>
        HMMManager HMMManager { get; }

        /// <summary>
        /// Gets the map of context indepent units. The map can be accessed by unit name.
        /// </summary>
        /// <value>The map of context independent units.</value>
        LinkedHashMap<string, Unit> ContextIndependentUnits { get; }

        /// <summary>
        /// Logs information about this loader 
        /// </summary>
        void LogInfo();

        /// <summary>
        /// Gets the size of the left context for context dependent units.
        /// </summary>
        /// <value>The left context size.</value>
        int LeftContextSize { get; }


        /// <summary>
        /// Gets the size of the right context for context dependent units.
        /// </summary>
        /// <value>The left context size.</value>
        int RightContextSize { get; }

        /// <summary>
        /// Gets the model properties.
        /// </summary>
        /// <value></value>
        JProperties Properties { get; }

        /// <summary>
        /// Apply the transform
        /// </summary>
        void Update(Transform transform, ClusteredDensityFileData clusters);
    }
}
