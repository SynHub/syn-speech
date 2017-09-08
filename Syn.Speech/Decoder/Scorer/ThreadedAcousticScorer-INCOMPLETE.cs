//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Threading;
//using Syn.Speech.Common.FrontEnd;
//using Syn.Speech.FrontEnd;
//using Syn.Speech.Util.Props;

//namespace Syn.Speech.Decoder.Scorer
//{
//    /// <summary>
//    /// An acoustic scorer that breaks the scoring up into a configurable number of separate threads.
//    /// 
//    /// All scores are maintained in LogMath log base
//    /// </summary>
//    public class ThreadedAcousticScorer : SimpleAcousticScorer
//    {
//        /**
//        /// The property that controls the thread priority of scoring threads.
//        /// Must be a value between {@link Thread#MIN_PRIORITY} and {@link Thread#MAX_PRIORITY}, inclusive.
//        /// The default is {@link Thread#NORM_PRIORITY}.
//         */
//        [S4Integer(defaultValue = (int)ThreadPriority.Normal)]
//        public static string PROP_THREAD_PRIORITY = "threadPriority";

//        /**
//        /// The property that controls the number of threads that are used to score HMM states. If the isCpuRelative
//        /// property is false, then is is the exact number of threads that are used to score HMM states. If the isCpuRelative
//        /// property is true, then this value is combined with the number of available processors on the system. If you want
//        /// to have one thread per CPU available to score states, set the NUM_THREADS property to 0 and the isCpuRelative to
//        /// true. If you want exactly one thread to process scores set NUM_THREADS to 1 and isCpuRelative to false.
//        /// <p/>
//        /// If the value is 1 isCpuRelative is false no additional thread will be instantiated, and all computation will be
//        /// done in the calling thread itself. The default value is 0.
//         */
//        [S4Integer(defaultValue = 0)]
//        public static string PROP_NUM_THREADS = "numThreads";

//        /**
//        /// The property that controls whether the number of available CPUs on the system is used when determining
//        /// the number of threads to use for scoring. If true, the NUM_THREADS property is combined with the available number
//        /// of CPUS to determine the number of threads. Note that the number of threads is contained to be never lower than
//        /// zero. Also, if the number of threads is 0, the states are scored on the calling thread, no separate threads are
//        /// started. The default value is false.
//         */
//        [S4Boolean(defaultValue = true)]
//        public static string PROP_IS_CPU_RELATIVE = "isCpuRelative";

//        /**
//        /// The property that controls the minimum number of scoreables sent to a thread. This is used to prevent
//        /// over threading of the scoring that could happen if the number of threads is high compared to the size of the
//        /// active list. The default is 50
//         */
//        [S4Integer(defaultValue = 10)]
//        public static string PROP_MIN_SCOREABLES_PER_THREAD = "minScoreablesPerThread";

//        private static string className = typeof(ThreadedAcousticScorer).Name;

//        private int numThreads;         // number of threads in use
//        private int threadPriority;
//        private int minScoreablesPerThread; // min scoreables sent to a thread
//        private ExecutorService executorService;

