using Syn.Speech.Helper;
//PATROLLED
namespace Syn.Speech.Linguist.Language.NGram.Trie
{
    public class NgramTrieQuant
    {
        public enum QuantType { NO_QUANT = 0, QUANT_16 = 1 }

        private int probBits;
        private int backoffBits;

        private int probMask;
        private int backoffMask;
        private float[][] tables;

        private QuantType quantType;

        public NgramTrieQuant(int order, QuantType quantType)
        {
            switch (quantType)
            {
                case QuantType.NO_QUANT:
                    return; //nothing to do here
                case QuantType.QUANT_16:
                    probBits = 16;
                    backoffBits = 16;
                    probMask = (1 << probBits) - 1;
                    backoffMask = (1 << backoffBits) - 1;
                    break;
                default:
                    throw new Error("Unsupported quantization type: " + quantType);
            }
            tables = new float[(order - 1) * 2 - 1][];
            this.quantType = quantType;
        }

        public void setTable(float[] table, int order, bool isProb)
        {
            int index = (order - 2) * 2;
            if (!isProb) index++;
            tables[index] = table;
        }

        public int getProbTableLen()
        {
            return 1 << probBits;
        }

        public int getBackoffTableLen()
        {
            return 1 << backoffBits;
        }

        public int getProbBoSize()
        {
            switch (quantType)
            {
                case QuantType.NO_QUANT:
                    return 63;
                case QuantType.QUANT_16:
                    return 32; //16 bits for prob + 16 bits for bo
                //TODO implement different quantization stages
                default:
                    throw new Error("Unsupported quantization type: " + quantType);
            }
        }

        public int getProbSize()
        {
            switch (quantType)
            {
                case QuantType.NO_QUANT:
                    return 31;
                case QuantType.QUANT_16:
                    return 16; //16 bits for probs
                //TODO implement different quantization stages
                default:
                    throw new Error("Unsupported quantization type: " + quantType);
            }
        }

        private float binsDecode(int tableIdx, int encodedVal)
        {
            return tables[tableIdx][encodedVal];
        }

        public float readProb(NgramTrieBitarr bitArr, int memPtr, int bitOffset, int orderMinusTwo)
        {
            switch (quantType)
            {
                case QuantType.NO_QUANT:
                    return bitArr.readNegativeFloat(memPtr, bitOffset);
                case QuantType.QUANT_16:
                    int tableIdx = orderMinusTwo * 2;
                    if (tableIdx < tables.Length - 1)
                        bitOffset += backoffBits;
                    return binsDecode(tableIdx, bitArr.readInt(memPtr, bitOffset, backoffMask));
                //TODO implement different quantization stages
                default:
                    throw new Error("Unsupported quantization type: " + quantType);
            }
        }

        public float readBackoff(NgramTrieBitarr bitArr, int memPtr, int bitOffset, int orderMinusTwo)
        {
            switch (quantType)
            {
                case QuantType.NO_QUANT:
                    bitOffset += 31;
                    return bitArr.readFloat(memPtr, bitOffset);
                case QuantType.QUANT_16:
                    int tableIdx = orderMinusTwo * 2 + 1;
                    return binsDecode(tableIdx, bitArr.readInt(memPtr, bitOffset, probMask));
                //TODO implement different quantization stages
                default:
                    throw new Error("Unsupported quantization type: " + quantType);
            }
        }
    }
}
