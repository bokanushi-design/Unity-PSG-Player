using System.Collections.Generic;
using UnityEngine;
using uPSG;
using System;

public class MMLDecoder : MonoBehaviour
{
    private List<SeqEvent> seqList;
    private readonly int[] noteOffsetTable = { 9, 11, 0, 2, 4, 5, 7 }; // a,b,c,d,e,f,g
    private const int PARAM_OMIT = -2;

    public List<SeqEvent> Decode(string _mmlString, int _tickPerNote)
    {
        seqList = new List<SeqEvent>();
        string mmlString = _mmlString;
        int tickPerNote = _tickPerNote;
        int baseTics = tickPerNote;
        int mmlOctave = ConstValue.DEFAULT_OCTAVE;
        
        List<int> envParamsList = new();
        bool isCommentLine = false;
        bool isCommentBlock = false;

        seqList.Clear();
        int mmlCount = 0;
        while (mmlCount < mmlString.Length)
        {
            char chr = mmlString[mmlCount];
            if (chr == ';' || chr == '/')
            {
                /* comment out */
                if (chr == ';')
                {
                    isCommentLine = true;
                }
                else
                {
                    char subChr = mmlString[mmlCount + 1];
                    if (subChr == '/') { isCommentLine = true; mmlCount++; }
                    if (subChr == '*') { isCommentBlock = true; mmlCount++; }
                }
                mmlCount++;
                continue;
            }
            if (isCommentBlock)
            {
                if (chr == '*')
                {
                    char subChr = mmlString[mmlCount + 1];
                    if (subChr == '/') { isCommentBlock = false; mmlCount++; }
                }
                mmlCount++;
                continue;
            }
            if (isCommentLine)
            {
                if (chr == '\n') { isCommentLine = false; }
                mmlCount++;
                continue;
            }

            if (chr == 't')
            {
                /* tempo */
                int mmlTempo;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    mmlTempo = Mathf.Clamp(result[1], ConstValue.TEMPO_MIN, ConstValue.TEMPO_MAX);
                }
                else
                {
                    mmlTempo = ConstValue.DEFAULT_TEMPO;
                }
                mmlCount += result[0] + 1;
                seqList.Add(new SeqEvent(SEQ_CMD.SET_TEMPO, mmlTempo, 0));
                continue;
            }

            if (chr == '@')
            {
                /* program change */
                int mmlPc;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    mmlPc = Mathf.Clamp(result[1], 0, ConstValue.PROGRAM_CHANGE_MAX);
                }
                else
                {
                    mmlPc = ConstValue.DEFAULT_PROGRAM_CHANGE;
                }
                mmlCount += result[0] + 1;
                seqList.Add(new SeqEvent(SEQ_CMD.PROGRAM_CHANGE, mmlPc, 0));
                continue;
            }

