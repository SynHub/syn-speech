using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Syn.Speech.Helper;
//REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// Provides a set of methods for performing simple math in the log domain.
    /// The logarithmic base can be set by the
    /// property: <code>LogMath.logBase</code>
    /// </summary>
    public class LogMath
    {
        public static float LogZero = -Float.MAX_VALUE;
        public static float LogOne = 0.0f;

        // Singeleton instance.
        private static LogMath _instance;
        private static float _logBase = 1.0001f;
        private static Boolean _useTable = true;

        private readonly float _naturalLogBase;
        private readonly float _inverseNaturalLogBase;

        private readonly float[] _theAddTable;
        private readonly float _maxLogValue;
        private readonly float _minLogValue;

        private readonly object _syncRoot = new Object();

        private LogMath() 
        {
            _naturalLogBase = (float) Math.Log(_logBase);
            _inverseNaturalLogBase = 1.0f / _naturalLogBase;
            // When converting a number from/to linear, we need to make
            // sure it's within certain limits to prevent it from underflowing/overflowing.
            // We compute the max value by computing the log of the max value that a float can contain.
            _maxLogValue = LinearToLog(JDouble.MAX_VALUE);
            // We compute the min value by computing the log of the min (absolute) value that a float can hold.
            //minLogValue = linearToLog(JDouble.MIN_VALUE); - old version
            _minLogValue = LinearToLog(4.9E-324);
            if (_useTable) {
                // Now create the addTable table. summation needed in the loop
                float innerSummation;
                // First decide number of elements.
                int entriesInTheAddTable;
                var veryLargeNumberOfEntries = 150000;
                var verySmallNumberOfEntries = 0;
                // To decide size of table, take into account that a base
                // of 1.0001 or 1.0003 converts probabilities, which are
                // numbers less than 1, into integers. Therefore, a good
                // approximation for the smallest number in the table,
                // therefore the value with the highest index, is an
                // index that maps into 0.5: indices higher than that, if
                // they were present, would map to less values less than
                // 0.5, therefore they would be mapped to 0 as
                // integers. Since the table implements the expression:
                //
                // log(1.0 + base^(-index)))
                //
                // then the highest index would be:
                //
                // topIndex = - log(logBase^(0.5) - 1)
                //
                // where log is the log in the appropriate base.
                //
                // Added -Math.rint(...) to round to nearest
                // integer. Added the negation to match the preceding
                // documentation
                entriesInTheAddTable = (int) -Math.Round(LinearToLog(LogToLinear(0.5f) - 1));
                // We reach this max if the log base is 1.00007. The
                // closer you get to 1, the higher the number of entries
                // in the table.
                if (entriesInTheAddTable > veryLargeNumberOfEntries) {
                    entriesInTheAddTable = veryLargeNumberOfEntries;
                }
                if (entriesInTheAddTable <= verySmallNumberOfEntries) 
                {
                    throw new ArgumentOutOfRangeException("The log base " + _logBase
                            + " yields a very small addTable. "
                            + "Either choose not to use the addTable, "
                            + "or choose a logBase closer to 1.0");
                }
                // PBL added this just to see how many entries really are
                // in the table
                _theAddTable = new float[entriesInTheAddTable];
                for (var index = 0; index < entriesInTheAddTable; ++index) 
                {
                    // This loop implements the expression:
                    //
                    // log( 1.0 + power(base, index))
                    //
                    // needed to add two numbers in the log domain.
                    innerSummation = (float) LogToLinear(-index);
                    innerSummation += 1.0f;
                    _theAddTable[index] = LinearToLog(innerSummation);
                }
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static LogMath GetLogMath() 
        {
            if (null == _instance) 
            {
                if (null == _instance)
                    _instance = new LogMath();
            }

            return _instance;
        }
        /// <summary>
        /// Sets log base.
        /// According to forum discussions a value between 1.00001 and 1.0004 should
        /// be used for speech recognition. Going above 1.0005 will probably hurt.
        /// </summary>
        /// <param name="logBase"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SetLogBase(float logBase) 
        {
            Debug.Assert(_instance == null);
            _logBase = logBase;
        }
        /// <summary>
        /// The property that controls whether we use the old, slow (but correct)
        /// method of performing the LogMath.add by doing the actual computation. 
        /// </summary>
        /// <param name="useTable"></param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static void SetUseTable(Boolean useTable) 
        {
            Debug.Assert(_instance == null);
            _useTable = useTable;
        }

        /// <summary>
        /// Returns the summation of two numbers when the arguments and the result are in log. 
        /// That is, it returns
        /// log(a + b) given log(a) and log(b) 
        /// This method makes use of the equality: 
        /// log(a + b) = log(a) + log (1 + exp(log(b) - log(a))) 
        /// which is derived from: 
        /// a + b = a/// (1 + (b / a))  which in turns makes use of: 
        /// b / a = exp (log(b) - log(a)) 
        /// Important to notice that <code>subtractAsLinear(a, b)</code> is *not* the same as
        /// <code>addAsLinear(a, -b)</code>, since we're in the log domain, and -b is in fact the inverse. No
        /// underflow/overflow check is performed. </p>
        ///
        /// @param logVal1 value in log domain (i.e. log(val1)) to add
        /// @param logVal2 value in log domain (i.e. log(val2)) to add
        /// @return sum of val1 and val2 in the log domain
        /// <summary>
        public float AddAsLinear(float logVal1, float logVal2) 
        {
            var logHighestValue = logVal1;
            var logDifference = logVal1 - logVal2;
            /*
            /// [ EBG: maybe we should also have a function to add many numbers, *
            /// say, return the summation of all terms in a given vector, if *
            /// efficiency becomes an issue.
             */
            // difference is always a positive number
            if (logDifference < 0) 
            {
                logHighestValue = logVal2;
                logDifference = -logDifference;
            }

            var toReturn = logHighestValue + AddTable(logDifference);;
            return toReturn;
        }

        /// <summary>
        /// Method used by add() internally. It returns the difference between the highest number and the total summation of
        /// two numbers. <p/> Considering the expression (in which we assume natural log) <p/> <p/> <b>log(a + b) = log(a) +
        /// log(1 + exp(log(b) - log(a))) </b> </p>
        /// <p/>
        /// the current function returns the second term of the right hand side of the equality above, generalized for the
        /// case of any log base. This function can be constructed as a table, if table lookup is faster than actual
        /// computation.
        ///
        /// @param index the index into the addTable
        /// @return the value pointed to by index
        /// <summary>
        private float AddTableActualComputation(float index) 
        {
            double logInnerSummation;
            // Negate index, since the derivation of this formula implies
            // the smallest number as a numerator, therefore the log of the
            // ratio is negative
            logInnerSummation = LogToLinear(-index);
            logInnerSummation += 1.0;
            return LinearToLog(logInnerSummation);
        }

        /// <summary>
        /// Method used by add() internally. It returns the difference between the highest number and the total summation of
        /// two numbers. <p/> Considering the expression (in which we assume natural log) <p/> <p/> <b>log(a + b) = log(a) +
        /// log(1 + exp(log(b) - log(a))) </b> </p>
        /// <p/>
        /// the current function returns the second term of the right hand side of the equality above, generalized for the
        /// case of any log base. This function is constructed as a table lookup.
        ///
        /// @param index the index into the addTable
        /// @return the value pointed to by index
        /// @throws IllegalArgumentException
        /// <summary>
        private float AddTable(float index)
        {
            if (index > Int32.MaxValue)
                return 0.0f;
            if (_useTable) {
                // int intIndex = (int) Math.rint(index);
                var intIndex =  (int) (index + 0.5);
                // When adding two numbers, the highest one should be
                // preserved, and therefore the difference should always
                // be positive.
                if (0 <= intIndex) {
                    if (intIndex < _theAddTable.Length) 
                    {
                        return _theAddTable[intIndex];
                    } 
                    else 
                    {
                        return 0.0f;
                    }
                } 
                else 
                {
                    throw new ArgumentOutOfRangeException("addTable index has to be negative");
                }
            } else 
            {
                return AddTableActualComputation(index);
            }
        }

        /// <summary>
        /// Returns the difference between two numbers when the arguments and the result are in log. <p/> <p/> That is, it
        /// returns log(a - b) given log(a) and log(b) </p> <p/> <p/> Implementation is less efficient than add(), since
        /// we're less likely to use this function, provided for completeness. Notice however that the result only makes
        /// sense if the minuend is higher than the subtrahend. Otherwise, we should return the log of a negative number.
        /// </p> <p/> <p/> It implements the subtraction as: </p> <p/> <p/> <b>log(a - b) = log(a) + log(1 - exp(log(b) -
        /// log(a))) </b> </p> <p/> <p/> No need to check for underflow/overflow. </p>
        ///
        /// @param logMinuend    value in log domain (i.e. log(minuend)) to be subtracted from
        /// @param logSubtrahend value in log domain (i.e. log(subtrahend)) that is being subtracted
        /// @return difference between minuend and the subtrahend in the log domain
        /// @throws IllegalArgumentException <p/> This is a very slow way to do this, but this method should rarely be used.
        ///                                  </p>
        /// <summary>
        public float SubtractAsLinear(float logMinuend, float logSubtrahend)
        {
            double logInnerSummation;
            if (logMinuend < logSubtrahend) {
                throw new ArgumentOutOfRangeException("Subtraction results in log "
                        + "of a negative number: " + logMinuend + " - "
                        + logSubtrahend);
            }
            logInnerSummation = 1.0;
            logInnerSummation -= LogToLinear(logSubtrahend - logMinuend);
            return logMinuend + LinearToLog(logInnerSummation);
        }

        /// <summary>
        /// Converts the source, which is assumed to be a log value whose base is sourceBase, to a log value whose base is
        /// resultBase. Possible values for both the source and result bases include Math.E, 10.0, LogMath.getLogBase(). If a
        /// source or result base is not supported, an IllegalArgumentException will be thrown. <p/> <p/> It takes advantage
        /// of the relation: </p> <p/> <p/> <b>log_a(b) = log_c(b) / lob_c(a) </b> </p> <p/> <p/> or: </p> <p/> <p/>
        /// <b>log_a(b) = log_c(b)/// lob_a(c) </b> </p> <p/> <p/> where <b>log_a(b) </b> is logarithm of <b>b </b> base <b>a
        /// </b> etc. </p>
        ///
        /// @param logSource  log value whose base is sourceBase
        /// @param sourceBase the base of the log the source
        /// @param resultBase the base to convert the source log to
        /// @throws IllegalArgumentException
        /// <summary>
        //  [[[ TODO: This is slow, but it probably doesn't need
        //  to be too fast ]]]
        // [ EBG: it can be made more efficient if one of the bases is
        // Math.E. So maybe we should consider two functions logToLn and
        // lnToLog instead of a generic function like this??
        //
        public static float LogToLog(float logSource, float sourceBase,float resultBase)
        {
            if ((sourceBase <= 0) || (resultBase <= 0)) 
            {
                throw new ArgumentOutOfRangeException("Trying to take log of "
                        + " non-positive number: " + sourceBase + " or "
                        + resultBase);
            }
            if (logSource == LogZero) {
                return LogZero;
            }
            var lnSourceBase = (float) Math.Log(sourceBase);
            var lnResultBase = (float) Math.Log(resultBase);
            return (logSource* lnSourceBase / lnResultBase);
        }
        /// <summary>
        /// Converts the source, which is a number in base Math.E, to a log value which base is the LogBase of this LogMath.
        /// </summary>
        /// <param name="logSource"></param>
        /// <returns></returns>
        public float LnToLog(float logSource) 
        {
            if (logSource == LogZero) {
                return LogZero;
            }
            return (logSource* _inverseNaturalLogBase);
        }
        /// <summary>
        /// Converts the source, which is a number in base 10, to a log value which base is the LogBase of this LogMath.
        /// </summary>
        /// <param name="logSource"></param>
        /// <returns></returns>
        public float Log10ToLog(float logSource) 
        {
            if (logSource == LogZero) 
            {
                return LogZero;
            }
            return LogToLog(logSource, 10.0f, _logBase);
        }
        /// <summary>
        /// Converts the source, whose base is the LogBase of this LogMath, to a log value which is a number in base Math.E.
        /// </summary>
        /// <param name="logSource"></param>
        /// <returns></returns>
        public float LogToLn(float logSource) 
        {
            if (logSource == LogZero) {
                return LogZero;
            }
            return logSource* _naturalLogBase;
        }

        /// <summary>
        /// Converts the value from linear scale to log scale. The log scale numbers are limited by the range of the type
        /// float. The linear scale numbers can be any double value.
        ///
        /// @param linearValue the value to be converted to log scale
        /// @return the value in log scale
        /// @throws IllegalArgumentException
        /// <summary>
        public float LinearToLog(double linearValue)
        {
            double returnValue;
            if (linearValue < 0.0) {
                throw new ArgumentOutOfRangeException(
                        "linearToLog: param must be >= 0: " + linearValue);
            } 
            else if (linearValue == 0.0) 
            {
                // [EBG] Shouldn't the comparison above be something like
                // linearValue < "epsilon"? Is it ever going to be 0.0?
                return LogZero;
            } 
            else {
                returnValue = Math.Log(linearValue)* _inverseNaturalLogBase;
                if (returnValue > Float.MAX_VALUE) 
                {
                    return Float.MAX_VALUE;
                } 
                else {
                    if (returnValue < -Float.MAX_VALUE) {
                        return -Float.MAX_VALUE;
                    } else {
                        return (float) returnValue;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the value from log scale to linear scale.
        ///
        /// @param logValue the value to be converted to the linear scale
        /// @return the value in the linear scale
        /// <summary>
        public double LogToLinear(float logValue) 
        {
            // return Math.pow(logBase, logValue);
            double returnValue;
            if (logValue < _minLogValue) {
                returnValue = 0.0;
            } else if (logValue > _maxLogValue) {
                returnValue = JDouble.MAX_VALUE;
            } else {
                returnValue = Math.Exp(LogToLn(logValue));
            }
            return returnValue;
        }

        /// <summary>
        /// Returns the actual log base. 
        /// </summary>
        /// <value></value>
        public float LogBase
        {
            get { return _logBase; }
        }

        public bool IsUseTable
        {
            get { return _useTable; }
        }

        /// </summary>
        /// Returns the log (base 10) of value
        ///
        /// @param value the value to take the log of
        /// @return the log (base 10) of value
        /// </summary>
        // [ EBG: Shouldn't we be using something like logToLog(value, base, 10)
        // for this? ]
        public static float Log10(float value) 
        {
            return (float) (0.4342944819* Math.Log(value));
            // If you want to get rid of the constant:
            // return ((1.0f / Math.log(10.0f))/// Math.log(value));
        }

        /// <summary>
        /// Converts a vector from linear domain to log domain using a given <code>LogMath</code>-instance for conversion. 
        /// </summary>
        /// <param name="vector"></param>
        public void LinearToLog(float[] vector) 
        {
            var nbGaussians = vector.Length;
            for (var i = 0; i < nbGaussians; i++) 
            {
                vector[i] = LinearToLog(vector[i]);
            }
        }

        /// <summary>
        /// Converts a vector from log to linear domain using a given <code>LogMath</code>-instance for conversion. 
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="?"></param>
        public void LogToLinear(float[] vector, float[] pout) 
        {
            for (var i = 0; i < vector.Length; i++) 
            {
                pout[i] = (float)LogToLinear(vector[i]);
            }
        }
    }
}
