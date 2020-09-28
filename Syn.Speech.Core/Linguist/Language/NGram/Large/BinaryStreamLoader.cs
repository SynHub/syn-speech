using System;
using System.IO;
//REFACTORED
namespace Syn.Speech.Linguist.Language.NGram.Large
{
    /// <summary>
    /// Language model that reads whole model into memory. Useful
    /// for loading language models from resources or external locations.
    /// </summary>
    public class BinaryStreamLoader: BinaryLoader
    {
        byte[] _modelData;
    
        public BinaryStreamLoader(String location, string format, Boolean
                applyLanguageWeightAndWip,
                float languageWeight, double wip,
                float unigramWeight)
            :base(format, applyLanguageWeightAndWip, languageWeight, wip,
                    unigramWeight)
           
        {        
            var stream = new FileStream(location,FileMode.Open);
            LoadModelLayout(stream);
        
            LoadModelData(stream);
        }

    
        /**
        /// Reads whole data into memory
        /// 
        /// @param stream  the stream to load model from
        /// @throws IOException 
         */
        private void LoadModelData(Stream stream)
        {
            var dataStream = new BufferedStream(stream, 8192);
            var bytes = new MemoryStream();
            var buffer = new byte[4096];
            while (true)
            {
                if (dataStream.Read(buffer, 0, 4096) == 0) break;
                bytes.Write(buffer,0,4096);
            }
            _modelData = bytes.ToArray();
        }

        public override sbyte[] LoadBuffer(long position, int size)
        {
            sbyte[] result = new sbyte[size];
            Array.Copy(_modelData, (int)position, result, 0, size);
            return result;
        }

    }
}
