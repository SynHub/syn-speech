using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Results;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{

    /// <summary>
    /// Implements a portion of the NIST align/scoring algorithm to compare a reference string to a hypothesis string.  
    /// It only keeps track of substitutions, insertions, and deletions.
    /// </summary>
    public class NISTAlign
    {

        /* Constants that help with the align.  The following are
         * used in the backtrace table and backtrace list.
         */
        const int Ok = 0;
        const int Substitution = 1;
        const int Insertion = 2;
        const int Deletion = 3;

        //Constants that help with the align.  The following are used to create the penalty table.
        private const int MaxPenalty = 1000000;
        private const int SubstitutionPenalty = 100;
        private const int InsertionPenalty = 75;
        private const int DeletionPenalty = 75;


        /// <summary>
        /// Used for padding out aligned strings.
        /// </summary>
        private const string Stars = "********************************************";
        private const string Spaces = "                                            ";
        private const string HRule = "============================================================================";

        //Totals over the life of this class.  These can be reset to 0 with a call to resetTotals.
        private int _totalSentences;
        private int _totalSentencesWithErrors;
        private int _totalSentencesWithSubtitutions;
        private int _totalSentencesWithInsertions;
        private int _totalSentencesWithDeletions;
        private int _totalReferenceWords;
        private int _totalHypothesisWords;
        private int _totalAlignedWords;
        private int _totalWordsCorrect;
        private int _totalSubstitutions;
        private int _totalInsertions;
        private int _totalDeletions;

        //Error values for one call to 'align'
        private int _substitutions;
        private int _insertions;
        private int _deletions;
        private int _correct;

        //The raw reference string.  Updated with each call to 'align'.
        private string _rawReference;

        /**
         * The reference annotation; typically the name of the audio file for the reference string.  This is an optional
         * part of the rawReference string.  If it is included, it is appended to the end of the string in parentheses.
         * Updated with each call to 'align'.
         */
        private string _referenceAnnotation;

        /**
         * Ordered list of words from rawReference after the annotation has been removed.  Updated with each call to
         * 'align'.
         */
        private LinkedList<Object> _referenceItems;

        /** Aligned list of words from rawReference.  Created in alignWords.  Updated with each call to 'align'. */
        private LinkedList<string> _alignedReferenceWords;

        /** The raw hypothesis string.  Updated with each call to 'align'. */
        private string _rawHypothesis;

        /// <summary>
        /// Ordered list of words from rawHypothesis after the annotation has been removed.  Updated with each call to'align'.
        /// </summary>
        private LinkedList<Object> _hypothesisItems;

        /// <summary>
        /// Aligned list of words from rawHypothesis.  Created in alignWords.  Updated with each call to 'align'.
        /// </summary>
        private LinkedList<string> _alignedHypothesisWords;

        /** Helpers to create percentage strings. */
        static readonly string percentageFormat = "##0.0%";


        private bool _showResults;
        private bool _showAlignedResults;


        /** Creates a new NISTAlign object. */
        public NISTAlign(bool showResults, bool showAlignedResults)
        {
            _showResults = showResults;
            _showAlignedResults = showAlignedResults;
            ResetTotals();
        }


        /**
         * Sets whether results are displayed
         *
         * @param showResults true if the results should be displayed
         */
        public void SetShowResults(bool showResults)
        {
            _showResults = showResults;
        }


        /**
         * Sets whether aligned results are displayed
         *
         * @param showAlignedResults true if the aligned results should be displayed
         */
        public void SetShowAlignedResults(bool showAlignedResults)
        {
            _showAlignedResults = showAlignedResults;
        }


        /** Reset the total insertions, deletions, and substitutions counts for this class. */
        public void ResetTotals()
        {
            _totalSentences = 0;
            _totalSentencesWithErrors = 0;
            _totalSentencesWithSubtitutions = 0;
            _totalSentencesWithInsertions = 0;
            _totalSentencesWithDeletions = 0;
            _totalReferenceWords = 0;
            _totalHypothesisWords = 0;
            _totalAlignedWords = 0;
            _totalWordsCorrect = 0;
            _totalSubstitutions = 0;
            _totalInsertions = 0;
            _totalDeletions = 0;
        }


        /**
         * Performs the NIST alignment on the reference and hypothesis strings.  This has the side effect of updating nearly
         * all the fields of this class.
         *
         * @param reference  the reference string
         * @param hypothesis the hypothesis string
         * @return true if the reference and hypothesis match
         */
        public bool Align(string reference, string hypothesis)
        {
            int annotationIndex;

            // Save the original strings for future reference.
            //
            _rawReference = reference;
            _rawHypothesis = hypothesis;

            // Strip the annotation off the reference string and
            // save it.
            //
            annotationIndex = _rawReference.IndexOf('(');
            if (annotationIndex != -1)
            {
                _referenceAnnotation = _rawReference.Substring(annotationIndex);
                _referenceItems = ToList(_rawReference.Substring(0, annotationIndex));
            }
            else
            {
                _referenceAnnotation = null;
                _referenceItems = ToList(_rawReference);
            }

            // Strip the annotation off the hypothesis string.
            // If one wanted to be anal retentive, they might compare
            // the hypothesis annotation to the reference annotation,
            // but I'm not quite that obsessive.
            //
            annotationIndex = _rawHypothesis.IndexOf('(');
            if (annotationIndex != -1)
            {
                _hypothesisItems = ToList(
                        _rawHypothesis.Substring(0, annotationIndex));
            }
            else
            {
                _hypothesisItems = ToList(_rawHypothesis);
            }

            // Reset the counts for this sentence.
            //
            _substitutions = 0;
            _insertions = 0;
            _deletions = 0;

            // Turn the list of reference and hypothesis words into two
            // aligned lists of strings.  This has the side effect of
            // creating alignedReferenceWords and alignedHypothesisWords.
            //
            AlignWords(Backtrace(CreateBacktraceTable(_referenceItems, _hypothesisItems, new CustomComparator1())), new CustomStringRenderer1());
            // Compute the number of correct words in the hypothesis.
            //
            _correct = _alignedReferenceWords.Count
                    - (_insertions + _deletions + _substitutions);

            // Update the totals that are kept over the lifetime of this
            // class.
            //
            UpdateTotals();

            return (_insertions + _deletions + _substitutions) == 0;
        }


        /**
         * Returns the reference string.  This string will be filtered (all spurious whitespace removed and annotation
         * removed) and set to all lower case.
         *
         * @return the reference string
         */
        public string GetReference()
        {
            return Tostring(_referenceItems);
        }


        /**
         * Returns the hypothesis string.  This string will be filtered (all spurious whitespace removed and annotation
         * removed) and set to all lower case.
         *
         * @return the hypothesis string
         */
        public string GetHypothesis()
        {
            return Tostring(_hypothesisItems);
        }


        /**
         * Returns the aligned reference string.
         *
         * @return the aligned reference string
         */
        public string GetAlignedReference()
        {
            return Tostring(_alignedReferenceWords);
        }


        /**
         * Returns the aligned hypothesis string.
         *
         * @return the aligned hypothesis string
         */
        public string GetAlignedHypothesis()
        {
            return Tostring(_alignedHypothesisWords);
        }


        /**
         * Gets the total number of word errors for all calls to align.
         *
         * @return the total number of word errors for all calls to align
         */
        public int GetTotalWordErrors()
        {
            return _totalSubstitutions + _totalInsertions + _totalDeletions;
        }


        /**
         * Returns the total word accuracy.
         *
         * @return the accuracy between 0.0 and 1.0
         */
        public float GetTotalWordAccuracy()
        {
            if (_totalReferenceWords == 0)
            {
                return 0;
            }
            else
            {
                return _totalWordsCorrect / ((float)_totalReferenceWords);
            }
        }


        /**
         * Returns the total word accuracy.
         *
         * @return the accuracy between 0.0 and 1.0
         */
        public float GetTotalWordErrorRate()
        {
            if (_totalReferenceWords == 0)
            {
                return 0;
            }
            else
            {
                return GetTotalWordErrors()
                        / ((float)_totalReferenceWords);
            }
        }


        /**
         * Returns the total sentence accuracy.
         *
         * @return the accuracy between 0.0 and 1.0
         */
        public float GetTotalSentenceAccuracy()
        {
            var totalSentencesCorrect = _totalSentences - _totalSentencesWithErrors;
            if (_totalSentences == 0)
            {
                return 0;
            }
            else
            {
                return (totalSentencesCorrect / (float)_totalSentences);
            }
        }


        /**
         * Gets the total number of words
         *
         * @return the total number of words
         */
        public int GetTotalWords()
        {
            return _totalReferenceWords;
        }


        /**
         * Gets the total number of substitution errors
         *
         * @return the total number of substitutions
         */
        public int GetTotalSubstitutions()
        {
            return _totalSubstitutions;
        }


        /**
         * Gets the total number of insertion errors
         *
         * @return the total number of insertion errors
         */
        public int GetTotalInsertions()
        {
            return _totalInsertions;
        }


        /**
         * Gets the total number of deletions
         *
         * @return the total number of deletions
         */
        public int GetTotalDeletions()
        {
            return _totalDeletions;
        }


        /**
         * Gets the total number of sentences
         *
         * @return the total number of sentences
         */
        public int GetTotalSentences()
        {
            return _totalSentences;
        }


        /**
         * Gets the total number of sentences with errors
         *
         * @return the total number of sentences with errors
         */
        public int GetTotalSentencesWithErrors()
        {
            return _totalSentencesWithDeletions;
        }


        /**
         * Prints the results for this sentence to System.out.  If you want the output to match the NIST output, see
         * printNISTSentenceSummary.
         *
         * @see #printNISTSentenceSummary
         */
        public void PrintSentenceSummary()
        {
            if (_showResults)
            {
                Console.WriteLine("REF:       " + Tostring(_referenceItems));
                Console.WriteLine("HYP:       " + Tostring(_hypothesisItems));
            }

            if (_showAlignedResults)
            {
                Console.WriteLine("ALIGN_REF: " + Tostring(_alignedReferenceWords));
                Console.WriteLine("ALIGN_HYP: " + Tostring(_alignedHypothesisWords));
            }
        }


        /**
         * Prints the total summary for all calls.  If you want the output to match the NIST output, see
         * printNISTTotalSummary.
         *
         * @see #printNISTTotalSummary
         */
        public void PrintTotalSummary()
        {
            if (_totalSentences > 0)
            {
                Console.Write(
                        "   Accuracy: " + ToPercentage("##0.000%",
                                GetTotalWordAccuracy()));
                Console.WriteLine(
                        "    Errors: " + GetTotalWordErrors()
                                + "  (Sub: " + _totalSubstitutions
                                + "  Ins: " + _totalInsertions
                                + "  Del: " + _totalDeletions + ')');
                Console.WriteLine(
                        "   Words: " + _totalReferenceWords
                                + "   Matches: " + _totalWordsCorrect
                                + "    WER: " + ToPercentage("##0.000%",
                                GetTotalWordErrorRate()));
                Console.WriteLine(
                        "   Sentences: " + _totalSentences
                                + "   Matches: " + (_totalSentences - _totalSentencesWithErrors)
                                + "   SentenceAcc: " + ToPercentage("##0.000%",
                                GetTotalSentenceAccuracy()));
            }
        }


        /** Prints the results for this sentence to System.out.  This matches the output from the NIST aligner. */
        public void PrintNistSentenceSummary()
        {
            var sentenceErrors = _substitutions + _insertions + _deletions;

            Console.WriteLine();

            Console.Write("REF: " + Tostring(_alignedReferenceWords));
            if (_referenceAnnotation != null)
            {
                Console.Write(' ' + _referenceAnnotation);
            }
            Console.WriteLine();

            Console.Write("HYP: " + Tostring(_alignedHypothesisWords));
            if (_referenceAnnotation != null)
            {
                Console.Write(' ' + _referenceAnnotation);
            }
            Console.WriteLine();

            Console.WriteLine();

            if (_referenceAnnotation != null)
            {
                Console.WriteLine("SENTENCE " + _totalSentences
                        + "  " + _referenceAnnotation);
            }
            else
            {
                Console.WriteLine("SENTENCE " + _totalSentences);
            }

            Console.WriteLine("Correct          = "
                    + ToPercentage("##0.0%",
                    _correct,
                    _referenceItems.Count)
                    + PadLeft(5, _correct)
                    + "   ("
                    + PadLeft(6, _totalWordsCorrect)
                    + ')');
            Console.WriteLine("Errors           = "
                    + ToPercentage("##0.0%",
                    sentenceErrors,
                    _referenceItems.Count)
                    + PadLeft(5, sentenceErrors)
                    + "   ("
                    + PadLeft(6, _totalSentencesWithErrors)
                    + ')');

            Console.WriteLine();
            Console.WriteLine(HRule);
        }


        /** Prints the summary for all calls to align to System.out.  This matches the output from the NIST aligner. */
        public void PrintNistTotalSummary()
        {
            var totalSentencesCorrect = _totalSentences - _totalSentencesWithErrors;

            Console.WriteLine();
            Console.WriteLine("---------- SUMMARY ----------");
            Console.WriteLine();
            Console.WriteLine("SENTENCE RECOGNITION PERFORMANCE:");
            Console.WriteLine("sentences                          " + _totalSentences);
            Console.WriteLine("  correct                  " + ToPercentage("##0.0%", totalSentencesCorrect, _totalSentences) + " (" + PadLeft(4, totalSentencesCorrect) + ')');
            Console.WriteLine("  with error(s)            "
                            + ToPercentage("##0.0%", _totalSentencesWithErrors, _totalSentences)
                            + " (" + PadLeft(4, _totalSentencesWithErrors) + ')');
            Console.WriteLine("    with substitutions(s)  "
                            + ToPercentage("##0.0%", _totalSentencesWithSubtitutions, _totalSentences)
                            + " (" + PadLeft(4, _totalSentencesWithSubtitutions) + ')');
            Console.WriteLine("    with insertion(s)      "
                            + ToPercentage("##0.0%", _totalSentencesWithInsertions, _totalSentences)
                            + " (" + PadLeft(4, _totalSentencesWithInsertions) + ')');
            Console.WriteLine("    with deletions(s)      "
                            + ToPercentage("##0.0%", _totalSentencesWithDeletions, _totalSentences)
                            + " (" + PadLeft(4, _totalSentencesWithDeletions) + ')');

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("WORD RECOGNITION PERFORMANCE:");
            Console.WriteLine("Correct           = "
                            + ToPercentage("##0.0%", _totalWordsCorrect, _totalReferenceWords)
                            + " (" + PadLeft(6, _totalWordsCorrect) + ')');
            Console.WriteLine("Substitutions     = "
                            + ToPercentage("##0.0%", _totalSubstitutions, _totalReferenceWords)
                            + " (" + PadLeft(6, _totalSubstitutions) + ')');
            Console.WriteLine("Deletions         = "
                            + ToPercentage("##0.0%", _totalDeletions, _totalReferenceWords)
                            + " (" + PadLeft(6, _totalDeletions) + ')');
            Console.WriteLine("Insertions        = "
                            + ToPercentage("##0.0%", _totalInsertions, _totalReferenceWords)
                            + " (" + PadLeft(6, _totalInsertions) + ')');
            Console.WriteLine("Errors            = "
                            + ToPercentage("##0.0%", GetTotalWordErrors(), _totalReferenceWords)
                            + " (" + PadLeft(6, GetTotalWordErrors()) + ')');

            Console.WriteLine();

            Console.WriteLine("Ref. words           = " + PadLeft(6, _totalReferenceWords));
            Console.WriteLine("Hyp. words           = " + PadLeft(6, _totalHypothesisWords));
            Console.WriteLine("Aligned words        = " + PadLeft(6, _totalAlignedWords));

            Console.WriteLine();
            Console.WriteLine(
                    "WORD ACCURACY=  "
                            + ToPercentage("##0.000%", _totalWordsCorrect, _totalReferenceWords)
                            + " ("
                            + PadLeft(5, _totalWordsCorrect)
                            + '/'
                            + PadLeft(5, _totalReferenceWords)
                            + ")  ERRORS= "
                            + ToPercentage("##0.000%",
                            GetTotalWordErrors(),
                            _totalReferenceWords)
                            + " ("
                            + PadLeft(5, GetTotalWordErrors())
                            + '/'
                            + PadLeft(5, _totalReferenceWords)
                            + ')');

            Console.WriteLine();
        }


        /**
         * Creates the backtrace table.  This is magic.  The basic idea is that the penalty table contains a set of penalty
         * values based on some strategically selected numbers.  I'm not quite sure what they are, but they help determine
         * the backtrace table values.  The backtrace table contains information used to help determine if words matched
         * (OK), were inserted (INSERTION), substituted (SUBSTITUTION), or deleted (DELETION).
         *
         * @param referenceItems  the ordered list of reference words
         * @param hypothesisItems the ordered list of hypothesis words
         * @return the backtrace table
         */
        int[,] CreateBacktraceTable<T>(LinkedList<T> referenceItems, LinkedList<T> hypothesisItems, IComparator comparator)
        {
            int[,] penaltyTable;
            int[,] backtraceTable;
            int penalty;
            int minPenalty;

            penaltyTable = new int[referenceItems.Count + 1, hypothesisItems.Count + 1];

            backtraceTable = new int[referenceItems.Count + 1, hypothesisItems.Count + 1];

            // Initialize the penaltyTable and the backtraceTable.  The
            // rows of each table represent the words in the reference
            // string.  The columns of each table represent the words in
            // the hypothesis string.
            //
            penaltyTable[0, 0] = 0;
            backtraceTable[0, 0] = Ok;

            // The lower left of the tables represent deletions.  If you
            // think about this, a shorter hypothesis string will have
            // deleted words from the reference string.
            //
            for (var i = 1; i <= referenceItems.Count; i++)
            {
                penaltyTable[i, 0] = DeletionPenalty * i;
                backtraceTable[i, 0] = Deletion;
            }

            // The upper right of the tables represent insertions.  If
            // you think about this, a longer hypothesis string will have
            // inserted words.
            //
            for (var j = 1; j <= hypothesisItems.Count; j++)
            {
                penaltyTable[0, j] = InsertionPenalty * j;
                backtraceTable[0, j] = Insertion;
            }

            // Row-by-row, column-by-column, fill out the tables.
            // The goal is to keep the penalty for each cell to a
            // minimum.
            //
            for (var i = 1; i <= referenceItems.Count; i++)
            {
                for (var j = 1; j <= hypothesisItems.Count; j++)
                {
                    minPenalty = MaxPenalty;

                    // First assume that this represents a deletion.
                    //
                    penalty = penaltyTable[i - 1, j] + DeletionPenalty;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        penaltyTable[i, j] = penalty;
                        backtraceTable[i, j] = Deletion;
                    }

                    // If the words match, we'll assume it's OK.
                    // Otherwise, we assume we have a substitution.
                    //
                    if (comparator.IsSimilar(referenceItems.ElementAt(i - 1), (hypothesisItems.ElementAt(j - 1))))
                    {
                        penalty = penaltyTable[i - 1, j - 1];
                        if (penalty < minPenalty)
                        {
                            minPenalty = penalty;
                            penaltyTable[i, j] = penalty;
                            backtraceTable[i, j] = Ok;
                        }
                    }
                    else
                    {
                        penalty = penaltyTable[i - 1, j - 1] + SubstitutionPenalty;
                        if (penalty < minPenalty)
                        {
                            minPenalty = penalty;
                            penaltyTable[i, j] = penalty;
                            backtraceTable[i, j] = Substitution;
                        }
                    }

                    // If you've made it this far, it should be obvious I
                    // have no idea what the heck this code is doing.  I'm
                    // just doing a transliteration.
                    //
                    penalty = penaltyTable[i, j - 1] + InsertionPenalty;
                    if (penalty < minPenalty)
                    {
                        minPenalty = penalty;
                        penaltyTable[i, j] = penalty;
                        backtraceTable[i, j] = Insertion;
                    }
                }
            }
            return backtraceTable;
        }


        /**
         * Backtraces through the penalty table.  This starts at the "lower right" corner (i.e., the last word of the longer
         * of the reference vs. hypothesis strings) and works its way backwards.
         *
         * @param backtraceTable created from call to createBacktraceTable
         * @return a linked list of Integers representing the backtrace
         */
        LinkedList<Integer> Backtrace(int[,] backtraceTable)
        {
            var list = new LinkedList<Integer>();
            var i = _referenceItems.Count;
            var j = _hypothesisItems.Count;
            while ((i >= 0) && (j >= 0))
            {
                Java.Add(list, backtraceTable[i, j]);
                switch (backtraceTable[i, j])
                {
                    case Ok:
                        i--;
                        j--;
                        break;
                    case Substitution:
                        i--;
                        j--;
                        _substitutions++;
                        break;
                    case Insertion:
                        j--;
                        _insertions++;
                        break;
                    case Deletion:
                        i--;
                        _deletions++;
                        break;
                }
            }
            return list;
        }


        /**
         * Based on the backtrace information, words are aligned as appropriate with insertions and deletions causing
         * asterisks to be placed in the word lists.  This generates the alignedReferenceWords and alignedHypothesisWords
         * lists.
         *
         * @param backtrace the backtrace list created in backtrace
         */
        void AlignWords(LinkedList<Integer> backtrace, IStringRenderer renderer)
        {
            var referenceWordsIterator = _referenceItems.GetEnumerator();
            var hypothesisWordsIterator = _hypothesisItems.GetEnumerator();
            string referenceWord;
            string hypothesisWord;
            Object a = null;
            Object b = null;

            _alignedReferenceWords = new LinkedList<string>();
            _alignedHypothesisWords = new LinkedList<string>();


            for (var m = backtrace.Count - 2; m >= 0; m--)
            {
                int backtraceEntry = backtrace.ElementAt(m);

                if (backtraceEntry != Insertion)
                {
                    referenceWordsIterator.MoveNext();
                    a = referenceWordsIterator.Current;
                    referenceWord = renderer.GetRef(a, b);
                }
                else
                {
                    referenceWord = null;
                }
                if (backtraceEntry != Deletion)
                {
                    hypothesisWordsIterator.MoveNext();
                    b = hypothesisWordsIterator.Current;
                    hypothesisWord = renderer.GetHyp(a, b);
                }
                else
                {
                    hypothesisWord = null;
                }
                switch (backtraceEntry)
                {
                    case Substitution:
                        {
                            referenceWord = referenceWord.ToUpper();
                            hypothesisWord = hypothesisWord.ToUpper();
                            break;
                        }
                    case Insertion:
                        {
                            hypothesisWord = hypothesisWord.ToUpper();
                            break;
                        }
                    case Deletion:
                        {
                            referenceWord = referenceWord.ToUpper();
                            break;
                        }
                    case Ok:
                        break;
                }

                // Expand the missing words out to be all *'s.
                //
                if (referenceWord == null)
                {
                    referenceWord = Stars.Substring(0, hypothesisWord.Length);
                }
                if (hypothesisWord == null)
                {
                    hypothesisWord = Stars.Substring(0, referenceWord.Length);
                }

                // Fill the words up with spaces so they are the same
                // Length.
                //
                if (referenceWord.Length > hypothesisWord.Length)
                {
                    hypothesisWord = hypothesisWord + (Spaces.Substring(0, referenceWord.Length - hypothesisWord.Length));
                }
                else if (referenceWord.Length < hypothesisWord.Length)
                {
                    referenceWord = referenceWord + (Spaces.Substring(0, hypothesisWord.Length - referenceWord.Length));
                }

                Java.Add(_alignedReferenceWords, referenceWord);
                Java.Add(_alignedHypothesisWords, hypothesisWord);

            }
        }


        /** Updates the total counts based on the current alignment. */
        void UpdateTotals()
        {
            _totalSentences++;
            if ((_substitutions + _insertions + _deletions) != 0)
            {
                _totalSentencesWithErrors++;
            }
            if (_substitutions != 0)
            {
                _totalSentencesWithSubtitutions++;
            }
            if (_insertions != 0)
            {
                _totalSentencesWithInsertions++;
            }
            if (_deletions != 0)
            {
                _totalSentencesWithDeletions++;
            }
            _totalReferenceWords += _referenceItems.Count;
            _totalHypothesisWords += _hypothesisItems.Count;
            _totalAlignedWords += _alignedReferenceWords.Count;

            _totalWordsCorrect += _correct;
            _totalSubstitutions += _substitutions;
            _totalInsertions += _insertions;
            _totalDeletions += _deletions;
        }


        /**
         * Turns the numerator/denominator into a percentage.
         *
         * @param pattern     percentage pattern (ala DecimalFormat)
         * @param numerator   the numerator
         * @param denominator the denominator
         * @return a string that represents the percentage value.
         */
        string ToPercentage(string pattern, int numerator, int denominator)
        {

            var toReturn = numerator / (double)denominator;
            return PadLeft(6, toReturn.ToString(pattern));
        }


        /**
         * Turns the float into a percentage.
         *
         * @param pattern percentage pattern (ala DecimalFormat)
         * @param value   the floating point value
         * @return a string that represents the percentage value.
         */
        string ToPercentage(string pattern, float value)
        {

            return value.ToString(pattern);
        }


        /**
         * Turns the integer into a left-padded string.
         *
         * @param width the total width of string, including spaces
         * @param i     the integer
         * @return a string padded left with spaces
         */
        string PadLeft(int width, int i)
        {
            return PadLeft(width, Integer.ToString(i));
        }


        /**
         * Pads a string to the left with spaces (i.e., prepends spaces to the string so it fills out the given width).
         *
         * @param width  the total width of string, including spaces
         * @param string the string to pad
         * @return a string padded left with spaces
         */
        string PadLeft(int width, string value)
        {
            var len = value.Length;
            if (len < width)
            {
                return Spaces.Substring(0, width - len) + (value);
            }
            else
            {
                return value;
            }
        }


        /**
         * Converts the given string or words to a LinkedList.
         *
         * @param s the string of words to parse to a LinkedList
         * @return a list, one word per item
         */
        LinkedList<Object> ToList(string s)
        {
            var list = new LinkedList<Object>();
            var st = new StringTokenizer(s.Trim());
            while (st.hasMoreTokens())
            {
                var token = st.nextToken();
                list.Add(token);
            }
            return list;
        }


        /**
         * convert the list of words back to a space separated string
         *
         * @param list the list of words
         * @return a space separated string
         */
        private string Tostring<T>(LinkedList<T> list)
        {
            if (list == null || list.Count == 0)
                return "";
            var sb = new StringBuilder();
            var iterator = list.GetEnumerator();
            while (iterator.MoveNext())
                sb.Append(iterator.Current).Append(' ');
            sb.Length = (sb.Length - 1);
            return sb.ToString();
        }


        /**
         * Take two filenames -- the first contains a list of reference sentences, the second contains a list of hypothesis
         * sentences. Aligns each pair of sentences and outputs the individual and total results.
         */
        public static void Main(string[] args)
        {
            var align = new NISTAlign(true, true);

            string reference;
            string hypothesis;

            try
            {
                var referenceFile = new StreamReader(args[0]);
                var hypothesisFile = new StreamReader(args[1]);
                try
                {
                    while (true)
                    {
                        reference = referenceFile.ReadLine();
                        hypothesis = hypothesisFile.ReadLine();
                        if ((reference == null) || (hypothesis == null))
                        {
                            break;
                        }
                        else
                        {
                            align.Align(reference, hypothesis);
                            align.PrintNistSentenceSummary();
                        }
                    }
                }
                catch (IOException e)
                {
                }
                align.PrintNistTotalSummary();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine();
                Console.WriteLine("Usage: align <reference file> <hypothesis file>");
                Console.WriteLine();
            }
        }

        interface IComparator
        {
            bool IsSimilar(Object obj, Object hyp);
        }


        public interface IStringRenderer
        {
            string GetRef(Object obj, Object hyp);
            string GetHyp(Object @ref, Object hyp);
        }


        public class CustomComparator1 : IComparator
        {
            public bool IsSimilar(Object obj, Object hyp)
            {
                if (obj is string && hyp is string)
                {
                    return obj.Equals(hyp);
                }
                return false;
            }
        }

        public class CustomComparator2 : IComparator
        {
            public bool IsSimilar(object refObject, object hypObject)
            {
                if (refObject is String && hypObject is ConfusionSet)
                {
                    var word = (String)refObject;
                    var set = (ConfusionSet)hypObject;
                    if (set.ContainsWord(word))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public class CustomStringRenderer1 : IStringRenderer
        {

            public string GetRef(Object obj, Object hyp)
            {
                return (string)obj;
            }
            public string GetHyp(Object @ref, Object hyp)
            {
                return (string)hyp;
            }
        }

        public class CustomStringRenderer2 : IStringRenderer
        {
            public string GetRef(object obj, object hyp)
            {
                return (string)obj;
            }

            public string GetHyp(object refObject, object hypObject)
            {
                var word = (String)refObject;
                var set = (ConfusionSet)hypObject;
                if (set.ContainsWord(word))
                    return word;
                var res = set.GetBestHypothesis().ToString();
                return res;
            }
        }


    }

}
