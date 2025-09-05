using System.Collections.Generic;
using UnityEngine;
using uPSG;

public class PSGPlayer : MonoBehaviour
{
    /// <summary>
    /// Specify the MML Decoder component
    /// </summary>
    [Tooltip("Specify the MML Decoder component")]
    [SerializeField] private MMLDecoder mmlDecoder;
    /// <summary>
    /// Specifies the destination AudioSource
    /// </summary>
    [Tooltip("Specifies the destination AudioSource")]
    [SerializeField] private AudioSource audioSource;
    /// <summary>
    /// Sample rate
    /// </summary>
    [Tooltip("Sample rate")]
    public int sampleRate = ConstValue.DEFAULT_SAMPLE_RATE;
    /// <summary>
    /// AudioClip duration (milliseconds)
    /// </summary>
    [Tooltip("AudioClip duration (milliseconds)")]
    public int audioClipSizeMilliSec = ConstValue.DEFAULT_AUDIO_CLIP_SIZE;
    /// <summary>
    /// Frequency of the A note at the 4th octave
    /// </summary>
    [Tooltip("Frequency of the A note at the 4th octave")]
    public float a4Freq = ConstValue.DEFAULT_A4_FREQ;
    /// <summary>
    /// 1 beat (quarter note) resolution
    /// </summary>
    [Tooltip("1 beat (quarter note) resolution")]
    public int tickPerNote = ConstValue.DEFAULT_TICK_PER_NOTE;
    /// <summary>
    /// Tone number
    /// </summary>
    [Tooltip("Tone number")]
    public int programChange = ConstValue.DEFAULT_PROGRAM_CHANGE;

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
    private float[] triangleTable = new float[ConstValue.TRIANGLE_TABLE_LENGTH];
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
    /// <summary>
    /// Sequence data playback position
    /// </summary>
    [Tooltip("Sequence data playback position")]
    [SerializeField] private int seqListIndex = 0;
    /// <summary>
    /// Sequence data
    /// </summary>
    [Tooltip("Sequence data")]
    [SerializeField] private List<SeqEvent> seqList = new();
    private int seqTempo = ConstValue.DEFAULT_TEMPO;
    private float gateStepRate = ConstValue.DEFAULT_GATE_STEP_RATE;
    private int sweepPitch = 0;
    private int sweepPitchRate = 0;
    private int sweepDulation;
    private uint sweepNextEventPosition;
    private readonly int PITCH_MAX = (ConstValue.NOTE_NUM_MAX - ConstValue.A4_NOTE_NUM) * 100;
    private readonly int PITCH_MIN = (ConstValue.NOTE_NUM_MIN - ConstValue.A4_NOTE_NUM) * 100;
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
    /// MML data for playback
    /// </summary>
    [Tooltip("MML data for playback")]
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
            // Since you cannot directly manipulate the AudioSource from the callback, stop it from the main thread.
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
    /// Decode and play MML
    /// </summary>
    public void Play()
    {
        if (DecodeMML()) {
            PlayDecoded();
        }
    }

    /// <summary>
    /// _Decode and play the MML specified in _mmlString
    /// </summary>
    /// <param name="_mmlString">MML string</param>
    public void Play(string _mmlString)
    {
        mmlString = _mmlString;
        Play();
    }

    /// <summary>
    /// Decode MML into sequence data
    /// </summary>
    /// <returns>True if decoding succeeded</returns>
    public bool DecodeMML()
    {
        if (mmlString == "") {
            Debug.Log("No MML string : " + gameObject.name);
            return false;
        }
        seqList.Clear();
        seqList.AddRange(mmlDecoder.Decode(mmlString, tickPerNote)); // Convert to sequence data (List) using MML Decoder
        if (seqList.Count == 0) {
            Debug.Log("No sequence data : " + gameObject.name);
            return false;
        }
        return true;
    }

    /// <summary>
    /// Playback of decoded sequence data
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
        seqTempo = ConstValue.DEFAULT_TEMPO;
        gateStepRate = ConstValue.DEFAULT_GATE_STEP_RATE;
        waveTie = false;
        noiseShort = false;
        programChange = ConstValue.DEFAULT_PROGRAM_CHANGE;
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

