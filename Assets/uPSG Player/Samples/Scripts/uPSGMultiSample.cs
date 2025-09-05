using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGMultiSample : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private MMLSplitter mmlSplitter;   // 設置したMML Splitterコンポーネントを登録する
    [SerializeField] private AudioMixer audioMixer; // 音量を調整しやすいようにAudioMixerを使用
    [SerializeField] private Slider volumeSlider;

    [SerializeField] private GameObject jsonPanel;
    [SerializeField] private TMP_InputField multiSeqJsonField;

    public string mmlString;
    private bool[] isMute = new bool[4];

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mmlSplitter.SetAllChannelsSampleRate(32000);    // 全てのPSG Playerのサンプルレートを設定
        mmlString = Resources.Load<TextAsset>("sample-multi_channel_MML").text;
        Resources.UnloadUnusedAssets();
        inputField.text = mmlString;

        audioMixer.GetFloat("PSG-Mix", out float _vol);
        _vol = Mathf.Clamp(_vol, -80f, 20f);
        float val = Mathf.Clamp01(Mathf.Pow(10f, _vol / 20f));
        volumeSlider.value = val;
        jsonPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPlayButton()
    {
        if (mmlSplitter.IsAnyChannelPlaying())  // いずれかのPSG Playerが再生中か
        {
            mmlSplitter.StopAllChannels();  // 全てのPSG Playerを再生停止
        }
        else
        {
            mmlSplitter.SplitMML(mmlString);    // マルチチャンネルMMLを各PSG Playerに分配
            mmlSplitter.PlayAllChannels();  // 全てのPSG Playerでデコードして再生
        }
    }

    public void OnInputChange(string inputText)
    {
        mmlString = inputText;
    }

    public void OnMuteToggleA(bool _isMute)
    {
        isMute[0] = _isMute;
        OnMute(0, isMute[0]);
    }
    public void OnMuteToggleB(bool _isMute)
    {
        isMute[1] = _isMute;
        OnMute(1, isMute[1]);
    }
    public void OnMuteToggleC(bool _isMute)
    {
        isMute[2] = _isMute;
        OnMute(2, isMute[2]);
    }
    public void OnMuteToggleD(bool _isMute)
    {
        isMute[3] = _isMute;
        OnMute(3, isMute[3]);
    }

    private void OnMute(int _ch, bool _isMute)
    {
        mmlSplitter.MuteChannel(_ch, _isMute); // チャンネルをミュート
    }

    public void OnNextButton()
    {
        SceneManager.LoadScene("MusicAndEffectSample");
    }

    public void OnVolumeChange(float val)
    {
        val = Mathf.Clamp(val, 0.0001f, 10f);
        audioMixer.SetFloat("PSG-Mix", 20f * Mathf.Log10(val));
    }

    public void OnExportJson()
    {
        jsonPanel.SetActive(true);
        mmlSplitter.SplitMML(mmlString);
        multiSeqJsonField.text = mmlSplitter.DecodeAndExportMultiSeqJson(false);
    }

    public void OnImportJson()
    {
        string multiSeqJson = Resources.Load<TextAsset>("sample-multi-sequence_json").text;
        mmlSplitter.ImportMultiSeqJson(multiSeqJson);
        mmlSplitter.PlayAllChannelsDecoded();
    }

    public void OnJsonClose()
    {
        jsonPanel.SetActive(false);
    }
}