            if (chr == 'v')
            {
                /* volume */
                int mmlVolume;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    mmlVolume = Mathf.Clamp(result[1], 0, ConstValue.SEQ_VOL_MAX);
                }
                else
                {
                    mmlVolume = ConstValue.SEQ_VOL_MAX;
                }
                mmlCount += result[0] + 1;
                seqList.Add(new SeqEvent(SEQ_CMD.VOLUME, mmlVolume, 0));
                continue;
            }

            if (chr == 'o')
            {
                /* octave */
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    mmlOctave = Mathf.Clamp(result[1], ConstValue.OCTAVE_MIN, ConstValue.OCTAVE_MAX);
                }
                else
                {
                    mmlOctave = ConstValue.DEFAULT_OCTAVE;
                }
                mmlCount += result[0] + 1;
                continue;
            }

            if (chr == '>')
            {
                /* octave up */
                mmlOctave++;
                mmlOctave = Mathf.Clamp(mmlOctave, 2, 8);
                mmlCount++;
                continue;
            }

            if (chr == '<')
            {
                /* octave down */
                mmlOctave--;
                mmlOctave = Mathf.Clamp(mmlOctave, 2, 8);
                mmlCount++;
                continue;
            }

            if (chr == 'l')
            {
                /* length */
                int mmlLength = ConstValue.DEFAULT_LENGTH;
                var result = MMLGetLength(mmlString, mmlCount + 1);
                if (result[1] > 0)
                {
                    mmlLength = Mathf.Clamp(result[1], ConstValue.LENGTH_MIN, ConstValue.LENGTH_MAX);
                }
                baseTics = tickPerNote * 4 / mmlLength;
                int bDot = baseTics;
                for (int i = 0; i < result[2]; i++)
                {
                    bDot /= 2;
                    baseTics += bDot;
                }
                mmlCount += result[0] + 1;
                continue;
            }

            if (chr == 'n')
            {
                /* note number */
                int tmpNoteNum = ConstValue.DEFAULT_NOTE_NUM;
                int tmpLength = 0;
                int tmpDots = 0;
                var resultN = MMLGetNum(mmlString, mmlCount + 1);
                if (resultN[1] > 0)
                {
                    tmpNoteNum = Mathf.Clamp(resultN[1], 1, 127);
                }
                mmlCount += resultN[0];
                if (mmlCount + 1 < mmlString.Length)
                {
                    char subChr = mmlString[mmlCount + 1];
                    if (subChr == ',')
                    {
                        mmlCount++;
                        var resultL = MMLGetLength(mmlString, mmlCount + 1);
                        if (resultL[1] > 0)
                        {
                            tmpLength = Mathf.Clamp(resultL[1], ConstValue.LENGTH_MIN, ConstValue.LENGTH_MAX);
                        }
                        tmpDots = resultL[2];
                        mmlCount += resultL[0] + 1;
                        if (mmlCount < mmlString.Length)
                        {
                            subChr = mmlString[mmlCount];
                            if (subChr == '&')
                            {
                                tmpNoteNum = -tmpNoteNum;
                            }
                        }
                    }
                }
                int tmpDulation;
                if (tmpLength > 0)
                {
                    tmpDulation = tickPerNote * 4 / tmpLength;
                }
                else
                {
                    tmpDulation = baseTics;
                }
                int tDot = tmpDulation;
                for (int i = 0; i < tmpDots; i++)
                {
                    tDot /= 2;
                    tmpDulation += tDot;
                }
                seqList.Add(new SeqEvent(SEQ_CMD.NOTE_ON, tmpNoteNum, tmpDulation));
                continue;
            }

            if (chr >='a' && chr <= 'g')
            {
                /* note */
                int tmpNoteNum = noteOffsetTable[chr - 'a'] + (mmlOctave + 1) * ConstValue.SEMITONES_IN_OCTAVE;
                int tmpLength = 0;
                int subCount = 0;
                char subChr;
                while (mmlCount + subCount < mmlString.Length)
                {
                    if (mmlCount + subCount + 1 >= mmlString.Length) { break; }
                    subChr = mmlString[mmlCount + subCount + 1];
                    if (subChr == '+' || subChr == '-')
                    {
                        if (subChr == '+') { tmpNoteNum++; subCount++; continue; }
                        if (subChr == '-') { tmpNoteNum--; subCount++; continue; }
                    }
                    break;
                }
                mmlCount += subCount;
                var result = MMLGetLength(mmlString, mmlCount + 1);
                if (result[1] > 0)
                {
                    tmpLength = Mathf.Clamp(result[1], ConstValue.LENGTH_MIN, ConstValue.LENGTH_MAX);
                }
                mmlCount += result[0] + 1;
                if (mmlCount < mmlString.Length)
                {
                    subChr = mmlString[mmlCount];
                    if (subChr == '&')
                    {
                        tmpNoteNum = -tmpNoteNum;
                    }
                }

                int tmpDulation;
                if (tmpLength > 0)
                {
                    tmpDulation = tickPerNote * 4 / tmpLength;
                }
                else
                {
                    tmpDulation = baseTics;
                }
                int tDot = tmpDulation;
                for (int i = 0; i < result[2]; i++)
                {
                    tDot /= 2;
                    tmpDulation += tDot;
                }
                seqList.Add(new SeqEvent(SEQ_CMD.NOTE_ON, tmpNoteNum, tmpDulation));
                continue;
            }

            if (chr == 'r')
            {
                /* rest */
                var result = MMLGetLength(mmlString, mmlCount + 1);
                int tmpDulation;
                if (result[1] > 0)
                {
                    tmpDulation = tickPerNote * 4 / Math.Clamp(result[1], ConstValue.LENGTH_MIN, ConstValue.LENGTH_MAX);
                }
                else
                {
                    tmpDulation = baseTics;
                }
                int tDot = tmpDulation;
                for (int i = 0; i < result[2]; i++)
                {
                    tDot /= 2;
                    tmpDulation += tDot;
                }
                seqList.Add(new SeqEvent(SEQ_CMD.REST, 0, tmpDulation));
                mmlCount += result[0] + 1;
                continue;
            }

            if (chr == '&')
            {
                /* tie */
                seqList.Add(new SeqEvent(SEQ_CMD.NOTE_TIE, 0, 0));
                mmlCount++;
                continue;
            }

            if (chr == '[')
            {
                /* repeat start */
                seqList.Add(new SeqEvent(SEQ_CMD.REPEAT_START, 0, 0));
                mmlCount++;
                continue;
            }

            if (chr == ']')
            {
                /* repeat end */
                int repeatNum = 1;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    repeatNum = Mathf.Clamp(result[1], 1, ConstValue.REPEAT_MAX);
                }
                seqList.Add(new SeqEvent(SEQ_CMD.REPEAT_END, repeatNum, 0));
                mmlCount += result[0] + 1;
                continue;
            }

            if (chr == 'L')
            {
                /* loop point */
                seqList.Add(new SeqEvent(SEQ_CMD.LOOP_POINT, 0, 0));
                mmlCount++;
                continue;
            }

            if (chr == 'T')
            {
                /* tune (Hz) */
                int mmlTune;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    mmlTune = Mathf.Clamp(result[1], ConstValue.A4_FREQ_MIN, ConstValue.A4_FREQ_MAX);
                }
                else
                {
                    mmlTune = ConstValue.DEFAULT_A4_FREQ;
                }
                mmlCount += result[0] + 1;
                seqList.Add(new SeqEvent(SEQ_CMD.TUNE, mmlTune, 0));
                continue;
            }

            if (chr == 'G')
            {
                /* gate step rate (percent) */
                int mmlGate;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    mmlGate = Mathf.Clamp(result[1], 1, ConstValue.GATE_MAX);
                }
                else
                {
                    mmlGate = 100;
                }
                mmlCount += result[0] + 1;
                seqList.Add(new SeqEvent(SEQ_CMD.GATE_STEP_RATE, mmlGate, 0));
                continue;
            }

            if (chr == 'S')
            {
                /* sweep (cent) */
                int tmpRate = 0;
                var result = MMLGetSignedNum(mmlString, mmlCount + 1);
                if (result[1] >= 0)
                {
                    tmpRate = result[1] * result[2];
                    tmpRate = Mathf.Clamp(tmpRate, ConstValue.SWEEP_MIN, ConstValue.SWEEP_MAX);
                }
                mmlCount += result[0] + 1;
                seqList.Add(new SeqEvent(SEQ_CMD.SWEEP, tmpRate, 0));
                continue;
            }

            if (chr == 'V')
            {
                /* envelope */
                int tmpId = 0;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[0] > 0)
                {
                    tmpId = result[1];
                }
                mmlCount += result[0] + 1;

                if (mmlString[mmlCount] == '{')
                {
                    /* set envelope params */
                    envParamsList.Clear();
                    var pResult = MMLGetParamNum(mmlString, mmlCount + 1);
                    envParamsList.AddRange(pResult);
                    int subCount = envParamsList[0];
                    if (subCount > 0)
                    {
                        envParamsList.RemoveAt(0);
                        mmlCount += subCount + 1;
                        if (mmlString[mmlCount] == '}')
                        {
                            seqList.Add(new SeqEvent(SEQ_CMD.ENV_PARAM_START, tmpId, 0));
                            int envDefaultVol = ConstValue.SEQ_VOL_MAX;
                            foreach (var param in envParamsList)
                            {
                                int envVal = param;
                                if (envVal == PARAM_OMIT) { // パラメーターが省略された
                                    envVal = envDefaultVol;
                                } else
                                {
                                    if (envVal >= 0) { envDefaultVol = envVal; }
                                }
                                seqList.Add(new SeqEvent(SEQ_CMD.ENV_PARAM, Mathf.Clamp(envVal, -1, ConstValue.SEQ_VOL_MAX), 0));
                            }
                            seqList.Add(new SeqEvent(SEQ_CMD.ENV_PARAM_END, 0, 0));
                        }
                    }
                    envParamsList.Clear();
                }
                else
                {
                    seqList.Add(new SeqEvent(SEQ_CMD.ENV_ON, tmpId, 0));
                }
                continue;
            }

            if (chr == 'M')
            {
                /* LFO */
                int tmpId = 0;
                var result = MMLGetNum(mmlString, mmlCount + 1);
                if (result[1] >= 0)
                {
                    tmpId = result[1];
                }
                mmlCount += result[0] + 1;

                if (mmlString[mmlCount] == '{')
                {
                    /* set LFO params */
                    var pResult = MMLGetParamNum(mmlString, mmlCount + 1);
                    int subCount = pResult[0];
                    if (subCount > 0 && pResult.Count > 3)
                    {
                        mmlCount += subCount + 1;
                        if (mmlString[mmlCount] == '}')
                        {
                            if (tmpId > 0)
                            {
                                int _lDelay = (pResult[1] >= 0) ? pResult[1] : 0;
                                int _lDeapth = (pResult[2] >= 0) ? pResult[2] : 0;
                                int _lSpeed = (pResult[3] >= 0) ? pResult[3] : 1;
                                seqList.Add(new SeqEvent(SEQ_CMD.LFO_SET, tmpId, 0));
                                seqList.Add(new SeqEvent(SEQ_CMD.LFO_DELAY, Mathf.Clamp(_lDelay, 0, ConstValue.LFO_MAX), 0));
                                seqList.Add(new SeqEvent(SEQ_CMD.LFO_DEAPTH, Mathf.Clamp(_lDeapth, 0, ConstValue.LFO_MAX), 0));
                                seqList.Add(new SeqEvent(SEQ_CMD.LFO_SPEED, Mathf.Clamp(_lSpeed, 1, ConstValue.LFO_MAX), 0));
                                seqList.Add(new SeqEvent(SEQ_CMD.LFO_PARAM_END, 0, 0));
                            }
                        }
                    }
                }
                else
                {
                    if (tmpId > 0)
                    {
                        seqList.Add(new SeqEvent(SEQ_CMD.LFO_SET, tmpId, 0));
                    }
                    else
                    {
                        seqList.Add(new SeqEvent(SEQ_CMD.LFO_SET, -1, 0));
                    }
                }
                continue;
            }

            /*string s = "";
            switch (chr)
            {
                case ' ':
                    s = "SP";
                    break;
                case '\n':
                    s = "\\n";
                    break;
                case '\r':
                    s = "\\r";
                    break;
                default:
                    s = string.Empty + chr;
                    break;
            }
            Debug.Log("MML Decode : " + s);*/

            mmlCount++;
        }
        seqList.Add(new SeqEvent(SEQ_CMD.END_OF_SEQ, 0, 0));
        return seqList;
    }

    private int[] MMLGetNum(string _mmlString, int offset)
    {
        int val = -1;
        int subCount = 0;
        while (true)
        {
            if (offset + subCount >= _mmlString.Length)
            {
                break;
            }
            char subChr = _mmlString[offset + subCount];
            if (subChr >= '0' && subChr <= '9')
            {
                if (val < 0) { val = 0; }
                val = val * 10 + (subChr - '0');
            }
            else
            {
                break;
            }
            subCount++;
        }
        int[] result = { subCount, val };
        return result;
    }

    private int[] MMLGetSignedNum(string _mmlString, int offset)
    {
        int val = -1;
        int subCount = 0;
        bool isPlus = true;
        while (true)
        {
            if (offset + subCount >= _mmlString.Length) { break; }

            char subChr = _mmlString[offset + subCount];
            if ((subChr >= '0' && subChr <= '9') || subChr == '+' || subChr == '-')
            {
                if (subChr == '+') { isPlus = true; subCount++; continue; }
                if (subChr == '-') { isPlus = false; subCount++; continue; }

                if (val < 0) { val = 0; }
                val = val * 10 + (subChr - '0');
            }
            else
            {
                break;
            }
            subCount++;
        }
        int[] result = { subCount, val, isPlus ? 1 : -1 };
        return result;
    }

    private List<int> MMLGetParamNum(string _mmlString, int offset)
    {
        int val = -1;
        bool loop = false;
        int subCount = 0;
        List<int> paramList = new List<int>();
        while (true)
        {
            if (offset + subCount >= _mmlString.Length) { break; }

            char subChr = _mmlString[offset + subCount];
            if ((subChr >='0' && subChr <= '9') || subChr == ',' || subChr == '|')
            {
                if (subChr == ',')
                {
                    if (loop)
                    {
                        paramList.Add(-1);
                        loop = false;
                    }
                    else
                    {
                        if (val < 0) { val = PARAM_OMIT; }
                        paramList.Add(val);
                    }
                    val = -1;
                    subCount++;
                    continue;
                }
                if (subChr == '|')
                {
                    loop = true;
                    subCount++;
                    continue;
                }
                if (val < 0) { val = 0; }
                val = val * 10 + (subChr - '0');
            }
            else
            {
                break;
            }
            subCount++;
        }
        if (val >= 0)
        {
            paramList.Add(val);
        }
        paramList.Insert(0, subCount);
        return paramList;
    }

    private int[] MMLGetLength(string _mmlString, int offset)
    {
        int _length = 0;
        int _dots = 0;
        int subCount = 0;
        while (true)
        {
            if (offset + subCount >= _mmlString.Length)
            {
                break;
            }
            char subChr = _mmlString[offset + subCount];
            if (subChr >= '0' && subChr <= '9' || subChr == '.')
            {
                if (subChr == '.')
                {
                    _dots++;
                }
                else
                {
                    _length = _length * 10 + (subChr - '0');
                }
            }
            else
            {
                break;
            }
            subCount++;
        }
        int[] result = { subCount, _length, _dots };
        return result;
    }
}
