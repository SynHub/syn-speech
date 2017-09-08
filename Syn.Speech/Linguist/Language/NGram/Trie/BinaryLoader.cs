using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
using Syn.Speech.Util;

namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    public class BinaryLoader
    {
        private static string TRIE_HEADER = "Trie Language Model";
        private FileStream inStream;

        public BinaryLoader(FileInfo location)
        {
            inStream = new FileStream(location.FullName, FileMode.Open);
        }

        private void loadModelData(Stream stream)
        {
            //var dataStream = new BufferedStream(stream);
            //ByteArrayOutputStream bytes = new ByteArrayOutputStream();
            //byte[] buffer = new byte[4096];
            //while (true)
            //{
            //    if (dataStream.read(buffer) < 0)
            //        break;
            //    bytes.write(buffer);
            //}

            inStream = stream as FileStream; //TODO check effect
            //inStream = ;
        }

        public BinaryLoader(URL location)
        {
            loadModelData(location.OpenStream());
        }

        public void verifyHeader()
        {
            string readHeader = readString(inStream, TRIE_HEADER.Length);
            if (!readHeader.Equals(TRIE_HEADER))
            {
                throw new Error("Bad binary LM file header: " + readHeader);
            }
        }

        public int[] readCounts()
        {
            int order = readOrder();
            int[] counts = new int[order];
            for (int i = 0; i < counts.Length; i++)
            {
                counts[i] = Utilities.ReadLittleEndianInt(inStream);
            }
            return counts;
        }

        public NgramTrieQuant readQuant(int order)
        {
            int quantTypeInt = Utilities.ReadLittleEndianInt(inStream);
            if (quantTypeInt < 0 || quantTypeInt >= 2/* QuantType has 2 Enums with int value 0 and 1*/)
            {
                throw new Error("Unknown quantatization type: " + quantTypeInt);
            }
                
            NgramTrieQuant.QuantType quantType = (NgramTrieQuant.QuantType)quantTypeInt;
            NgramTrieQuant quant = new NgramTrieQuant(order, quantType);
            //reading tables
            for (int i = 2; i <= order; i++)
            {
                quant.setTable(readFloatArr(quant.getProbTableLen()), i, true);
                if (i < order)
                    quant.setTable(readFloatArr(quant.getBackoffTableLen()), i, false);
            }
            return quant;
        }

        public TrieUnigram[] readUnigrams(int count)
        {
            TrieUnigram[] unigrams = new TrieUnigram[count + 1];
            for (int i = 0; i < count + 1; i++)
            {
                unigrams[i] = new TrieUnigram();
                unigrams[i].prob = Utilities.ReadLittleEndianFloat(inStream);
                unigrams[i].backoff = Utilities.ReadLittleEndianFloat(inStream);
                unigrams[i].next = Utilities.ReadLittleEndianInt(inStream);
            }
            return unigrams;
        }

        public void readTrieByteArr(byte[] arr)
        {
            inStream.Read(arr);
        }

        public String[] readWords(int unigramNum)
        {
            int len = Utilities.ReadLittleEndianInt(inStream);
            if (len <= 0)
            {
                throw new Error("Bad word string size: " + len);
            }
            String[] words = new String[unigramNum];
            byte[] bytes = new byte[len];
            inStream.Read(bytes);
   
            int s = 0;
            int wordStart = 0;
            for (int i = 0; i < len; i++)
            {
                char c = (char)(bytes[i] & 0xFF);
                if (c == '\0')
                {
                    // if its the end of a string, add it to the 'words' array
                    words[s] = Encoding.UTF8.GetString(bytes, wordStart, i - wordStart); //TODO: Check effect -> new String(bytes, wordStart, i - wordStart);
                    wordStart = i + 1;
                    s++;
                }
            }
            Trace.Assert(s == unigramNum);
            return words;
        }


        public void close()
        {
            inStream.Close();
        }

        private int readOrder()
        {
            return (int)inStream.ReadByte();
        }

        private float[] readFloatArr(int len)
        {
            float[] arr = new float[len];
            for (int i = 0; i < len; i++)
                arr[i] = Utilities.ReadLittleEndianFloat(inStream);
            return arr;
        }

        private String readString(FileStream stream, int length)
        {
            StringBuilder builder = new StringBuilder();
            byte[] bytes = new byte[length];
            stream.Read(bytes);//TODO: Check Extension
            for (int i = 0; i < length; i++)
            {
                builder.Append((char)bytes[i]);
            }
            return builder.ToString();
        }
    }
}
