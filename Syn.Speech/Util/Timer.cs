using System;
using System.Diagnostics;
using Syn.Speech.Logging;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Keeps track of execution times. This class provides methods that can be used for timing processes. The process to be
    /// timed should be bracketed by calls to timer.start() and timer.stop().  Repeated operations can be timed more than
    /// once. The timer will report the minimum, maximum, average and last time executed for all start/stop pairs when the
    /// timer.dump is called.
    /// Timer instances can be obtained from a global cache implemented in {@code TimerPool}.
    /// </summary>
    /// <see cref="TimerPool"/>
    public class Timer
    {
        private const string TimeFormatter = "{###0.0000}";

        private readonly string _name;

        private double _sum;
        private long _startTime;
        private Boolean _notReliable; // if true, timing is not reliable

        /// <summary>
        /// Creates a timer.
        /// </summary>
        /// <param name="name"></param>
        public Timer(string name) 
        {
            MinTime = JLong.MAX_VALUE;
            Debug.Assert( name != null , "timers must have a name!");
            _name = name;
            Reset();
        }

        /// <summary>
        /// Retrieves the name of the timer
        /// </summary>
        /// <returns></returns>
        public string GetName() 
        {
            return _name;
        }


        /// <summary>
        /// Resets the timer as if it has never run before.
        /// </summary>
        public void Reset() 
        {
            _startTime = 0L;
            Count = 0L;
            _sum = 0L;
            MinTime = JLong.MAX_VALUE;
            MaxTime = 0L;
            _notReliable = false;
        }

        /// <summary>
        /// Returns true if the timer has started.
        /// </summary>
        /// <returns></returns>
        public Boolean IsStarted() 
        {
            return (_startTime > 0L);
        }

        /// <summary>
        /// Starts the timer running. 
        /// </summary>
        public void Start() 
        {
            if (_startTime != 0L) 
            {
                _notReliable = true; // start called while timer already running
                this.LogInfo(GetName() + " timer.start() called without a stop()");
            }
            _startTime = Java.CurrentTimeMillis();
        }


        /// <summary>
        /// Starts the timer at the given time.
        /// </summary>
        /// <param name="time"></param>
        public void Start(long time) 
        {
            if (_startTime != 0L) 
            {
                _notReliable = true; // start called while timer already running
                this.LogInfo(GetName() + " timer.start() called without a stop()");
            }

            if (time > Java.CurrentTimeMillis()) 
            {
                throw new RankException ("Start time is later than current time");
            }
            _startTime = time;
        }




        /// <summary>
        /// Stops the timer.
        /// </summary>
        /// <returns>the duration since start in milliseconds</returns>
        public long Stop()
        {
            if (_startTime == 0L) 
            {
                _notReliable = true;        // stop called, but start never called
                this.LogInfo(GetName() + " timer.stop() called without a start()");
            }
            CurTime = Java.CurrentTimeMillis() - _startTime;
            _startTime = 0L;
            if (CurTime > MaxTime) {
                MaxTime = CurTime;
            }
            if (CurTime < MinTime) {
                MinTime = CurTime;
            }
            Count++;
            _sum += CurTime;
            return CurTime;
        }



        /// <summary>
        /// Dump the timer. Shows the timer details.
        /// </summary>
        public void Dump() 
        {
            ShowTimesShort();
        }


        /**
        /// Gets the count of starts for this timer
         *
        /// @return the count
         */

        public long Count { get; private set; }


        /**
        /// Returns the latest time gathered
         *
        /// @return the time in milliseconds
         */

        public long CurTime { get; private set; }


        /**
        /// Gets the average time for this timer in milliseconds
         *
        /// @return the average time
         */
        public double GetAverageTime() 
        {
            if (Count == 0) 
            {
                return 0.0;
            }
            return _sum / Count;
        }

        /// <summary>
        /// Gets the min time for this timer in milliseconds.
        /// </summary>
        /// <value>
        /// The min time
        /// </value>
        public long MinTime { get; private set; }

        /// <summary>
        /// Gets the max time for this timer in milliseconds.
        /// </summary>
        /// <value>
        /// The max time in milliseconds.
        /// </value>
        public long MaxTime { get; private set; }

        /// <summary>
        /// Formats times into a standard format.
        /// </summary>
        /// <param name="time">The time (in seconds) to be formatted.</param>
        /// <returns>A string representation of the time.</returns>
        private static string FmtTime(double time)
        {
            return time.ToString(TimeFormatter);
            //return String.Format(timeFormatter, time);
        }


        /// <summary>
        ///  Shows brief timing statistics.
        /// </summary>
        private void ShowTimesShort() 
        {
            var avgTime = 0.0;

            if (Count == 0) {
                return;
            }

            if (Count > 0) {
                avgTime = _sum / Count / 1000.0;
            }

            if (_notReliable) 
            {
                this.LogInfo(_name);
                this.LogInfo("Not reliable.");
            } 
            else 
            {
                this.LogInfo(_name);
                this.LogInfo(Count);
                this.LogInfo(FmtTime(CurTime));
                this.LogInfo(FmtTime(MinTime));
                this.LogInfo(FmtTime(MaxTime));
                this.LogInfo(FmtTime(avgTime));
                this.LogInfo(FmtTime(_sum / 1000.0));
                this.LogInfo(String.Empty);
            }
        }
    }
}
