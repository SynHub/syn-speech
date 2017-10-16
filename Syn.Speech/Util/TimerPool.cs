using System;
using System.Collections.Generic;
using System.Diagnostics;
using Syn.Speech.Logging;
//REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Keeps references to a list of timers which can be referenced by a key-pair consisting of an owner and a timer name.
    /// </summary>
    public class TimerPool
    {
        private static readonly Dictionary<Object, List<Timer>> WeakRefTimerPool = new Dictionary<Object, List<Timer>>();

        /// <summary>
        /// Prevents a default instance of the <see cref="TimerPool"/> class from being created.
        /// </summary>
        private TimerPool() {

        }

        /// <summary>
        /// Retrieves (or creates) a timer with the given name
        /// </summary>
        /// <param name="owner">The owner.</param>
        /// <param name="timerName">The name of the particular timer to retrieve. If the timer does not already exist, it will be created  @return the timer.</param>
        /// <returns></returns>
        public static  Timer GetTimer(Object owner, string timerName) 
        {
            if (!WeakRefTimerPool.ContainsKey(owner))
                WeakRefTimerPool.Add(owner, new List<Timer>());

            var ownerTimers = WeakRefTimerPool[owner];

            foreach (var timer in ownerTimers) 
            {
                if (timer.GetName().Equals(timerName))
                    return timer;
            }

            // there is no timer named 'timerName' yet, so create it
            var requestedTimer = new Timer(timerName);
            ownerTimers.Add(requestedTimer);

            return requestedTimer;
        }


        /// <summary>
        /// Returns the number of currently caches {@code Timer} instances.
        /// </summary>
        public static int GetNumCachedTimers() 
        {
            var counter = 0;
            foreach (var timers in WeakRefTimerPool.Values) 
            {
                counter += timers.Count;
            }

            return counter;
        }


        /// <summary>
        /// Dump all timers.
        /// </summary>
        public static void DumpAll() 
        {
            ShowTimesShortTitle();

            foreach (var timers in WeakRefTimerPool.Values) 
            {
                foreach (var timer in timers) 
                {
                    timer.Dump();
                }
            }
        }


        /// <summary>
        /// Shows the timing stats title.
        /// </summary>
        private static void ShowTimesShortTitle() 
        {
            var title = "Timers";
            var titleBar =
                    "# ----------------------------- " + title +
                            "----------------------------------------------------------- ";
            Trace.Write(titleBar);
            Trace.Write("# Name");
            Trace.Write("Count");
            Trace.Write("CurTime");
            Trace.Write("MinTime");
            Trace.Write("MaxTime");
            Trace.Write("AvgTime");
            Logger.LogInfo<TimerPool>("TotTime");
            
        }


        /// <summary>
        /// Resets all timers.
        /// </summary>
        public static void ResetAll() 
        {
            foreach (var timers in WeakRefTimerPool.Values) 
            {
                foreach (var timer in timers) 
                {
                    timer.Reset();
                }
            }
        }

    }
}