        audioClipSizeMilliSec = Mathf.Clamp(audioClipSizeMilliSec, ConstValue.AUDIO_CLIP_SIZE_MIN, ConstValue.AUDIO_CLIP_SIZE_MAX);
        // Generate an AudioClip for stream playback (requests samples for the buffer via OnAudioRead during generation)
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
    /// Stop playback
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
    /// Is the AudioSource clip playing?
    /// </summary>
    /// <returns>True while playing</returns>
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    /// <summary>
    /// Mute the volume of the currently playing audio to silence it
    /// </summary>
    /// <param name="isOn">True enables mute</param>
    public void Mute(bool isOn)
    {
        audioMute = isOn;
        audioSource.mute = audioMute;
        if (audioMute) { seqMute = true; }
    }

    /// <summary>
    /// Playback of sequence data (same as PlayDecoded())
    /// </summary>
    public void PlaySequence()
    {
        PlayDecoded();
    }

    /// <summary>
    /// Convert sequence data to JSON and return a string
    /// </summary>
    /// <param name="_prettyPrint">If True, format the output for readability</param>
    /// <returns>JSON formatted string</returns>
    public string ExportSeqJson(bool _prettyPrint)
    {
        if (seqList.Count < 1) { return null; }
        string jsonString = JsonUtility.ToJson(GetSeqJson(), _prettyPrint);
        return jsonString;
    }

    public string ExportSeqJson()
    {
        return ExportSeqJson(false);
    }

    /// <summary>
    /// Return sequence data in SeqJson class object
    /// </summary>
    /// <returns>SeqJson Class Object</returns>
    public SeqJson GetSeqJson()
    {
        SeqJson seqJson = new();
        seqJson.jsonTickPerNote = tickPerNote;
        seqJson.jsonSeqList.Clear();
        seqJson.jsonSeqList.AddRange(seqList);
        return seqJson;
    }

    /// <summary>
    /// Decode MML and convert sequence data to JSON
    /// </summary>
    /// <param name="_prettyPrint">If True, format the output for readability</param>
    /// <returns>JSON formatted string</returns>
    public string DecodeAndExportSeqJson(bool _prettyPrint)
    {
        if (!DecodeMML()) { return null; }
        return ExportSeqJson(_prettyPrint);
    }

    public string DecodeAndExportSeqJson()
    {
        return DecodeAndExportSeqJson(false);
    }

    /// <summary>
    /// Read JSON as sequence data
    /// </summary>
    /// <param name="_jsonString">JSON formatted string</param>
    /// <returns>True if import succeeded</returns>
    public bool ImportSeqJson(string _jsonString)
    {
        SeqJson seqJson = JsonUtility.FromJson<SeqJson>(_jsonString);
        return SetSeqJson(seqJson);
    }

    /// <summary>
    /// Import the SeqJson class object directly into sequence data
    /// </summary>
    /// <param name="_seqJson">SeqJson Class Object</param>
    /// <returns>True if import succeeded</returns>
    public bool SetSeqJson(SeqJson _seqJson)
    {
        if (_seqJson.jsonTickPerNote > 0 && _seqJson.jsonSeqList.Count > 0)
        {
            tickPerNote = _seqJson.jsonTickPerNote;
            seqList.Clear();
            seqList.AddRange(_seqJson.jsonSeqList);
            return true;
        }
        return false;
    }

    /*********************************/

    private void OnAudioRead(float[] data)
    {
        // During playback, it is intermittently called to generate blocks of sample data from the AudioClip.
        GenerateSound(data.Length); // Generating sample data
        for (int i=0; i<data.Length; i++)
        {
            // Read from queue to buffer
            data[i] = (waveQueue.Count > 0) ? waveQueue.Dequeue() : 0;
        }
    }

    private void OnAudioSetPosition(int newPosition)
    {
        // Called when the clip loops or changes the playback position.
        audioPosition = newPosition;
        if (seqListIndex < 0)
        {
            // Since AudioSource can only be manipulated on the main thread, set the stopAudio true.
            stopAudio = seqEndPosition < audioPositionSetCount * (int)(sampleRate * ((float)audioClipSizeMilliSec / 1000f));
        }
        if (audioPosition == 0)
        {
            // The moment the clip looped
            audioPositionSetCount++;
        }
    }

