using System;
using Syn.Logging;
using Syn.Speech.Decoders.Adaptation;
using Syn.Speech.Linguist.Acoustic.Tiedstate;
using Syn.Speech.Recognizers;


//PATROLLED + REFACTORED
namespace Syn.Speech.Api
{
    /// <summary>
    /// Base class for high-level speech recognizers.
    /// </summary>
    public class AbstractSpeechRecognizer
    {
        protected Context Context;
        protected Recognizer Recognizer;
    
        protected ClusteredDensityFileData Clusters;

        protected readonly SpeechSourceProvider SpeechSourceProvider;

        /// <summary>
        /// Constructs recognizer object using provided configuration.
        /// </summary>
        /// <param name="configuration"></param>
        public AbstractSpeechRecognizer(Configuration configuration):this(new Context(configuration))
        {

        }

        protected AbstractSpeechRecognizer(Context context) 
        {
            try
            {
                Context = context;
                Recognizer = context.GetInstance(typeof(Recognizer)) as Recognizer;
                SpeechSourceProvider = new SpeechSourceProvider();
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }

        /// <summary>
        /// Returns result of the recognition.
        /// </summary>
        /// <returns></returns>
        public SpeechResult GetResult() 
        {
            try
            {
                var result = Recognizer.Recognize();
                return null == result ? null : new SpeechResult(result);
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
            return null;
        }
    
        public Stats CreateStats(int numClasses) 
        {
            Clusters = new ClusteredDensityFileData(Context.GetLoader(), numClasses);
            return new Stats(Context.GetLoader(), Clusters);
        }

        public void SetTransform(Transform transform) 
        {
            if (Clusters != null) {
                Context.GetLoader().Update(transform, Clusters);
            }
        }

        public void LoadTransform(String path, int numClass)
        {
            try
            {
                Clusters = new ClusteredDensityFileData(Context.GetLoader(), numClass);
                var transform = new Transform((Sphinx3Loader)Context.GetLoader(), numClass);
                transform.Load(path);
                Context.GetLoader().Update(transform, Clusters);
            }
            catch (Exception exception)
            {
                this.LogError(exception);
            }
        }
    }
}
