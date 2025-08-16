using System.Collections.Generic;
using UnityEngine;
using uPSG;

public class PSGPlayer : MonoBehaviour
{
    [SerializeField] private MMLDecoder mmlDecoder;
    [SerializeField] private AudioSource audioSource;
    /// <summary>
    /// サンプルレート
    /// </summary>
    public int sampleRate = 32000;
    /// <summary>
    /// AudioClipの長さ（ミリ秒）
    /// </summary>
    public int audioClipSizeMilliSec = 1000;
    /// <summary>
    /// オクターブ4のラの音の周波数
    /// </summary>
    public float a4Freq = 440f;
    /// <summary>
    /// 音色番号
    /// </summary>
    public int programChange;

    private int audioPosition = 0;
    private int audioPositionSetCount = 0;
    private bool stopAudio = false;
    private int noteNumber;
    private Queue<float> waveQueue = new();
    private float waveFreq;
    private double waveLength;
    private uint wavePositon;
    private float waveVolume = 1f;
    private double waveNextEventPosition;
    private double waveDevide;
    private bool waveHigh = true;
    private float waveSample;
    private bool waveTie = false;
    private readonly float[] pulseWidth = { 12.5f, 25f, 50f, 75f };
    private float[] triangleTable = new float[32];
    private int ttIndex;
    private int noiseReg;
    private bool noiseShortFreq;
    private readonly int[] noiseTable = { 
        0x002, 0x004, 0x008, 0x010, 0x020, 0x030, 0x040, 0x050, 
        0x065, 0x07f, 0x0be, 0x0fe, 0x17d, 0x1fc, 0x39f, 0x7f2 
    };
    private int noiseIndex = 0;
    private bool noiseShort = false;

    private uint seqPosition = 0;
    private float seqVolume = 1f;
    private double seqNextEventPosition = 0d;
    private double seqNextGatePosition = 0d;
    private double seqEndPosition = 0d;
    private int seqRepeatIndex = 0;
    private int seqRepeatCount = -1;
    private bool seqLoop = false;
    private int seqLoopIndex = 0;
    [SerializeField]
    private int seqListIndex = 0;
    [SerializeField]
    private List<SeqEvent> seqList = new();
    private int seqTempo = 120;
    private int ticPerNote = 960;
    private float gateStepRate = 1f;
    private int sweepPitch = 0;
    private int sweepPitchRate = 0;
    private int sweepDulation;
    private uint sweepNextEventPosition;
    private readonly int PITCH_MAX = (120 - 69) * 100;
    private readonly int PITCH_MIN = (12 - 69) * 100;
    private List<EnvPat> envPatList = new();
    private int envPatIndex = 0;
    private int envPatId = 0;
    private int envLoopIndex;
    private int envVolIndex = 0;
    private List<int> envParamList = new();
    private int envDulation;
    private uint envNextEventPosition;
    private bool isEnvOn = false;
    private List<LfoPat> lfoPatList = new();
    private int lfoPatIndex = 0;
    private int lfoPatId = 0;
    private int lfoDelay = 0;
    private int lfoSpeed = 0;
    private int lfoDeapth = 0;
    private int lfoCount = 0;
    private float lfoPitch = 0;
    private int lfoDulation;
    private uint lfoNextEventPosition;
    private bool isLfoOn = false;

    private bool audioMute = false;
    private bool seqMute = false;

    /// <summary>
    /// 演奏するMMLデータ
    /// </summary>
    [Multiline] public string mmlString = "";

    private void Awake()
    {
        if (mmlDecoder == null)
        {
            mmlDecoder = gameObject.AddComponent<MMLDecoder>();
        }
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        GenerateTriangleTable();

    }

    void Start()
    {

    }

    void Update()
    {
        if (stopAudio)
        {
            // コールバックから直接AudioSourceを操作できないので、メインスレッドから停止処理をする
            audioSource.loop = false;
            if (audioSource.timeSamples > seqEndPosition % sampleRate)
            {
                Stop();
                stopAudio = false;
            }
        }
        if (audioSource.loop == false && !audioSource.isPlaying && audioSource.clip != null)
        {
            audioSource.clip = null;
        }
    }

