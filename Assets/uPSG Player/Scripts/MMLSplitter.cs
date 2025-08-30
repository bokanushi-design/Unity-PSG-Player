using System.IO;
using UnityEngine;

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
}
