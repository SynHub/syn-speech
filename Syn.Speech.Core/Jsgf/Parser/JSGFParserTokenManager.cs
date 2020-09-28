using System;
using System.IO;
using System.Text;
using Syn.Speech.Helper;
//PATROLLED + REFACTORED - Class makes use of heavy casting and octal to hex conversions
namespace Syn.Speech.Jsgf.Parser
{
    /// <summary>
    /// Token Manager. 
    /// </summary>
    public class JSGFParserTokenManager : JSGFParserConstants
    {
        /// <summary>
        /// Debug output.
        /// </summary>
        public TextWriter DebugStream = Console.Out;
        /// <summary>
        /// Set debug output.
        /// </summary>
        /// <param name="ds">The ds.</param>
        public void SetDebugStream(TextWriter ds) { DebugStream = ds; }
        private int jjStopStringLiteralDfa_0(int pos, long active0)
        {
            switch (pos)
            {
                case 0:
                    if ((active0 & 0x400000140L) != 0L)
                        return 2;
                    if ((active0 & 0x800e000L) != 0L)
                    {
                        jjmatchedKind = 23;
                        return 37;
                    }
                    if ((active0 & 0x20000000L) != 0L)
                        return 5;
                    return -1;
                case 1:
                    if ((active0 & 0x100L) != 0L)
                        return 0;
                    if ((active0 & 0x800e000L) != 0L)
                    {
                        jjmatchedKind = 23;
                        jjmatchedPos = 1;
                        return 37;
                    }
                    return -1;
                case 2:
                    if ((active0 & 0x8000000L) != 0L)
                    {
                        if (jjmatchedPos < 1)
                        {
                            jjmatchedKind = 23;
                            jjmatchedPos = 1;
                        }
                        return -1;
                    }
                    if ((active0 & 0xe000L) != 0L)
                    {
                        jjmatchedKind = 23;
                        jjmatchedPos = 2;
                        return 37;
                    }
                    return -1;
                case 3:
                    if ((active0 & 0x8000000L) != 0L)
                    {
                        if (jjmatchedPos < 1)
                        {
                            jjmatchedKind = 23;
                            jjmatchedPos = 1;
                        }
                        return -1;
                    }
                    if ((active0 & 0xe000L) != 0L)
                    {
                        jjmatchedKind = 23;
                        jjmatchedPos = 3;
                        return 37;
                    }
                    return -1;
                case 4:
                    if ((active0 & 0xe000L) != 0L)
                    {
                        jjmatchedKind = 23;
                        jjmatchedPos = 4;
                        return 37;
                    }
                    return -1;
                case 5:
                    if ((active0 & 0xc000L) != 0L)
                        return 37;
                    if ((active0 & 0x2000L) != 0L)
                    {
                        jjmatchedKind = 23;
                        jjmatchedPos = 5;
                        return 37;
                    }
                    return -1;
                default:
                    return -1;
            }
        }
        private int jjStartNfa_0(int pos, long active0)
        {
            return jjMoveNfa_0(jjStopStringLiteralDfa_0(pos, active0), pos + 1);
        }
        private int JJStopAtPos(int pos, int kind)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            return pos + 1;
        }
        private int jjMoveStringLiteralDfa0_0()
        {
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 40:
                    return JJStopAtPos(0, 36);
                case 41:
                    return JJStopAtPos(0, 37);
                case 42:
                    return JJStopAtPos(0, 30);
                case 43:
                    return JJStopAtPos(0, 35);
                case 46:
                    return jjStartNfaWithStates_0(0, 29, 5);
                case 47:
                    jjmatchedKind = 34;
                    return jjMoveStringLiteralDfa1_0(0x140L);
                case 59:
                    return JJStopAtPos(0, 26);
                case 60:
                    return JJStopAtPos(0, 28);
                case 61:
                    return JJStopAtPos(0, 32);
                case 62:
                    return JJStopAtPos(0, 31);
                case 86:
                    return jjMoveStringLiteralDfa1_0(0x8000000L);
                case 91:
                    return JJStopAtPos(0, 38);
                case 93:
                    return JJStopAtPos(0, 39);
                case 103:
                    return jjMoveStringLiteralDfa1_0(0x2000L);
                case 105:
                    return jjMoveStringLiteralDfa1_0(0x4000L);
                case 112:
                    return jjMoveStringLiteralDfa1_0(0x8000L);
                case 124:
                    return JJStopAtPos(0, 33);
                default:
                    return jjMoveNfa_0(3, 0);
            }
        }
        private int jjMoveStringLiteralDfa1_0(long active0)
        {
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                jjStopStringLiteralDfa_0(0, active0);
                return 1;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 42:
                    if ((active0 & 0x100L) != 0L)
                        return jjStartNfaWithStates_0(1, 8, 0);
                    break;
                case 47:
                    if ((active0 & 0x40L) != 0L)
                        return JJStopAtPos(1, 6);
                    break;
                case 49:
                    return jjMoveStringLiteralDfa2_0(active0, 0x8000000L);
                case 109:
                    return jjMoveStringLiteralDfa2_0(active0, 0x4000L);
                case 114:
                    return jjMoveStringLiteralDfa2_0(active0, 0x2000L);
                case 117:
                    return jjMoveStringLiteralDfa2_0(active0, 0x8000L);
            }
            return jjStartNfa_0(0, active0);
        }
        private int jjMoveStringLiteralDfa2_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(0, old0);
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                jjStopStringLiteralDfa_0(1, active0);
                return 2;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 46:
                    return jjMoveStringLiteralDfa3_0(active0, 0x8000000L);
                case 97:
                    return jjMoveStringLiteralDfa3_0(active0, 0x2000L);
                case 98:
                    return jjMoveStringLiteralDfa3_0(active0, 0x8000L);
                case 112:
                    return jjMoveStringLiteralDfa3_0(active0, 0x4000L);
            }
            return jjStartNfa_0(1, active0);
        }
        private int jjMoveStringLiteralDfa3_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(1, old0);
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                jjStopStringLiteralDfa_0(2, active0);
                return 3;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 48:
                    if ((active0 & 0x8000000L) != 0L)
                        return JJStopAtPos(3, 27);
                    break;
                case 108:
                    return jjMoveStringLiteralDfa4_0(active0, 0x8000L);
                case 109:
                    return jjMoveStringLiteralDfa4_0(active0, 0x2000L);
                case 111:
                    return jjMoveStringLiteralDfa4_0(active0, 0x4000L);
            }
            return jjStartNfa_0(2, active0);
        }
        private int jjMoveStringLiteralDfa4_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(2, old0);
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                jjStopStringLiteralDfa_0(3, active0);
                return 4;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 105:
                    return jjMoveStringLiteralDfa5_0(active0, 0x8000L);
                case 109:
                    return jjMoveStringLiteralDfa5_0(active0, 0x2000L);
                case 114:
                    return jjMoveStringLiteralDfa5_0(active0, 0x4000L);
            }
            return jjStartNfa_0(3, active0);
        }
        private int jjMoveStringLiteralDfa5_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(3, old0);
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                jjStopStringLiteralDfa_0(4, active0);
                return 5;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 97:
                    return jjMoveStringLiteralDfa6_0(active0, 0x2000L);
                case 99:
                    if ((active0 & 0x8000L) != 0L)
                        return jjStartNfaWithStates_0(5, 15, 37);
                    break;
                case 116:
                    if ((active0 & 0x4000L) != 0L)
                        return jjStartNfaWithStates_0(5, 14, 37);
                    break;
            }
            return jjStartNfa_0(4, active0);
        }
        private int jjMoveStringLiteralDfa6_0(long old0, long active0)
        {
            if (((active0 &= old0)) == 0L)
                return jjStartNfa_0(4, old0);
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                jjStopStringLiteralDfa_0(5, active0);
                return 6;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 114:
                    if ((active0 & 0x2000L) != 0L)
                        return jjStartNfaWithStates_0(6, 13, 37);
                    break;
            }
            return jjStartNfa_0(5, active0);
        }
        private int jjStartNfaWithStates_0(int pos, int kind, int state)
        {
            jjmatchedKind = kind;
            jjmatchedPos = pos;
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException) { return pos + 1; }
            return jjMoveNfa_0(state, pos + 1);
        }
        readonly ulong[] _jjbitVec0 = {
   0xfffffffffffffffeL, 0xffffffffffffffffL, 0xffffffffffffffffL, 0xffffffffffffffffL
};
        readonly ulong[] _jjbitVec2 = {
   0x0L, 0x0L, 0xffffffffffffffffL, 0xffffffffffffffffL
};
        readonly ulong[] _jjbitVec3 = {
   0x1ff00000fffffffeL, 0xffffffffffffc000L, 0xffffffffL, 0x600000000000000L
};
        readonly ulong[] _jjbitVec4 = {
   0x0L, 0x0L, 0x0L, 0xff7fffffff7fffffL
};
        readonly ulong[] _jjbitVec5 = {
   0x0L, 0xffffffffffffffffL, 0xffffffffffffffffL, 0xffffffffffffffffL
};
        readonly ulong[] _jjbitVec6 = {
   0xffffffffffffffffL, 0xffffffffffffffffL, 0xffffL, 0x0L
};
        readonly ulong[] _jjbitVec7 = {
   0xffffffffffffffffL, 0xffffffffffffffffL, 0x0L, 0x0L
};
        readonly long[] _jjbitVec8 = {
   0x3fffffffffffL, 0x0L, 0x0L, 0x0L
};
        private int jjMoveNfa_0(int startState, int curPos)
        {
            int startsAt = 0;
            _jjnewStateCnt = 54;
            int i = 1;
            _jjstateSet[0] = startState;
            int kind = 0x7fffffff;
            for (; ; )
            {
                if (++_jjround == 0x7fffffff)
                    ReInitRounds();
                if (CurChar < 64)
                {
                    long l = 1L << CurChar;
                    do
                    {
                        switch (_jjstateSet[--i])
                        {
                            case 3:
                                if ((0x7ff30fa00000000L & l) != 0L)
                                {
                                    if (kind > 23)
                                        kind = 23;
                                    JJCheckNAdd(37);
                                }
                                else if (CurChar == 34)
                                    JJCheckNAddStates(0, 2);
                                else if (CurChar == 46)
                                    JJCheckNAdd(5);
                                else if (CurChar == 47)
                                    _jjstateSet[_jjnewStateCnt++] = 2;
                                if ((0x3ff000000000000L & l) != 0L)
                                {
                                    if (kind > 16)
                                        kind = 16;
                                    JJCheckNAddStates(3, 10);
                                }
                                else if (CurChar == 39)
                                    JJAddStates(11, 12);
                                break;
                            case 0:
                                if (CurChar == 42)
                                    _jjstateSet[_jjnewStateCnt++] = 1;
                                break;
                            case 1:
                                if ((0xffff7fffffffffffL & (ulong)l) != 0L && kind > 7)
                                    kind = 7;
                                break;
                            case 2:
                                if (CurChar == 42)
                                    _jjstateSet[_jjnewStateCnt++] = 0;
                                break;
                            case 4:
                                if (CurChar == 46)
                                    JJCheckNAdd(5);
                                break;
                            case 5:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                JJCheckNAddStates(13, 15);
                                break;
                            case 7:
                                if ((0x280000000000L & l) != 0L)
                                    JJCheckNAdd(8);
                                break;
                            case 8:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                JJCheckNAddTwoStates(8, 9);
                                break;
                            case 10:
                                if (CurChar == 39)
                                    JJAddStates(11, 12);
                                break;
                            case 11:
                                if ((0xffffff7fffffdbffL & (ulong)l) != 0L)
                                    JJCheckNAdd(12);
                                break;
                            case 12:
                                if (CurChar == 39 && kind > 20)
                                    kind = 20;
                                break;
                            case 14:
                                if ((0x8400000000L & l) != 0L)
                                    JJCheckNAdd(12);
                                break;
                            case 15:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAddTwoStates(16, 12);
                                break;
                            case 16:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAdd(12);
                                break;
                            case 17:
                                if ((0xf000000000000L & l) != 0L)
                                    _jjstateSet[_jjnewStateCnt++] = 18;
                                break;
                            case 18:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAdd(16);
                                break;
                            case 19:
                                if (CurChar == 34)
                                    JJCheckNAddStates(0, 2);
                                break;
                            case 20:
                                if ((0xfffffffbffffdbffL & (ulong)l) != 0L)
                                    JJCheckNAddStates(0, 2);
                                break;
                            case 22:
                                if ((0x8400000000L & l) != 0L)
                                    JJCheckNAddStates(0, 2);
                                break;
                            case 23:
                                if (CurChar == 34 && kind > 21)
                                    kind = 21;
                                break;
                            case 24:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAddStates(16, 19);
                                break;
                            case 25:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAddStates(0, 2);
                                break;
                            case 26:
                                if ((0xf000000000000L & l) != 0L)
                                    _jjstateSet[_jjnewStateCnt++] = 27;
                                break;
                            case 27:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAdd(25);
                                break;
                            case 29:
                                JJCheckNAddStates(20, 22);
                                break;
                            case 31:
                                if ((0x8400000000L & l) != 0L)
                                    JJCheckNAddStates(20, 22);
                                break;
                            case 33:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAddStates(23, 26);
                                break;
                            case 34:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAddStates(20, 22);
                                break;
                            case 35:
                                if ((0xf000000000000L & l) != 0L)
                                    _jjstateSet[_jjnewStateCnt++] = 36;
                                break;
                            case 36:
                                if ((0xff000000000000L & l) != 0L)
                                    JJCheckNAdd(34);
                                break;
                            case 37:
                                if ((0x7ff30fa00000000L & l) == 0L)
                                    break;
                                if (kind > 23)
                                    kind = 23;
                                JJCheckNAdd(37);
                                break;
                            case 38:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 16)
                                    kind = 16;
                                JJCheckNAddStates(3, 10);
                                break;
                            case 39:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 16)
                                    kind = 16;
                                JJCheckNAdd(39);
                                break;
                            case 40:
                                if ((0x3ff000000000000L & l) != 0L)
                                    JJCheckNAddTwoStates(40, 41);
                                break;
                            case 41:
                                if (CurChar != 46)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                JJCheckNAddStates(27, 29);
                                break;
                            case 42:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                JJCheckNAddStates(27, 29);
                                break;
                            case 44:
                                if ((0x280000000000L & l) != 0L)
                                    JJCheckNAdd(45);
                                break;
                            case 45:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                JJCheckNAddTwoStates(45, 9);
                                break;
                            case 46:
                                if ((0x3ff000000000000L & l) != 0L)
                                    JJCheckNAddTwoStates(46, 47);
                                break;
                            case 48:
                                if ((0x280000000000L & l) != 0L)
                                    JJCheckNAdd(49);
                                break;
                            case 49:
                                if ((0x3ff000000000000L & l) == 0L)
                                    break;
                                if (kind > 18)
                                    kind = 18;
                                JJCheckNAddTwoStates(49, 9);
                                break;
                            case 50:
                                if ((0x3ff000000000000L & l) != 0L)
                                    JJCheckNAddStates(30, 32);
                                break;
                            case 52:
                                if ((0x280000000000L & l) != 0L)
                                    JJCheckNAdd(53);
                                break;
                            case 53:
                                if ((0x3ff000000000000L & l) != 0L)
                                    JJCheckNAddTwoStates(53, 9);
                                break;
                        }
                    } while (i != startsAt);
                }
                else if (CurChar < 128)
                {
                    long l = 1L << (CurChar & 0x3F);
                    do
                    {
                        switch (_jjstateSet[--i])
                        {
                            case 3:
                                if ((0x47fffffed7ffffffL & l) != 0L)
                                {
                                    if (kind > 23)
                                        kind = 23;
                                    JJCheckNAdd(37);
                                }
                                else if (CurChar == 123)
                                    JJCheckNAddStates(20, 22);
                                break;
                            case 1:
                                if (kind > 7)
                                    kind = 7;
                                break;
                            case 6:
                                if ((0x2000000020L & l) != 0L)
                                    JJAddStates(33, 34);
                                break;
                            case 9:
                                if ((0x5000000050L & l) != 0L && kind > 18)
                                    kind = 18;
                                break;
                            case 11:
                                if ((0xffffffffefffffffL & (ulong)l) != 0L)
                                    JJCheckNAdd(12);
                                break;
                            case 13:
                                if (CurChar == 92)
                                    JJAddStates(35, 37);
                                break;
                            case 14:
                                if ((0x14404410000000L & l) != 0L)
                                    JJCheckNAdd(12);
                                break;
                            case 20:
                                if ((0xffffffffefffffffL & (ulong)l) != 0L)
                                    JJCheckNAddStates(0, 2);
                                break;
                            case 21:
                                if (CurChar == 92)
                                    JJAddStates(38, 40);
                                break;
                            case 22:
                                if ((0x14404410000000L & l) != 0L)
                                    JJCheckNAddStates(0, 2);
                                break;
                            case 28:
                                if (CurChar == 123)
                                    JJCheckNAddStates(20, 22);
                                break;
                            case 29:
                                if ((0xdfffffffffffffffL & (ulong)l) != 0L)
                                    JJCheckNAddStates(20, 22);
                                break;
                            case 30:
                                if (CurChar == 92)
                                    JJAddStates(41, 43);
                                break;
                            case 31:
                                if ((0x2014404410000000L & l) != 0L)
                                    JJCheckNAddStates(20, 22);
                                break;
                            case 32:
                                if (CurChar == 125 && kind > 22)
                                    kind = 22;
                                break;
                            case 37:
                                if ((0x47fffffed7ffffffL & l) == 0L)
                                    break;
                                if (kind > 23)
                                    kind = 23;
                                JJCheckNAdd(37);
                                break;
                            case 43:
                                if ((0x2000000020L & l) != 0L)
                                    JJAddStates(44, 45);
                                break;
                            case 47:
                                if ((0x2000000020L & l) != 0L)
                                    JJAddStates(46, 47);
                                break;
                            case 51:
                                if ((0x2000000020L & l) != 0L)
                                    JJAddStates(48, 49);
                                break;
                        }
                    } while (i != startsAt);
                }
                else
                {
                    int hiByte = CurChar >> 8;
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 0x3F);
                    int i2 = (CurChar & 0xff) >> 6;
                    long l2 = 1L << (CurChar & 0x3F);
                    do
                    {
                        switch (_jjstateSet[--i])
                        {
                            case 3:
                            case 37:
                                if (!jjCanMove_1(hiByte, i1, i2, l1, l2))
                                    break;
                                if (kind > 23)
                                    kind = 23;
                                JJCheckNAdd(37);
                                break;
                            case 1:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2) && kind > 7)
                                    kind = 7;
                                break;
                            case 11:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    _jjstateSet[_jjnewStateCnt++] = 12;
                                break;
                            case 20:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    JJAddStates(0, 2);
                                break;
                            case 29:
                                if (jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    JJAddStates(20, 22);
                                break;
                        }
                    } while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = _jjnewStateCnt) == (startsAt = 54 - (_jjnewStateCnt = startsAt)))
                    return curPos;
                try { CurChar = InputStream.ReadChar(); }
                catch (IOException) { return curPos; }
            }
        }
        private int jjMoveStringLiteralDfa0_3()
        {
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 42:
                    return jjMoveStringLiteralDfa1_3(0x800L);
                default:
                    return 1;
            }
        }
        private int jjMoveStringLiteralDfa1_3(long active0)
        {
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                return 1;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 47:
                    if ((active0 & 0x800L) != 0L)
                        return JJStopAtPos(1, 11);
                    break;
                default:
                    return 2;
            }
            return 2;
        }
        private int jjMoveStringLiteralDfa0_1()
        {
            return jjMoveNfa_1(4, 0);
        }
        private int jjMoveNfa_1(int startState, int curPos)
        {
            int startsAt = 0;
            _jjnewStateCnt = 4;
            int i = 1;
            _jjstateSet[0] = startState;
            int kind = 0x7fffffff;
            for (; ; )
            {
                if (++_jjround == 0x7fffffff)
                    ReInitRounds();
                if (CurChar < 64)
                {
                    long l = 1L << CurChar;
                    do
                    {
                        switch (_jjstateSet[--i])
                        {
                            case 4:
                                if ((0xffffffffffffdbffL & (ulong)l) != 0L)
                                {
                                    if (kind > 9)
                                        kind = 9;
                                    JJCheckNAddStates(50, 52);
                                }
                                else if ((0x2400L & l) != 0L)
                                {
                                    if (kind > 9)
                                        kind = 9;
                                }
                                if (CurChar == 13)
                                    _jjstateSet[_jjnewStateCnt++] = 2;
                                break;
                            case 0:
                                if ((0xffffffffffffdbffL & (ulong)l) == 0L)
                                    break;
                                kind = 9;
                                JJCheckNAddStates(50, 52);
                                break;
                            case 1:
                                if ((0x2400L & l) != 0L && kind > 9)
                                    kind = 9;
                                break;
                            case 2:
                                if (CurChar == 10 && kind > 9)
                                    kind = 9;
                                break;
                            case 3:
                                if (CurChar == 13)
                                    _jjstateSet[_jjnewStateCnt++] = 2;
                                break;
                        }
                    } while (i != startsAt);
                }
                else if (CurChar < 128)
                {
                    long l = 1L << (CurChar & 0x3F);
                    do
                    {
                        switch (_jjstateSet[--i])
                        {
                            case 4:
                            case 0:
                                kind = 9;
                                JJCheckNAddStates(50, 52);
                                break;
                            default: break;
                        }
                    } while (i != startsAt);
                }
                else
                {
                    int hiByte = CurChar >> 8;
                    int i1 = hiByte >> 6;
                    long l1 = 1L << (hiByte & 0x3F);
                    int i2 = (CurChar & 0xff) >> 6;
                    long l2 = 1L << (CurChar & 0x3F);
                    do
                    {
                        switch (_jjstateSet[--i])
                        {
                            case 4:
                            case 0:
                                if (!jjCanMove_0(hiByte, i1, i2, l1, l2))
                                    break;
                                if (kind > 9)
                                    kind = 9;
                                JJCheckNAddStates(50, 52);
                                break;
                        }
                    } while (i != startsAt);
                }
                if (kind != 0x7fffffff)
                {
                    jjmatchedKind = kind;
                    jjmatchedPos = curPos;
                    kind = 0x7fffffff;
                }
                ++curPos;
                if ((i = _jjnewStateCnt) == (startsAt = 4 - (_jjnewStateCnt = startsAt)))
                    return curPos;
                try { CurChar = InputStream.ReadChar(); }
                catch (IOException e) { return curPos; }
            }
        }
        private int jjMoveStringLiteralDfa0_2()
        {
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 42:
                    return jjMoveStringLiteralDfa1_2(0x400L);
                default:
                    return 1;
            }
        }
        private int jjMoveStringLiteralDfa1_2(long active0)
        {
            try { CurChar = InputStream.ReadChar(); }
            catch (IOException)
            {
                return 1;
            }
            var intCurChar = Java.ToInt(CurChar);
            switch (intCurChar)
            {
                case 47:
                    if ((active0 & 0x400L) != 0L)
                        return JJStopAtPos(1, 10);
                    break;
                default:
                    return 2;
            }
            return 2;
        }
        readonly int[] _jjnextStates = {
   20, 21, 23, 39, 40, 41, 46, 47, 50, 51, 9, 11, 13, 5, 6, 9, 
   20, 21, 25, 23, 29, 30, 32, 29, 30, 34, 32, 42, 43, 9, 50, 51, 
   9, 7, 8, 14, 15, 17, 22, 24, 26, 31, 33, 35, 44, 45, 48, 49, 
   52, 53, 0, 1, 3, 
};
        private bool jjCanMove_0(int hiByte, int i1, int i2, long l1, long l2)
        {
            switch (hiByte)
            {
                case 0:
                    return ((_jjbitVec2[i2] & (ulong)l2) != 0L);
                default:
                    if ((_jjbitVec0[i1] & (ulong)l1) != 0L)
                        return true;
                    return false;
            }
        }
        private bool jjCanMove_1(int hiByte, int i1, int i2, long l1, long l2)
        {
            switch (hiByte)
            {
                case 0:
                    return ((_jjbitVec4[i2] & (ulong)l2) != 0L);
                case 48:
                    return ((_jjbitVec5[i2] & (ulong)l2) != 0L);
                case 49:
                    return ((_jjbitVec6[i2] & (ulong)l2) != 0L);
                case 51:
                    return ((_jjbitVec7[i2] & (ulong)l2) != 0L);
                case 61:
                    return ((_jjbitVec8[i2] & l2) != 0L);
                default:
                    if ((_jjbitVec3[i1] & (ulong)l1) != 0L)
                        return true;
                    return false;
            }
        }

        /// <summary>
        /// Token literal values.
        /// </summary>
        public static readonly string[] jjstrLiteralImages = {
"", null, null, null, null, null, null, null, null, null, null, null, null, 
"\x67\x72\x61\x6D\x6D\x61\x72", "\x69\x6D\x70\x6F\x72\x74", "\x70\x75\x62\x6C\x69\x63", null, null, null, null, 
null, null, null, null, null, null, "\x3B", "\x56\x31\x2E\x30", "\x3C", "\x2E", "\x2A", 
"\x3E", "\x3D", "\x7C", "\x2F", "\x2B", "\x28", "\x29", "\x5B", "\x5D"
        };

        /** Lexer state names. */
        public readonly String[] LexStateNames = {
   "DEFAULT",
   "IN_SINGLE_LINE_COMMENT",
   "IN_FORMAL_COMMENT",
   "IN_MULTI_LINE_COMMENT",
};

        /** Lex State array. */
        public readonly int[] JjnewLexState = {
   -1, -1, -1, -1, -1, -1, 1, 2, 3, 0, 0, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
   -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 
};
        readonly long[] _jjtoToken = {
   0xfffcf5e001L, 
};
        readonly long[] _jjtoSkip = {
   0xe3eL, 
};
        readonly long[] _jjtoSpecial = {
   0xe00L, 
};
        readonly long[] _jjtoMore = {
   0x11c0L, 
};
        protected JavaCharStream InputStream;
        private readonly uint[] _jjrounds = new uint[54];
        private readonly int[] _jjstateSet = new int[108];
        private static readonly StringBuilder jjimage = new StringBuilder();
        private StringBuilder image = jjimage;
        private int _jjimageLen;
        private int _lengthOfMatch;
        protected char CurChar;
        /** Constructor. */
        public JSGFParserTokenManager(JavaCharStream stream)
        {
            if (JavaCharStream.StaticFlag)
                throw new Error("ERROR: Cannot use a static CharStream class with a non-static lexical analyzer.");
            InputStream = stream;
        }

        /** Constructor. */
        public JSGFParserTokenManager(JavaCharStream stream, int lexState)
            : this(stream)
        {

            SwitchTo(lexState);
        }

        /** Reinitialise parser. */
        public void ReInit(JavaCharStream stream)
        {
            jjmatchedPos = _jjnewStateCnt = 0;
            _curLexState = DefaultLexState;
            InputStream = stream;
            ReInitRounds();
        }
        private void ReInitRounds()
        {
            int i;
            _jjround = 0x80000001;
            for (i = 54; i-- > 0; )
                _jjrounds[i] = 0x80000000;
        }

        /** Reinitialise parser. */
        public void ReInit(JavaCharStream stream, int lexState)
        {
            ReInit(stream);
            SwitchTo(lexState);
        }

        /** Switch to specified lex state. */
        public void SwitchTo(int lexState)
        {
            if (lexState >= 4 || lexState < 0)
                throw new TokenMgrError("Error: Ignoring invalid lexical state : " + lexState + ". State unchanged.", TokenMgrError.InvalidLexicalState);
            else
                _curLexState = lexState;
        }

        protected Token jjFillToken()
        {
            Token t;
            String curTokenImage;
            int beginLine;
            int endLine;
            int beginColumn;
            int endColumn;
            if (jjmatchedPos < 0)
            {
                if (image == null)
                    curTokenImage = "";
                else
                    curTokenImage = image.ToString();
                beginLine = endLine = InputStream.GetBeginLine();
                beginColumn = endColumn = InputStream.GetBeginColumn();
            }
            else
            {
                String im = jjstrLiteralImages[jjmatchedKind];
                curTokenImage = (im == null) ? InputStream.GetImage() : im;
                beginLine = InputStream.GetBeginLine();
                beginColumn = InputStream.GetBeginColumn();
                endLine = InputStream.GetEndLine();
                endColumn = InputStream.GetEndColumn();
            }
            t = Token.NewToken(jjmatchedKind, curTokenImage);

            t.BeginLine = beginLine;
            t.EndLine = endLine;
            t.BeginColumn = beginColumn;
            t.EndColumn = endColumn;

            return t;
        }

        int _curLexState;
        private const int DefaultLexState = 0;
        int _jjnewStateCnt;
        uint _jjround;
        int jjmatchedPos;
        int jjmatchedKind;

        /// <summary>
        /// Gets the next token.
        /// </summary>
        /// <returns></returns>
        public Token GetNextToken()
        {
            Token specialToken = null;
            Token matchedToken;
            int curPos = 0;

        EOFLoop: for (; ; )
            {
                try
                {
                    CurChar = InputStream.BeginToken();
                }
                catch (IOException e)
                {
                    jjmatchedKind = 0;
                    matchedToken = jjFillToken();
                    matchedToken.SpecialToken = specialToken;
                    return matchedToken;
                }
                image = jjimage;
                image.Length = 0;
                _jjimageLen = 0;

                for (; ; )
                {
                    switch (_curLexState)
                    {
                        case 0:
                            try
                            {
                                InputStream.Backup(0);
                                while (CurChar <= 32 && (0x100003600L & (1L << CurChar)) != 0L)
                                    CurChar = InputStream.BeginToken();
                            }
                            catch (IOException e1) { goto EOFLoop; } //TODO: Check "goto" behaviour
                            jjmatchedKind = 0x7fffffff;
                            jjmatchedPos = 0;
                            curPos = jjMoveStringLiteralDfa0_0();
                            break;
                        case 1:
                            jjmatchedKind = 9;
                            jjmatchedPos = -1;
                            curPos = 0;
                            curPos = jjMoveStringLiteralDfa0_1();
                            if (jjmatchedPos < 0 || (jjmatchedPos == 0 && jjmatchedKind > 12))
                            {
                                jjmatchedKind = 12;
                                jjmatchedPos = 0;
                            }
                            break;
                        case 2:
                            jjmatchedKind = 0x7fffffff;
                            jjmatchedPos = 0;
                            curPos = jjMoveStringLiteralDfa0_2();
                            if (jjmatchedPos == 0 && jjmatchedKind > 12)
                            {
                                jjmatchedKind = 12;
                            }
                            break;
                        case 3:
                            jjmatchedKind = 0x7fffffff;
                            jjmatchedPos = 0;
                            curPos = jjMoveStringLiteralDfa0_3();
                            if (jjmatchedPos == 0 && jjmatchedKind > 12)
                            {
                                jjmatchedKind = 12;
                            }
                            break;
                    }
                    if (jjmatchedKind != 0x7fffffff)
                    {
                        if (jjmatchedPos + 1 < curPos)
                            InputStream.Backup(curPos - jjmatchedPos - 1);
                        if ((_jjtoToken[jjmatchedKind >> 6] & (1L << (jjmatchedKind & 0x3F))) != 0L)
                        {
                            matchedToken = jjFillToken();
                            matchedToken.SpecialToken = specialToken;
                            if (JjnewLexState[jjmatchedKind] != -1)
                                _curLexState = JjnewLexState[jjmatchedKind];
                            return matchedToken;
                        }
                        else if ((_jjtoSkip[jjmatchedKind >> 6] & (1L << (jjmatchedKind & 0x3F))) != 0L)
                        {
                            if ((_jjtoSpecial[jjmatchedKind >> 6] & (1L << (jjmatchedKind & 0x3F))) != 0L)
                            {
                                matchedToken = jjFillToken();
                                if (specialToken == null)
                                    specialToken = matchedToken;
                                else
                                {
                                    matchedToken.SpecialToken = specialToken;
                                    specialToken = (specialToken.Next = matchedToken);
                                }
                                SkipLexicalActions(matchedToken);
                            }
                            else
                                SkipLexicalActions(null);
                            if (JjnewLexState[jjmatchedKind] != -1)
                                _curLexState = JjnewLexState[jjmatchedKind];
                            goto EOFLoop; //TODO: Check "goto" behaviour
                        }
                        MoreLexicalActions();
                        if (JjnewLexState[jjmatchedKind] != -1)
                            _curLexState = JjnewLexState[jjmatchedKind];
                        curPos = 0;
                        jjmatchedKind = 0x7fffffff;
                        try
                        {
                            CurChar = InputStream.ReadChar();
                            continue;
                        }
                        catch (IOException e1) { }
                    }
                    int errorLine = InputStream.GetEndLine();
                    int errorColumn = InputStream.GetEndColumn();
                    String errorAfter = null;
                    bool eofSeen = false;
                    try { InputStream.ReadChar(); InputStream.Backup(1); }
                    catch (IOException e1)
                    {
                        eofSeen = true;
                        errorAfter = curPos <= 1 ? "" : InputStream.GetImage();
                        if (CurChar == '\n' || CurChar == '\r')
                        {
                            errorLine++;
                            errorColumn = 0;
                        }
                        else
                            errorColumn++;
                    }
                    if (!eofSeen)
                    {
                        InputStream.Backup(1);
                        errorAfter = curPos <= 1 ? "" : InputStream.GetImage();
                    }
                    throw new TokenMgrError(eofSeen, _curLexState, errorLine, errorColumn, errorAfter, CurChar, TokenMgrError.LEXICAL_ERROR);
                }
            }
        }

        void SkipLexicalActions(Token matchedToken)
        {
            switch (jjmatchedKind)
            {
                default:
                    break;
            }
        }
        void MoreLexicalActions()
        {
            _jjimageLen += (_lengthOfMatch = jjmatchedPos + 1);
            switch (jjmatchedKind)
            {
                case 7:
                    image.Append(InputStream.GetSuffix(_jjimageLen));
                    _jjimageLen = 0;
                    InputStream.Backup(1);
                    break;
                default:
                    break;
            }
        }
        private void JJCheckNAdd(int state)
        {
            if (_jjrounds[state] != _jjround)
            {
                _jjstateSet[_jjnewStateCnt++] = state;
                _jjrounds[state] = _jjround;
            }
        }
        private void JJAddStates(int start, int end)
        {
            do
            {
                _jjstateSet[_jjnewStateCnt++] = _jjnextStates[start];
            } while (start++ != end);
        }
        private void JJCheckNAddTwoStates(int state1, int state2)
        {
            JJCheckNAdd(state1);
            JJCheckNAdd(state2);
        }
        private void JJCheckNAddStates(int start, int end)
        {
            do
            {
                JJCheckNAdd(_jjnextStates[start]);
            } while (start++ != end);
        }

    }
}
