using System.IO;
using UnityEngine;

public class MMLSplitter : MonoBehaviour
{
    /// <summary>
    /// 各チャンネルのPSG Playerコンポーネント
    /// </summary>
    [SerializeField] private PSGPlayer[] psgPlayers;
    /// <summary>
    /// 分割するMMLデータ
    /// </summary>
    public string multiChMMLString;
    /// <summary>
    /// 分割後のMMLデータ
    /// </summary>
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
    /// MMLを分割してPSG Playerに送信する
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
    /// _multiChMMLStringで指定したMMLを分割してPSG Playerに送信する
    /// </summary>
    /// <param name="_multiChMMLString"></param>
    public void SplitMML(string _multiChMMLString)
    {
        multiChMMLString = _multiChMMLString;
        SplitMML();
    }

    /// <summary>
    /// 全てのPSG Playerを同時に再生開始
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
    /// 全てのPSG Playerでデコード済みのシーケンスを同時に再生開始
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
    /// 全てのPSG Playerの再生を停止
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
    /// 指定したチャンネルをミュートする
    /// </summary>
    /// <param name="channel">チャンネルの指定</param>
    /// <param name="isMute">Trueならミュート</param>
    public void MuteChannel(int channel, bool isMute)
    {
        if (!CheckPlayersReady()) { return; }
        psgPlayers[channel].Mute(isMute);
    }

    /// <summary>
    /// 全てのPSG Playerのサンプルレートを設定する
    /// </summary>
    /// <param name="_rate">サンプルレート（Hz）</param>
    public void SetAllChannelsSampleRate(int _rate)
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.sampleRate = _rate;
        }
    }

    /// <summary>
    /// 全てのPSG PlayerのAudioClipの長さを設定する
    /// </summary>
    /// <param name="_msec">AudioClipの長さ（ミリ秒）</param>
    public void SetAllChannelClipSize(int _msec)
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.audioClipSizeMilliSec = _msec;
        }
    }

    /// <summary>
    /// いずれかのPSG PlayerのAudioSourceのクリップが再生中か
    /// </summary>
    /// <returns>いずれかが再生中ならTrue</returns>
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
