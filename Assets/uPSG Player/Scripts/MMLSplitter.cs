using System.Collections.Generic;
using System.IO;
using UnityEngine;
using uPSG;

public class MMLSplitter : MonoBehaviour
{
    /// <summary>
    /// PSG Player component for each channel
    /// </summary>
    [Tooltip("PSG Player component for each channel")]
    [SerializeField] private PSGPlayer[] psgPlayers;
    /// <summary>
    /// MML data to split
    /// </summary>
    [Tooltip("MML data to split")]
    public string multiChMMLString;
    /// <summary>
    /// MML data after splitting
    /// </summary>
    [Tooltip("MML data after splitting")]
    private string[] mmlStrings;

    private void Awake()
    {
        if (!CheckPlayersReady())
        {
            Debug.LogError("PSG Player component not attached : " + gameObject.name);
        }
    }

    private bool CheckPlayersReady()
    {
        bool isOk = true;
        if (psgPlayers.Length == 0)
        {
            isOk = false;
        }
        else
        {
            foreach (var player in psgPlayers)
            {
                if (player == null)
                {
                    isOk = false;
                }
            }
        }
        return isOk;
    }

    /// <summary>
    /// Split the MML and send it to the PSG Player
    /// </summary>
    public void SplitMML()
    {
        if (!CheckPlayersReady())
        {
            Debug.LogError("PSG Player componet not attached : " + gameObject.name);
            return;
        }
        mmlStrings = new string[psgPlayers.Length];
        bool[] sendCh = new bool[psgPlayers.Length];
        StringReader reader = new StringReader(multiChMMLString);
        string line;
        sendCh[0] = true;
        while ((line = reader.ReadLine()) != null)
        {
            bool chFound = false;
            bool changeSend = false;
            bool[] _sendCh = new bool[psgPlayers.Length];
            int charCount = 0;
            while (charCount < line.Length)
            {
                char chr = line[charCount];
                if (chr >= 'A' && chr <= ('A' + psgPlayers.Length) || chr == ' ')
                {
                    if (chr != ' ')
                    {
                        int chId = chr - 'A';
                        if (chId < _sendCh.Length)
                        {
                            _sendCh[chId] = true;
                        }
                        chFound = true;
                    }
                    else
                    {
                        if (chFound) { changeSend = true; }
                    }
                }
                else
                {
                    break;
                }
                charCount++;
            }

            if (changeSend)
            {
                for (int i = 0; i < sendCh.Length; i++)
                {
                    sendCh = _sendCh;
                }
            }

            for (int i = 0; i < sendCh.Length; i++)
            {
                if (sendCh[i])
                {
                    mmlStrings[i] += line.Substring(charCount) + "\n";
                }
            }
        }
        reader.Close();
        int idx = 0;
        foreach (var mml in mmlStrings)
        {
            psgPlayers[idx].mmlString = (mml != null) ? mml : "";
            idx++;
        }
    }

    /// <summary>
    /// Split the MML specified in the _multiChMMLString and send it to the PSG Player.
    /// </summary>
    /// <param name="_multiChMMLString"></param>
    public void SplitMML(string _multiChMMLString)
    {
        multiChMMLString = _multiChMMLString;
        SplitMML();
    }