//        /**
//        /// @param frontEnd
//        ///            the frontend to retrieve features from for scoring
//        /// @param scoreNormalizer
//        ///            optional post-processor for computed scores that will
//        ///            normalize scores. If not set, no normalization will applied
//        ///            and the token scores will be returned unchanged.
//        /// @param minScoreablesPerThread
//        ///            the number of threads that are used to score HMM states. If
//        ///            the isCpuRelative property is false, then is is the exact
//        ///            number of threads that are used to score HMM states. If the
//        ///            isCpuRelative property is true, then this value is combined
//        ///            with the number of available processors on the system. If you
//        ///            want to have one thread per CPU available to score states, set
//        ///            the NUM_THREADS property to 0 and the isCpuRelative to true.
//        ///            If you want exactly one thread to process scores set
//        ///            NUM_THREADS to 1 and isCpuRelative to false.
//        ///            <p/>
//        ///            If the value is 1 isCpuRelative is false no additional thread
//        ///            will be instantiated, and all computation will be done in the
//        ///            calling thread itself. The default value is 0.
//        /// @param cpuRelative
//        ///            controls whether the number of available CPUs on the system is
//        ///            used when determining the number of threads to use for
//        ///            scoring. If true, the NUM_THREADS property is combined with
//        ///            the available number of CPUS to determine the number of
//        ///            threads. Note that the number of threads is constrained to be
//        ///            never lower than zero. Also, if the number of threads is 0,
//        ///            the states are scored on the calling thread, no separate
//        ///            threads are started. The default value is false.
//        /// @param numThreads
//        ///            the minimum number of scoreables sent to a thread. This is
//        ///            used to prevent over threading of the scoring that could
//        ///            happen if the number of threads is high compared to the size
//        ///            of the active list. The default is 50
//        /// @param threadPriority
//        ///            the thread priority of scoring threads. Must be a value between
//        ///            {@link Thread#MIN_PRIORITY} and {@link Thread#MAX_PRIORITY}, inclusive.
//        ///            The default is {@link Thread#NORM_PRIORITY}.
//         */
//        public ThreadedAcousticScorer(BaseDataProcessor frontEnd, IScoreNormalizer scoreNormalizer,
//                                      int minScoreablesPerThread, Boolean cpuRelative, int numThreads, int threadPriority) 
//            :base(frontEnd, scoreNormalizer)
//        {
            
//            init(minScoreablesPerThread, cpuRelative, numThreads, threadPriority);
//        }

//        public ThreadedAcousticScorer() 
//        {
//        }

//        override
//        public void newProperties(PropertySheet ps)
//        {
//            base.newProperties(ps);
//            init(ps.getInt(PROP_MIN_SCOREABLES_PER_THREAD), 
//                ps.getBoolean(PROP_IS_CPU_RELATIVE),
//                ps.getInt(PROP_NUM_THREADS), 
//                ps.getInt(PROP_THREAD_PRIORITY));
//        }

//        private void init(int minScoreablesPerThread, Boolean cpuRelative, int numThreads, int threadPriority) 
//        {
//            this.minScoreablesPerThread = minScoreablesPerThread;
//            if (cpuRelative) 
//            {
//                numThreads += Environment.ProcessorCount;
//            }
//            this.numThreads = numThreads;
//            this.threadPriority = threadPriority;
//        }

//        public void allocate() {
//            base.allocate();

//            if (executorService == null)
//            {
//                if (numThreads > 1)
//                {
//                    Trace.WriteLine("# of scoring threads: " + numThreads);
//                    ThreadPool.QueueUserWorkItem(new WaitCallback())
//                    executorService = Executors.newFixedThreadPool(numThreads, new CustomThreadFactory(className, true, threadPriority));
//                }
//                else
//                {
//                    Trace.WriteLine("no scoring threads");
//                }
//            }
//        }

//        public void deallocate() {
//            base.deallocate();
//            if (executorService != null)
//            {
//                executorService.shutdown();
//                executorService = null;
//            }
//        }

        
//         protected internal virtual T doScoring<T>(List<T> scoreableList, IData data) where T : IScoreable
//        {
//                    if (numThreads > 1)
//        {
//            int totalSize = scoreableList.Count;
//            int jobSize = Math.Max((totalSize + numThreads - 1) / numThreads, minScoreablesPerThread);

//            if (jobSize < totalSize)
//            {
//                List<Callable<T>> tasks = new List<Callable<T>>();
//                for (int from = 0, to = jobSize; from < totalSize; from = to, to += jobSize)
//                {
//                    List<T> scoringJob = scoreableList.GetRange(@from, Math.Min(to, totalSize));
//                    tasks.Add(new Callable<T>()
//                    {
//                        public T call() 
//                        {
//                            return ThreadedAcousticScorer.super.doScoring(scoringJob, data);
//                        }
//                        }
//                   );
//                }

//                List<T> finalists = new List<T>(tasks.Count);

//                foreach (Future<T> result in executorService.invokeAll(tasks))
//                    finalists.Add(result.get());

//                if (finalists.Count == 0)
//                {
//                    throw new DataProcessingException("No scoring jobs ended");
//                }

//                return Collections.min(finalists, Scoreable.COMPARATOR);
//            }
//        }
//        // if no additional threads are necessary, do the scoring in the calling thread
//        return base.doScoring(scoreableList, data);
//        }

//    }
//}