    /// <summary>
    /// MMLをデコードして再生
    /// </summary>
    public void Play()
    {
        if (DecodeMML()) {
            PlayDecoded();
        }
    }

    /// <summary>
    /// _mmlStringで指定したMMLをデコードして再生
    /// </summary>
    /// <param name="_mmlString">MML文字列</param>
    public void Play(string _mmlString)
    {
        mmlString = _mmlString;
        Play();
    }

    /// <summary>
    /// MMLをシーケンスデータにデコード
    /// </summary>
    /// <returns>デコード成功ならTrue</returns>
    public bool DecodeMML()
    {
        if (mmlString == "") {
            Debug.Log("No MML string : " + gameObject.name);
            return false;
        }
        seqList.Clear();
        seqList.AddRange(mmlDecoder.Decode(mmlString, ticPerNote)); // MML Decoderでシーケンスデータ（List）に変換
        if (seqList.Count == 0) {
            Debug.Log("No sequence data : " + gameObject.name);
            return false;
        }
        return true;
    }

    /// <summary>
    /// デコード済のシーケンスデータを再生
    /// </summary>
    public void PlayDecoded()
    {
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not attached : " + gameObject.name);
            return;
        }
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioPosition = 0;
        audioPositionSetCount = 0;
        stopAudio = false;
        waveVolume = 1f;
        seqListIndex = 0;
        seqPosition = 0;
        seqNextEventPosition = 0d;
        seqNextGatePosition = 0d;
        seqRepeatCount = -1;
        seqLoop = false;
        gateStepRate = 1f;
        waveTie = false;
        noiseShort = false;
        programChange = 2;
        envPatIndex = 0;
        envDulation = sampleRate / 60;
        envPatList.Clear();
        envVolIndex = 0;
        sweepPitch = 0;
        sweepPitchRate = 0;
        sweepDulation = sampleRate / 60;
        isLfoOn = false;
        lfoPatList.Clear();
        lfoDelay = 0;
        lfoDeapth = 0;
        lfoSpeed = 1;
        lfoCount = 0;
        lfoDulation = sampleRate / 60;

        audioClipSizeMilliSec = Mathf.Clamp(audioClipSizeMilliSec, 20, 1000);
        // ストリーム再生のAudioClipを生成（生成時にバッファ分のサンプルをOnAudioReadで要求する）
        AudioClip channelClip = AudioClip.Create("PSG Sound", (int)(sampleRate * ((float)audioClipSizeMilliSec / 1000f)), 1, sampleRate, true, OnAudioRead, OnAudioSetPosition);
        audioSource.clip = channelClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    public void Loop(bool val)
    {
        audioSource.loop = val;
    }

