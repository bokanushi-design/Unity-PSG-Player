using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGDPCMSample : MonoBehaviour
{
    /**** v0.9.9beta ****/

    /// <summary>
    /// This is a sample that uses 5 PSG players to play 4 PSG sounds and 1 DPCM sound.
    /// DPCM data is loaded and used from binary files generated in advance by DpcmConverter.
    /// In this sample, DPCM data is assigned only to Channel E, so DPCM will not play on the other channels.
    /// Also, since DPCM has trouble reproducing particularly high frequencies, I've turned up the volume on the AudioSource in Channel E.
    /// </summary>

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private MultiChannelController multiChannelController;   // Register the placed MultiChannelController component.
    [SerializeField] private AudioMixer audioMixer; // Use AudioMixer to easily adjust the volume.
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private PSGPlayer dpcmPlayer; // PSG Player for playing DPCM datas.

    public string mmlString;
    private int audioSampleRate = 44100;
    private string sampleMML = "sample-PSG_and_DPCM_MML";
    private bool[] isMute = new bool[5];

    private void Start()
    {
        multiChannelController.SetAllChannelsSampleRate(audioSampleRate);    // Set the sample rate for all PSG Players.
        mmlString = Resources.Load<TextAsset>(sampleMML).text; // Load sample MML from the Resources folder.
        inputField.text = mmlString;

        // Load the DPCM datas from the Resources folder and register it in PSGPlayer's dpcmDataList.
        dpcmPlayer.dpcmDataList.Clear();
        byte[] dpcmData1 = Resources.Load<TextAsset>("dpcm_woodblock-max_Rate_24_28kHz").bytes;
        dpcmPlayer.dpcmDataList.Add(dpcmData1);
        byte[] dpcmData2 = Resources.Load<TextAsset>("dpcm_timbales-max_Rate_24_28kHz").bytes;
        dpcmPlayer.dpcmDataList.Add(dpcmData2);
        Resources.UnloadUnusedAssets();

        audioMixer.GetFloat("PSG-Mix", out float _vol);
        _vol = Mathf.Clamp(_vol, -80f, 20f);
        float val = Mathf.Clamp01(Mathf.Pow(10f, _vol / 20f));
        volumeSlider.value = val;
    }

    public void OnPlayButton()
    {
        if (multiChannelController.IsAnyChannelPlaying())  // Either a PSG Player is currently playing.
        {
            multiChannelController.StopAllChannels();  // Stop playing all PSG Players.
        }
        else
        {
            multiChannelController.SplitMML(mmlString);    // Distribute the multi-channel MML to each PSG Player.
            multiChannelController.PlayAllChannels();  // Decode and play on all PSG Players.
        }
    }

    public void OnInputChange(string inputText)
    {
        mmlString = inputText;
    }

    public void OnVolumeChange(float val)
    {
        val = Mathf.Clamp(val, 0.0001f, 10f);
        audioMixer.SetFloat("PSG-Mix", 20f * Mathf.Log10(val));
    }

    public void OnCopyMml()
    {
        GUIUtility.systemCopyBuffer = mmlString;
    }

    public void OnClearMml()
    {
        mmlString = "";
        inputField.text = mmlString;
    }

    public void OnNextButton()
    {
        SceneManager.LoadScene("DTMFsample");
    }

    public void OnReloadMml()
    {
        mmlString = Resources.Load<TextAsset>(sampleMML).text;
        Resources.UnloadUnusedAssets();
        inputField.text = mmlString;
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
    public void OnMuteToggleE(bool _isMute)
    {
        isMute[4] = _isMute;
        OnMute(4, isMute[4]);
    }

    private void OnMute(int _ch, bool _isMute)
    {
        multiChannelController.NoteSyncMuteChannel(_ch, _isMute);   // Mute the channel with note sync.
    }

}