    private void GenerateSound(int bufferSize)
    {
        // Generate sound sample data sequentially
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
                // Sequence processing
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
                        waveVolume = envPatList[envPatIndex].envVolList[envVolIndex] / (float)ConstValue.SEQ_VOL_MAX;
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
                        int x = (Mathf.Clamp(noteNumber, ConstValue.NOTE_NUM_MIN, ConstValue.NOTE_NUM_MAX) - ConstValue.A4_NOTE_NUM) * 100 + sweepPitch;
                        if (x >= PITCH_MAX || x <= PITCH_MIN)
                        {
                            // When the swept frequency reaches the lower or upper limit, stop the sound.
                            waveFreq = 0;
                            waveLength = sampleRate / a4Freq;
                        }
                        else
                        {
                            // Calculate the frequency
                            waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / (double)ConstValue.CENT_IN_OCTAVE));
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
                        int x = (Mathf.Clamp(noteNumber, ConstValue.NOTE_NUM_MIN, ConstValue.NOTE_NUM_MAX) - ConstValue.A4_NOTE_NUM) * 100 + sweepPitch;
                        waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / (double)ConstValue.CENT_IN_OCTAVE));
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
                            // Do not apply the LFO for the duration set in lfoDelay.
                            lfoNextEventPosition = (uint)(lfoDulation * lfoDelay / 2);
                        }
                        else
                        {
                            // The pitch is calculated by multiplying the triangle wave table value by lfoDepth.
                            lfoPitch = (triangleTable[lfoCount] * lfoDeapth * 100f / 255f);
                            int x = (Mathf.Clamp(noteNumber, ConstValue.NOTE_NUM_MIN, ConstValue.NOTE_NUM_MAX) - ConstValue.A4_NOTE_NUM) * 100 + (int)lfoPitch;
                            waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / (double)ConstValue.CENT_IN_OCTAVE));
                            waveLength = sampleRate / waveFreq;
                            lfoCount++;
                            if (lfoCount >= triangleTable.Length) { lfoCount -= triangleTable.Length; }
                            // Process at the interval set by lfoSpeed
                            lfoNextEventPosition += (uint)(sampleRate / (lfoSpeed * 2));
                        }
                    }
                }
            }

            if (waveFreq == 0 || seqMute)
            {
                /*** REST ***/
                // Treat frequency 0 as silent
                _sample = 0;
            }
            else
            {
                // Generate samples from frequency and volume
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
        // Sequence data processing
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
            // Process all commands with Dulation 0
            switch (seqList[seqListIndex].seqCmd)
            {
                case SEQ_CMD.SET_TEMPO:
                    seqTempo = seqList[seqListIndex].seqParam;
                    break;
                case SEQ_CMD.NOTE_ON:
                    noteNumber = seqList[seqListIndex].seqParam;
                    if (noteNumber < 0)
                    {
                        // For negative note numbers, since they are tied notes, set the gate to 100%.
                        tieGate = true;
                        noteNumber = -noteNumber;
                    }
                    if (programChange < 5)
                    {
                        // When the swept frequency reaches the lower or upper limit, stop the sound.
                        int x = (Mathf.Clamp(noteNumber, ConstValue.NOTE_NUM_MIN, ConstValue.NOTE_NUM_MAX) - ConstValue.A4_NOTE_NUM) * 100 + sweepPitch;   // Noten umber 69 is A4
                        if (x >=PITCH_MAX || x <= PITCH_MIN)
                        {
                            waveFreq = 0;
                            waveLength = sampleRate / a4Freq;
                        }
                        else
                        {
                            waveFreq = (float)(a4Freq * System.Math.Pow(2d, x / (double)ConstValue.CENT_IN_OCTAVE));
                            waveLength = sampleRate / waveFreq;
                        }
                    }
                    else
                    {
                        // The pitch of noise ranges from 0 to 15, so set the note number to the remainder when divided by 16.
                        waveFreq = a4Freq;
                        noiseIndex = noteNumber % noiseTable.Length;
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
                case SEQ_CMD.VOLUME:
                    seqVolume = Mathf.Clamp01(seqList[seqListIndex].seqParam / (float)ConstValue.SEQ_VOL_MAX);
                    waveVolume = seqVolume;
                    isEnvOn = false;
                    break;
                case SEQ_CMD.REPEAT_START:
                    // Remember the repeat start position
                    seqRepeatIndex = seqListIndex;
                    break;
                case SEQ_CMD.REPEAT_END:
                    if (seqRepeatCount < 0)
                    {
                        // If the counter is -1 (not repeating), remember the repeat count.
                        seqRepeatCount = seqList[seqListIndex].seqParam - 1;
                    }
                    else
                    {
                        // If the counter is 0 or greater, subtract 1.
                        seqRepeatCount--;
                    }
                    if (seqRepeatCount > 0)
                    {
                        // If the counter is 1 or greater, return to the repeat start position.
                        seqListIndex = seqRepeatIndex;
                    }
                    else
                    {
                        // When the counter reaches 0, set it to -1 (repeat ends)
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
                        // Is the envelope number registered?
                        if (envPatList[i].envId == envPatId)
                        {
                            envPatIndex = i;
                            break;
                        }
                    }
                    if (envPatIndex < 0)
                    {
                        // OFF if not registered.
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
                    // Preparing Envelope Settings
                    envPatId = seqList[seqListIndex].seqParam;
                    envParamList.Clear();
                    envLoopIndex = -1;
                    break;
                case SEQ_CMD.ENV_PARAM:
                    if (seqList[seqListIndex].seqParam < 0)
                    {
                        // The location at -1 is the loop point.
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
                        // If no loop point is set, loop the last parameter.
                        envLoopIndex = envParamList.Count - 1;
                    }
                    for (int i = 0; i < envPatList.Count; i++)
                    {
                        if (envPatList[i].envId == envPatId)
                        {
                            // If an envelope number is registered, delete it.
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
                        // If the LFO number is 0 or less, disable the LFO.
                        lfoPatId = 0;
                        isLfoOn = false;
                        break;
                    }
                    lfoPatIndex = -1;
                    for (int i = 0; i < lfoPatList.Count; i++)
                    {
                        // Check if the LFO number is registered in the LFO list
                        if (lfoPatList[i].lfoId == lfoPatId)
                        {
                            lfoPatIndex = i;
                            break;
                        }
                    }
                    if (lfoPatIndex < 0)
                    {
                        // If not registered, disable LFO.
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
                        // If the LFO number is registered, delete it.
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
                    seqEndPosition = seqNextEventPosition + sampleRate * 60d / seqTempo * seqList[seqListIndex].seqStep / tickPerNote;
                    break;
            }
            _duration = seqList[seqListIndex].seqStep;
            seqNextGatePosition = seqNextEventPosition + sampleRate * 60d / seqTempo * _duration * (tieGate ? 1f : gateStepRate) / tickPerNote;
            seqNextEventPosition += sampleRate * 60d / seqTempo * _duration / tickPerNote;
            seqListIndex++;
            if (eos) { break; }
        }
        if (seqListIndex >= seqList.Count)
        {
            // Processing after sequence completion
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
        // Preprocessing for sound generation
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
            do
            {
                ttIndex++;
                if (ttIndex >= triangleTable.Length)
                {
                    ttIndex = 0;
                }
                waveNextEventPosition += waveDevide;

            } while (wavePositon >= waveNextEventPosition);
        }
        waveSample = triangleTable[ttIndex];
    }

    private void PrepareNoiseSound(int _noiseIndex, bool _noiseShort)
    {
        int noiseFreq = noiseTable[_noiseIndex];
        noiseShortFreq = _noiseShort;
        if (!waveTie)
        {
            noiseReg = ConstValue.NOISE_REG;
            waveNextEventPosition = waveDevide;
        }
        waveDevide = noiseFreq * sampleRate / ConstValue.NES_FREQ;
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
        // 4-bit resolution triangle wave table
        float tVal = 0f;
        bool tRise = true;
        for (int i=0; i<triangleTable.Length; i++)
        {
            if (tRise)
            {
                // Ascending
                tVal += 1f / 8f;
                if (tVal >= 1f)
                {
                    tVal = 1f;
                    tRise = false;
                }
            }
            else
            {
                // Descending
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