    /// <summary>
    /// 再生を停止
    /// </summary>
    public void Stop()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }

    /// <summary>
    /// AudioSourceのクリップが再生されているか
    /// </summary>
    /// <returns>再生中はTrue</returns>
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    /// <summary>
    /// 再生中の音量をミュートして音を消す
    /// </summary>
    /// <param name="isOn">Trueでミュート有効</param>
    public void Mute(bool isOn)
    {
        audioMute = isOn;
        audioSource.mute = audioMute;
        if (audioMute) { seqMute = true; }
    }

    private void OnAudioRead(float[] data)
    {
        // 再生時にAudioClipから、サンプルデータのブロックを生成するために、断続的に呼び出される
        GenerateSound(data.Length); // サンプルデータ生成
        for (int i=0; i<data.Length; i++)
        {
            // キューからバッファに読み込む
            data[i] = (waveQueue.Count > 0) ? waveQueue.Dequeue() : 0;
        }
    }

    private void OnAudioSetPosition(int newPosition)
    {
        // クリップがループしたり再生位置を変更したりするときに呼び出される
        audioPosition = newPosition;
        if (seqListIndex < 0)
        {
            // AudioSourceはメインスレッドでしか操作できないので、stopAudioフラグを立てる
            stopAudio = seqEndPosition < audioPositionSetCount * (int)(sampleRate * ((float)audioClipSizeMilliSec / 1000f));
        }
        if (audioPosition == 0)
        {
            // クリップがループした瞬間
            audioPositionSetCount++;
        }
    }

    private void GenerateSound(int bufferSize)
    {
        // 音のサンプルデータを順次生成
        float _sample;
        for (int i = 0; i < bufferSize; i++)
        {
            if (seqPosition >= System.Math.Round(seqNextGatePosition, 0, System.MidpointRounding.AwayFromZero))
            {
                /*** GATE ***/
                waveFreq = 0;
            }

            if (seqPosition >= System.Math.Round(seqNextEventPosition, 0, System.MidpointRounding.AwayFromZero))
            {
                // シーケンス処理
                GetSeqEvent();
            }

            if (waveFreq != 0 && !seqMute)
            {
                /****** WAVE VOLUME ******/

                if (isEnvOn)
                {
                    /*** ENVELOPE ***/
                    if (wavePositon >= envNextEventPosition && envPatIndex < envPatList.Count)
                    {
                        waveVolume = envPatList[envPatIndex].envVolList[envVolIndex] / 15f;
                        envVolIndex++;
                        if (envVolIndex >= envPatList[envPatIndex].envVolList.Count)
                        {
                            envVolIndex = envLoopIndex;
                        }
                        envNextEventPosition += (uint)envDulation;
                    }
                }
                else
                {
                    /*** VOLUME ***/
                    waveVolume = seqVolume;
                }

                /****** WAVE FREQUENCY ******/

                if (sweepPitchRate != 0)
                {
                    /*** SWEEP ***/
                    if (wavePositon >= sweepNextEventPosition)
                    {
                        int x = (Mathf.Clamp(noteNumber, 12, 120) - 69) * 100 + sweepPitch;
                        if (x >= PITCH_MAX || x <= PITCH_MIN)
                        {
                            // スイープした周波数が下限か上限に達した場合、音を止める
                            waveFreq = 0;
                            waveLength = sampleRate / a4Freq;
                        }
                        else
                        {
                            // 周波数を算出
                            waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / 1200d));
                            waveLength = sampleRate / waveFreq;
                            sweepPitch += sweepPitchRate;
                        }
                        sweepNextEventPosition += (uint)sweepDulation;
                    }
                }
                else
                {
                    //sweepPitch = 0;
                    if (!isLfoOn)
                    {
                        int x = (Mathf.Clamp(noteNumber, 12, 120) - 69) * 100 + sweepPitch;
                        waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / 1200d));
                        waveLength = sampleRate / waveFreq;
                    }
                    sweepNextEventPosition = wavePositon;
                }

                if (isLfoOn)
                {
                    /*** LFO ***/
                    if (wavePositon >= lfoNextEventPosition)
                    {
                        if (wavePositon < lfoDulation * lfoDelay / 2)
                        {
                            // Delayで設定した分LFOをかけない
                            lfoNextEventPosition = (uint)(lfoDulation * lfoDelay / 2);
                        }
                        else
                        {
                            // ピッチは三角波と同じテーブルにDeapthを掛ける
                            lfoPitch = (triangleTable[lfoCount] * lfoDeapth * 100f / 255f);
                            int x = (Mathf.Clamp(noteNumber, 12, 120) - 69) * 100 + (int)lfoPitch;
                            waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / 1200d));
                            waveLength = sampleRate / waveFreq;
                            lfoCount++;
                            if (lfoCount >= triangleTable.Length) { lfoCount -= triangleTable.Length; }
                            // Speedで設定した間隔で処理する
                            lfoNextEventPosition += (uint)(sampleRate / (lfoSpeed * 2));
                        }
                    }
                }
            }

            if (waveFreq == 0 || seqMute)
            {
                /*** REST ***/
                // 周波数0を無音とする
                _sample = 0;
            }
            else
            {
                // 周波数と音量からサンプルを生成
                if (programChange < 4)
                {
                    GenerateSquareSound();
                }
                if (programChange == 4)
                {
                    GenerateTriangleSound();
                }
                if (programChange == 5 || programChange == 6)
                {
                    GenerateNoiseSound();
                }
                _sample = waveSample * waveVolume;
            }
            waveQueue.Enqueue(_sample);
            wavePositon++;
            seqPosition++;
        }
    }

    private void GetSeqEvent()
    {
        // シーケンスデータ処理
        if (seqListIndex < 0) { return; }
        if ((seqListIndex == seqLoopIndex) && seqLoop)
        {
            seqNextEventPosition = 0d;
            seqPosition = 0;
        }
        bool tieGate = false;
        bool eos = false;
        int _duration = 0;
        while(_duration == 0)
        {
            // Dulationが0のコマンドを全て処理する
            switch (seqList[seqListIndex].seqCmd)
            {
                case SEQ_CMD.SET_TEMPO:
                    seqTempo = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.NOTE_ON:
                    noteNumber = seqList[seqListIndex].seqParam;
                    if (noteNumber < 0)
                    {
                        // 負のノートナンバーの場合はタイ（&）で繋げる音なので、ゲートを100%にする
                        tieGate = true;
                        noteNumber = -noteNumber;
                    }
                    if (programChange < 5)
                    {
                        // スイープした周波数が下限か上限に達した場合、音を止める
                        int x = (Mathf.Clamp(noteNumber, 12, 120) - 69) * 100 + sweepPitch;   // ノートナンバー69がA4
                        if (x >=PITCH_MAX || x <= PITCH_MIN)
                        {
                            waveFreq = 0;
                            waveLength = sampleRate / a4Freq;
                        }
                        else
                        {
                            waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / 1200d));
                            waveLength = sampleRate / waveFreq;
                        }
                    }
                    else
                    {
                        // ノイズは0から15までなので、ノートナンバーを16で割った剰余にする
                        waveFreq = a4Freq;
                        noiseIndex = noteNumber % 16;
                    }
                    PrepareSound();
                    break;
                case SEQ_CMD.NOTE_TIE:
                    waveTie = true;
                    break;
                case SEQ_CMD.GATE_STEP_RATE:
                    gateStepRate = seqList[seqListIndex].seqParam / 100f;
                    break;
                case SEQ_CMD.REST:
                    waveFreq = 0;
                    sweepPitch = 0;
                    waveLength = sampleRate / a4Freq;
                    break;
                case SEQ_CMD.PROGRAM_CHANGE:
                    programChange = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.SHORT_NOISE:
                    noiseShort = (seqList[seqListIndex].seqParam != 0) ? true : false;
                    break;
                case SEQ_CMD.VOLUME:
                    seqVolume = Mathf.Clamp01(seqList[seqListIndex].seqParam / 15f);
                    waveVolume = seqVolume;
                    isEnvOn = false;
                    break;
                case SEQ_CMD.REPEAT_START:
                    // リピートの開始位置を保存
                    seqRepeatIndex = seqListIndex;
                    break;
                case SEQ_CMD.REPEAT_END:
                    if (seqRepeatCount < 0)
                    {
                        // カウンターが-1（リピートしてない）ならリピート回数を保存
                        seqRepeatCount = seqList[seqListIndex].seqParam - 1;
                    }
                    else
                    {
                        // カウンターが0以上なら1減らす
                        seqRepeatCount--;
                    }
                    if (seqRepeatCount > 0)
                    {
                        // カウンターが1以上ならリピート開始位置に戻す
                        seqListIndex = seqRepeatIndex;
                    }
                    else
                    {
                        // カウンターが0になったら-1にする（リピート終了）
                        seqRepeatCount = -1;
                    }
                    break;
                case SEQ_CMD.SWEEP:
                    sweepPitchRate = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.ENV_ON:
                    envPatId = seqList[seqListIndex].seqParam;
                    if (envPatId <= 0)
                    {
                        envPatId = 0;
                        isEnvOn = false;
                        break;
                    }
                    envPatIndex = -1;
                    for (int i=0; i<envPatList.Count; i++)
                    {
                        // エンベロープ番号が登録されてるか
                        if (envPatList[i].envId == envPatId)
                        {
                            envPatIndex = i;
                            break;
                        }
                    }
                    if (envPatIndex < 0)
                    {
                        // 登録されてなければOFF
                        isEnvOn = false;
                    }
                    else
                    {
                        isEnvOn = true;
                        envVolIndex = 0;
                        envLoopIndex = envPatList[envPatIndex].envLoopIndex;
                        envNextEventPosition = 0;
                    }
                    break;
                case SEQ_CMD.ENV_PARAM_START:
                    // エンベロープ設定の準備
                    envPatId = seqList[seqListIndex].seqParam;
                    envParamList.Clear();
                    envLoopIndex = -1;
                    break;
                case SEQ_CMD.ENV_PARAM:
                    if (seqList[seqListIndex].seqParam < 0)
                    {
                        // -1の場所がループポイント
                        envLoopIndex = envParamList.Count;
                    }
                    else
                    {
                        envParamList.Add(seqList[seqListIndex].seqParam);
                    }
                    break;
                case SEQ_CMD.ENV_PARAM_END:
                    if (envPatId <= 0) { break; }
                    if (envParamList.Count < 1) { break; }
                    if (envLoopIndex < 0 || envLoopIndex >= envParamList.Count)
                    {
                        // ループポイントが設定されてなければ最後のパラメータをループ
                        envLoopIndex = envParamList.Count - 1;
                    }
                    for (int i = 0; i < envPatList.Count; i++)
                    {
                        if (envPatList[i].envId == envPatId)
                        {
                            // エンベロープ番号が登録されてれば削除
                            envPatList.RemoveAt(i);
                            break;
                        }
                    }
                    envPatList.Add(new EnvPat(envPatId, envParamList, envLoopIndex));
                    envPatIndex = envPatList.Count;
                    envParamList.Clear();
                    break;
                case SEQ_CMD.LFO_SET:
                    lfoPatId = seqList[seqListIndex].seqParam;
                    if (lfoPatId <= 0)
                    {
                        // LFO番号が0以下ならLFOを無効にする
                        lfoPatId = 0;
                        isLfoOn = false;
                        break;
                    }
                    lfoPatIndex = -1;
                    for (int i = 0; i < lfoPatList.Count; i++)
                    {
                        // LFOリストにLFO番号が登録されてるか確認
                        if (lfoPatList[i].lfoId == lfoPatId)
                        {
                            lfoPatIndex = i;
                            break;
                        }
                    }
                    if (lfoPatIndex < 0)
                    {
                        // 登録されてなければLFO無効
                        isLfoOn = false;
                    }
                    else
                    {
                        isLfoOn = true;
                        lfoCount = 0;
                        lfoDelay = lfoPatList[lfoPatIndex].lfoDelay;
                        lfoDeapth = lfoPatList[lfoPatIndex].lfoDeapth;
                        lfoSpeed = lfoPatList[lfoPatIndex].lfoSpeed;
                        lfoNextEventPosition = 0;
                    }
                    break;
                case SEQ_CMD.LFO_DELAY:
                    lfoDelay = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.LFO_DEAPTH:
                    lfoDeapth = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.LFO_SPEED:
                    lfoSpeed = seqList[seqListIndex].seqParam;
                    lfoSpeed = Mathf.Clamp(lfoSpeed, 1, 255);
                    break;
                case SEQ_CMD.LFO_PARAM_END:
                    if (lfoPatId <= 0)
                    {
                        break;
                    }
                    lfoPatIndex = -1;
                    for (int i = 0; i < lfoPatList.Count; i++)
                    {
                        // LFO番号が登録されてれば削除
                        if (lfoPatList[i].lfoId == lfoPatId)
                        {
                            lfoPatList.RemoveAt(i);
                            break;
                        }
                    }
                    lfoPatList.Add(new LfoPat(lfoPatId, lfoDelay, lfoDeapth, lfoSpeed));
                    isLfoOn = true;
                    break;
                case SEQ_CMD.LOOP_POINT:
                    seqLoopIndex = seqListIndex;
                    seqLoop = true;
                    break;
                case SEQ_CMD.TUNE:
                    a4Freq = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.END_OF_SEQ:
                    eos = true;
                    seqEndPosition = seqNextEventPosition + sampleRate * 60d / seqTempo * seqList[seqListIndex].seqStep / ticPerNote;
                    break;
            }
            _duration = seqList[seqListIndex].seqStep;
            seqNextGatePosition = seqNextEventPosition + sampleRate * 60d / seqTempo * _duration * (tieGate ? 1f : gateStepRate) / ticPerNote;
            seqNextEventPosition += sampleRate * 60d / seqTempo * _duration / ticPerNote;
            seqListIndex++;
            if (eos) { break; }
        }
        if (seqListIndex >= seqList.Count)
        {
            // シーケンス終了後の処理
            if (seqLoop)
            {
                seqListIndex = seqLoopIndex;
            }
            else
            {
                seqListIndex = -1;
                waveFreq = 0;
            }
        }
    }

    private void PrepareSound()
    {
        // 発音の前処理
        if (!audioMute && seqMute) { seqMute = false; }

        if (programChange < 4)
        {
            PrepareSquareSound();
        }
        if (programChange == 4)
        {
            PrepareTriangleSound();
        }
        if (programChange == 5 || programChange == 6)
        {
            noiseShort = (programChange == 5) ? false : true;
            PrepareNoiseSound(noiseIndex, noiseShort);
        }
        if (!waveTie) {
            wavePositon = 0;
            envVolIndex = 0;
            envNextEventPosition = 0;
            sweepPitch = 0;
            sweepNextEventPosition = 0;
            lfoCount = 0;
            lfoNextEventPosition = 0;
        }
        else
        {
            waveTie = false;
        }
    }

    private void PrepareSquareSound()
    {
        if (!waveTie)
        {
            waveNextEventPosition = waveDevide;
            waveHigh = true;
            waveSample = waveHigh ? 1 : -1;
        }
    }

    private void GenerateSquareSound()
    {
        waveDevide = waveLength * pulseWidth[Mathf.Clamp(programChange, 0, pulseWidth.Length)] / 100d;
        if (wavePositon >= System.Math.Round(waveNextEventPosition, 0, System.MidpointRounding.AwayFromZero))
        {
            waveHigh = !waveHigh;
            waveSample = waveHigh ? 1 : -1;
            waveNextEventPosition += (waveSample > 0) ? (waveLength - waveDevide) : waveDevide;
        }
    }

    private void PrepareTriangleSound()
    {
        if (!waveTie)
        {
            waveNextEventPosition = waveDevide;
            ttIndex = 0;
            waveSample = triangleTable[ttIndex];
        }
    }

    private void GenerateTriangleSound()
    {
        waveDevide = waveLength / triangleTable.Length;
        if (wavePositon > System.Math.Round(waveNextEventPosition, 0, System.MidpointRounding.AwayFromZero))
        {
            ttIndex++;
            if (ttIndex >= triangleTable.Length)
            {
                ttIndex = 0;
            }
            waveNextEventPosition += waveDevide;
        }
        waveSample = triangleTable[ttIndex];
    }

    private void PrepareNoiseSound(int _noiseIndex, bool _noiseShort)
    {
        int noiseFreq = noiseTable[_noiseIndex];
        noiseShortFreq = _noiseShort;
        if (!waveTie)
        {
            noiseReg = 0x8000;
            waveNextEventPosition = waveDevide;
        }
        waveDevide = (double)noiseFreq * (double)sampleRate / 894886.25d;
    }

    private void GenerateNoiseSound()
    {
        if (wavePositon >= System.Math.Round(waveNextEventPosition, 0, System.MidpointRounding.AwayFromZero))
        {
            int output = 0;
            double dummyPos = 0;
            int sSum = 0;
            int sNum = 0;
            do
            {
                noiseReg >>= 1;
                noiseReg |= ((noiseReg ^ (noiseReg >> (noiseShortFreq ? 6 : 1))) & 1) << 15;
                output = noiseReg & 1;
                dummyPos += waveDevide;
                sSum += output;
                sNum++;
            } while (dummyPos < 1);
            waveSample = ((float)sSum / (float)sNum * 2f - 1f);
            waveNextEventPosition += dummyPos;
        }
    }

    private void GenerateTriangleTable()
    {
        // 4bit分解能の三角波テーブル
        float tVal = 0f;
        bool tRise = true;
        for (int i=0; i<triangleTable.Length; i++)
        {
            if (tRise)
            {
                // 上行
                tVal += 1f / 8f;
                if (tVal >= 1f)
                {
                    tVal = 1f;
                    tRise = false;
                }
            }
            else
            {
                // 下行
                tVal -= 1f / 8f;
                if (tVal <= -1)
                {
                    tVal = -1f;
                    tRise = true;
                }
            }
            triangleTable[i] = tVal;
        }
    }
}
