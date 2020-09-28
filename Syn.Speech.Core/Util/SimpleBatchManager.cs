using System;
using System.Collections.Generic;
//PATROLLED + REFACTORED
namespace Syn.Speech.Util
{
    /// <summary>
    /// A simple implementation of the batch manager suitable for single threaded batch processing 
    /// </summary>
    public class SimpleBatchManager : IBatchManager
    {
        private readonly int _skip;
        private int _whichBatch;
        private readonly int _totalBatches;
        private int _curItem;
        private List<String> _items;

        /**
        /// Constructs a SimpleBatchManager object.
         *
        /// @param filename     the name of the batch file
        /// @param skip         number of records to skip between items
        /// @param whichBatch   which chunk of the batch should we process
        /// @param totalBatches the total number of chuncks that the batch is divided into.
         */
        public SimpleBatchManager(String filename, int skip, int whichBatch, int totalBatches) 
        {
            Filename = filename;
            _skip = skip;
            _whichBatch = whichBatch;
            _totalBatches = totalBatches;
        }

        /** Starts processing the batch */
        public void Start()
        {
            _curItem = 0;
            _items = GetBatchItems(Filename);
        }

        /**
        /// Gets the next available batch item or null if no more are available
         *
        /// @return the next available batch item
        /// @throws IOException if an I/O error occurs while getting the next item from the batch file.
         */
        public BatchItem GetNextItem()
        {
            if (_curItem >= _items.Count) 
            {
                return null;
            }
            var line = _items[_curItem++];
            return new BatchItem(BatchFile.GetFilename(line),
                BatchFile.GetReference(line));
        }

        /** Stops processing the batch */
        public void Stop()
        {
        }

        /**
        /// Returns the name of the file
         *
        /// @return the filename
         */

        public string Filename { get; private set; }

        /**
        /// Gets the set of lines from the file
         *
        /// @param file the name of the file
         */
        private List<String> GetBatchItems(String file)
        {
            var list = BatchFile.GetLines(file, _skip);

            if (_totalBatches > 1) 
            {
                var linesPerBatch = list.Count / _totalBatches;
                if (linesPerBatch < 1) {
                    linesPerBatch = 1;
                }
                if (_whichBatch >= _totalBatches) {
                    _whichBatch = _totalBatches - 1;
                }
                var startLine = _whichBatch* linesPerBatch;
                // last batch needs to get all remaining lines
                if (_whichBatch == (_totalBatches - 1)) 
                {
                    list = list.GetRange(startLine, list.Count);
                } 
                else 
                {
                    list = list.GetRange(startLine, startLine + linesPerBatch);
                }
            }
            return list;
        }
    }
}
