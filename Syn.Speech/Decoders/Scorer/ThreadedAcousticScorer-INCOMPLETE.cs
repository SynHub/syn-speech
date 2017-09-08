using System;
using System.Collections.Generic;
using System.Threading;
using Syn.Speech.FrontEnds;
using Syn.Speech.Util.Props;

namespace Syn.Speech.Decoders.Scorer
{
    /// <summary>
    /// An acoustic scorer that breaks the scoring up into a configurable number of separate threads.
    /// 
    /// All scores are maintained in LogMath log base
    /// </summary>
    public class ThreadedAcousticScorer : SimpleAcousticScorer
    {
        /**
        /// The property that controls the thread priority of scoring threads.
        /// Must be a value between {@link Thread#MIN_PRIORITY} and {@link Thread#MAX_PRIORITY}, inclusive.
        /// The default is {@link Thread#NORM_PRIORITY}.
         */
        [S4Integer(DefaultValue = (int)ThreadPriority.Normal)]
        public static string PropThreadPriority = "threadPriority";

        /**
        /// The property that controls the number of threads that are used to score HMM states. If the isCpuRelative
        /// property is false, then is is the exact number of threads that are used to score HMM states. If the isCpuRelative
        /// property is true, then this value is combined with the number of available processors on the system. If you want
        /// to have one thread per CPU available to score states, set the NUM_THREADS property to 0 and the isCpuRelative to
        /// true. If you want exactly one thread to process scores set NUM_THREADS to 1 and isCpuRelative to false.
        /// <p/>
        /// If the value is 1 isCpuRelative is false no additional thread will be instantiated, and all computation will be
        /// done in the calling thread itself. The default value is 0.
         */
        [S4Integer(DefaultValue = 0)]
        public static string PropNumThreads = "numThreads";

        /**
        /// The property that controls whether the number of available CPUs on the system is used when determining
        /// the number of threads to use for scoring. If true, the NUM_THREADS property is combined with the available number
        /// of CPUS to determine the number of threads. Note that the number of threads is contained to be never lower than
        /// zero. Also, if the number of threads is 0, the states are scored on the calling thread, no separate threads are
        /// started. The default value is false.
         */
        [S4Boolean(DefaultValue = true)]
        public static string PropIsCpuRelative = "isCpuRelative";

        /**
        /// The property that controls the minimum number of scoreables sent to a thread. This is used to prevent
        /// over threading of the scoring that could happen if the number of threads is high compared to the size of the
        /// active list. The default is 50
         */
        [S4Integer(DefaultValue = 10)]
        public static string PropMinScoreablesPerThread = "minScoreablesPerThread";

        private static readonly string ClassName = typeof(ThreadedAcousticScorer).Name;

        private int _numThreads;         // number of threads in use
        private int _threadPriority;
        private int _minScoreablesPerThread; // min scoreables sent to a thread
        //private concu executorService;

        /**
        /// @param frontEnd
        ///            the frontend to retrieve features from for scoring
        /// @param scoreNormalizer
        ///            optional post-processor for computed scores that will
        ///            normalize scores. If not set, no normalization will applied
        ///            and the token scores will be returned unchanged.
        /// @param minScoreablesPerThread
        ///            the number of threads that are used to score HMM states. If
        ///            the isCpuRelative property is false, then is is the exact
        ///            number of threads that are used to score HMM states. If the
        ///            isCpuRelative property is true, then this value is combined
        ///            with the number of available processors on the system. If you
        ///            want to have one thread per CPU available to score states, set
        ///            the NUM_THREADS property to 0 and the isCpuRelative to true.
        ///            If you want exactly one thread to process scores set
        ///            NUM_THREADS to 1 and isCpuRelative to false.
        ///            <p/>
        ///            If the value is 1 isCpuRelative is false no additional thread
        ///            will be instantiated, and all computation will be done in the
        ///            calling thread itself. The default value is 0.
        /// @param cpuRelative
        ///            controls whether the number of available CPUs on the system is
        ///            used when determining the number of threads to use for
        ///            scoring. If true, the NUM_THREADS property is combined with
        ///            the available number of CPUS to determine the number of
        ///            threads. Note that the number of threads is constrained to be
        ///            never lower than zero. Also, if the number of threads is 0,
        ///            the states are scored on the calling thread, no separate
        ///            threads are started. The default value is false.
        /// @param numThreads
        ///            the minimum number of scoreables sent to a thread. This is
        ///            used to prevent over threading of the scoring that could
        ///            happen if the number of threads is high compared to the size
        ///            of the active list. The default is 50
        /// @param threadPriority
        ///            the thread priority of scoring threads. Must be a value between
        ///            {@link Thread#MIN_PRIORITY} and {@link Thread#MAX_PRIORITY}, inclusive.
        ///            The default is {@link Thread#NORM_PRIORITY}.
         */
        public ThreadedAcousticScorer(BaseDataProcessor frontEnd, IScoreNormalizer scoreNormalizer,
                                      int minScoreablesPerThread, Boolean cpuRelative, int numThreads, int threadPriority) 
            :base(frontEnd, scoreNormalizer)
        {
            
            Init(minScoreablesPerThread, cpuRelative, numThreads, threadPriority);
        }

        public ThreadedAcousticScorer() 
        {
        }

        
        public override void NewProperties(PropertySheet ps)
        {
            base.NewProperties(ps);
            Init(ps.GetInt(PropMinScoreablesPerThread), 
                ps.GetBoolean(PropIsCpuRelative),
                ps.GetInt(PropNumThreads), 
                ps.GetInt(PropThreadPriority));
        }

        private void Init(int minScoreablesPerThread, Boolean cpuRelative, int numThreads, int threadPriority) 
        {
            _minScoreablesPerThread = minScoreablesPerThread;
            if (cpuRelative) 
            {
                numThreads += Environment.ProcessorCount;
            }
            _numThreads = numThreads;
            _threadPriority = threadPriority;
        }

        public override void Allocate() {
            base.Allocate();
        }

        public override void Deallocate() {
            base.Deallocate();
        }

        
        protected override IScoreable DoScoring<T>(List<T> scoreableList, IData data) 
        {
        // if no additional threads are necessary, do the scoring in the calling thread
        return base.DoScoring(scoreableList, data);
        }

    }
}
