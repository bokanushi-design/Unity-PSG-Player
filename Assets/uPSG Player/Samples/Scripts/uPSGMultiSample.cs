using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGMultiSample : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private MMLSplitter mmlSplitter;   // Register the placed MML Splitter component.
    [SerializeField] private AudioMixer audioMixer; // Use AudioMixer to easily adjust the volume.
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioSource audioSource;   // AudioSource for playing rendered clip.

    [SerializeField] private GameObject jsonPanel;
    [SerializeField] private TMP_InputField multiSeqJsonField;  // Field for JSON display.

    public string mmlString;
    private bool[] isMute = new bool[4];

    void Start()
    {
        mmlSplitter.SetAllChannelsSampleRate(32000);    // Set the sample rate for all PSG Players.
        mmlString = Resources.Load<TextAsset>("sample-multi_channel_MML").text; // Load sample MML from the Resources folder.
        Resources.UnloadUnusedAssets();
        inputField.text = mmlString;

        audioMixer.GetFloat("PSG-Mix", out float _vol);
        _vol = Mathf.Clamp(_vol, -80f, 20f);
        float val = Mathf.Clamp01(Mathf.Pow(10f, _vol / 20f));
        volumeSlider.value = val;
        jsonPanel.SetActive(false);
    }

    public void OnPlayButton()
    {
        if (mmlSplitter.IsAnyChannelPlaying())  // Either a PSG Player is currently playing.
        {
            mmlSplitter.StopAllChannels();  // Stop playing all PSG Players.
        }
        else
        {
            mmlSplitter.SplitMML(mmlString);    // Distribute the multi-channel MML to each PSG Player.
            mmlSplitter.PlayAllChannels();  // Decode and play on all PSG Players.
        }
    }

    public void OnPlayRendered()
    {
        if (audioSource.isPlaying) { audioSource.Stop(); return; }
        mmlSplitter.SplitMML(mmlString);    // Distribute the multi-channel MML to each PSG Player.
        mmlSplitter.DecodeAllChannels();    // Decode on all PSG Players.
        AudioClip audioClip = mmlSplitter.ExportMixedAudioClip(32000);  // Render sequence to AudioClip.
        audioSource.PlayOneShot(audioClip);
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
        mmlSplitter.MuteChannel(_ch, _isMute); // Mute the channel.
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
        multiSeqJsonField.text = mmlSplitter.DecodeAndExportMultiSeqJson(false);    // Serialize the sequence into JSON.
    }

    public void OnImportJson()
    {
        string multiSeqJson = Resources.Load<TextAsset>("sample-multi-sequence_json").text; // Load the sample JSON file.
        mmlSplitter.ImportMultiSeqJson(multiSeqJson);   // Deserialize JSON into a sequence.
        mmlSplitter.PlayAllChannelsDecoded();   // Play the sequence.
    }

    public void OnJsonClose()
    {
        jsonPanel.SetActive(false);
    }
}
