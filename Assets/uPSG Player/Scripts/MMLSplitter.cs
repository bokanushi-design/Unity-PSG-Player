using System.IO;
using UnityEngine;

public class MMLSplitter : MonoBehaviour
{
    /// <summary>
    /// �e�`�����l����PSG Player�R���|�[�l���g
    /// </summary>
    [SerializeField] private PSGPlayer[] psgPlayers;
    /// <summary>
    /// ��������MML�f�[�^
    /// </summary>
    public string multiChMMLString;
    /// <summary>
    /// �������MML�f�[�^
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
    /// MML�𕪊�����PSG Player�ɑ��M����
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
    /// _multiChMMLString�Ŏw�肵��MML�𕪊�����PSG Player�ɑ��M����
    /// </summary>
    /// <param name="_multiChMMLString"></param>
    public void SplitMML(string _multiChMMLString)
    {
        multiChMMLString = _multiChMMLString;
        SplitMML();
    }

    /// <summary>
    /// �S�Ă�PSG Player�𓯎��ɍĐ��J�n
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
    /// �S�Ă�PSG Player�Ńf�R�[�h�ς݂̃V�[�P���X�𓯎��ɍĐ��J�n
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
    /// �S�Ă�PSG Player�̍Đ����~
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
    /// �w�肵���`�����l�����~���[�g����
    /// </summary>
    /// <param name="channel">�`�����l���̎w��</param>
    /// <param name="isMute">True�Ȃ�~���[�g</param>
    public void MuteChannel(int channel, bool isMute)
    {
        if (!CheckPlayersReady()) { return; }
        psgPlayers[channel].Mute(isMute);
    }

    /// <summary>
    /// �S�Ă�PSG Player�̃T���v�����[�g��ݒ肷��
    /// </summary>
    /// <param name="_rate">�T���v�����[�g�iHz�j</param>
    public void SetAllChannelsSampleRate(int _rate)
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.sampleRate = _rate;
        }
    }

    /// <summary>
    /// �S�Ă�PSG Player��AudioClip�̒�����ݒ肷��
    /// </summary>
    /// <param name="_msec">AudioClip�̒����i�~���b�j</param>
    public void SetAllChannelClipSize(int _msec)
    {
        if (!CheckPlayersReady()) { return; }
        foreach (var player in psgPlayers)
        {
            player.audioClipSizeMilliSec = _msec;
        }
    }

    /// <summary>
    /// �����ꂩ��PSG Player��AudioSource�̃N���b�v���Đ�����
    /// </summary>
    /// <returns>�����ꂩ���Đ����Ȃ�True</returns>
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