    /// <summary>
    /// Start playing all PSG Player simultaneously
    /// </summary>
    public void PlayAllChannels()
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.Play();
        }
    }

    /// <summary>
    /// Start simultaneous playback of decoded sequences on all PSG player
    /// </summary>
    public void PlayAllChannelsDecoded()
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.PlayDecoded();
        }
    }

    /// <summary>
    /// Start simultaneous playback of sequences on all PSG player (same as PlayAllChannelsDecoded())
    /// </summary>
    public void PlayAllChannelsSequence()
    {
        PlayAllChannelsDecoded();
    }

    /// <summary>
    /// Decode MML on all PSG players
    /// </summary>
    public void DecodeAllChannels()
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var pPlayer in psgPlayers)
        {
            pPlayer.DecodeMML();
        }
    }

    /// <summary>
    /// Stop all PSG Player playback
    /// </summary>
    public void StopAllChannels()
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.Stop();
        }
    }

    /// <summary>
    /// Mute the specified channel
    /// </summary>
    /// <param name="channel">Specifying the channel</param>
    /// <param name="isMute">Set to True to mute</param>
    public void MuteChannel(int channel, bool isMute)
    {
        if (!CheckPlayersReady()) { return; }
        psgPlayers[channel].Mute(isMute);
    }

    /// <summary>
    /// Set all PSG player sample rates
    /// </summary>
    /// <param name="_rate">Sample rate (Hz)</param>
    public void SetAllChannelsSampleRate(int _rate)
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.sampleRate = _rate;
        }
    }

    /// <summary>
    /// Set the length of all PSG Player AudioClips
    /// </summary>
    /// <param name="_msec">AudioClip duration (milliseconds)</param>
    public void SetAllChannelClipSize(int _msec)
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.audioClipSizeMilliSec = _msec;
        }
    }

    /// <summary>
    /// Is any PSG Player's AudioSource clip currently playing?
    /// </summary>
    /// <returns>If any are playing, True</returns>
    public bool IsAnyChannelPlaying()
    {
        if (!CheckPlayersReady()) { return false; }
        bool result = false;
        foreach (var player in psgPlayers)
        {
            result |= player.IsPlaying();
        }
        return result;
    }

    /// <summary>
    /// Export decoded multi-channel sequences as JSON
    /// </summary>
    /// <param name="_prettyPrint">If True, format the output for readability</param>
    /// <returns>JSON formatted string</returns>
    public string ExportMultiSeqJson(bool _prettyPrint)
    {
        MultiSeqJson multiSeqJson = new();
        foreach (var pPlayer in psgPlayers)
        {
            SeqJson seqJson = pPlayer.GetSeqJson();
            multiSeqJson.seqJsonList.Add(seqJson);
        }
        string multiSeqJsonString = JsonUtility.ToJson(multiSeqJson, _prettyPrint);
        return multiSeqJsonString;
    }

    public string ExportMultiSeqJson()
    {
        return ExportMultiSeqJson(false);
    }

    /// <summary>
    /// Decode multi-channel MML into sequences and export JSON
    /// </summary>
    /// <param name="_prettyPrint">If True, format the output for readability</param>
    /// <returns>JSON formatted string</returns>
    public string DecodeAndExportMultiSeqJson(bool _prettyPrint)
    {
        DecodeAllChannels();
        return ExportMultiSeqJson(_prettyPrint);
    }

    public string DecodeAndExportMultiSeqJson()
    {
        return DecodeAndExportMultiSeqJson(false);
    }

    /// <summary>
    /// Import multichannel JSON data into sequences
    /// </summary>
    /// <param name="_jsonString">JSON formatted string</param>
    public void ImportMultiSeqJson(string _jsonString)
    {
        MultiSeqJson multiSeqJson = JsonUtility.FromJson<MultiSeqJson>(_jsonString);
        int seqJsonCount = 0;
        foreach (var pPlayer in psgPlayers)
        {
            if (seqJsonCount < multiSeqJson.seqJsonList.Count)
            {
                SeqJson seqJson = multiSeqJson.seqJsonList[seqJsonCount];
                pPlayer.SetSeqJson(seqJson);
            }
            else
            {
                SeqJson seqJson = new();
                seqJson.jsonTickPerNote = ConstValue.DEFAULT_TICK_PER_NOTE;
                SeqEvent seqEvent = new SeqEvent(SEQ_CMD.END_OF_SEQ, 0, 0);
                seqJson.jsonSeqList.Add(seqEvent);
                pPlayer.SetSeqJson(seqJson);
            }
            seqJsonCount++;
        }
    }

    /// <summary>
    /// Mix the waveform data rendered by each PSG Player and export it as an AudioClip.
    /// </summary>
    /// <param name="_sampleRate"></param>
    /// <returns>Rendered AudioClip</returns>
    public AudioClip ExportMixedAudioClip(int _sampleRate)
    {
        SetAllChannelsSampleRate(_sampleRate);
        List<float[]> channelClipData = new();
        int mixedDataLength = 0;
        foreach(var pPlayer in psgPlayers)
        {
            float[] clipData = pPlayer.RenderSequenceTodClipData();
            channelClipData.Add(clipData);
            if (clipData.Length > mixedDataLength) { mixedDataLength = clipData.Length; };
        }

        float[] mixedData = new float[mixedDataLength];
        for (int dataCount=0; dataCount<mixedDataLength; dataCount++)
        {
            float data = 0;
            for (int chCount=0; chCount<channelClipData.Count; chCount++)
            {
                if (dataCount < channelClipData[chCount].Length)
                {
                    data += channelClipData[chCount][dataCount];
                }
            }
            mixedData[dataCount] = data / channelClipData.Count;
        }

        AudioClip audioClip = AudioClip.Create("Mixed Rendered Sound", mixedDataLength, 1, _sampleRate, false);
        audioClip.SetData(mixedData, 0);
        return audioClip;
    }
}
