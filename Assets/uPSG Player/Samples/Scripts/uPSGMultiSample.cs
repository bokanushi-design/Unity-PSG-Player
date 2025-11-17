using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGMultiSample : MonoBehaviour
{
    /**** v0.9.6beta ****/

    /// <summary>
    /// This is a sample that synthesizes polyphonic sound using four PSG Players and an MMLSplitter.
    /// This script is attached to the SceneController in the MultiChannelSample scene.
    /// The MML is displayed in the InputField at the center of the screen and streamed using the ÅgPLAY/STOPÅh button.
    /// The sample MML loads the MML text file located in the Resource folder.
    /// MML can be manually rewritten and will be reflected each time it is played back.
    /// Click the ÅgPlay RenderedÅh button to render the entire MML into an AudioClip.
    /// Regular rendering places a load on the CPU, so playback may freeze briefly until it resumes.
    /// Press ÅgPlay Async RenderedÅh to render asynchronously.
    /// Asynchronous rendering takes longer to produce output, but it can distribute CPU load.
    /// Pressing ÅgImport SequenceÅh will load the JSON file from the Resources folder and play it as a stream.
    /// Pressing ÅgExport SequenceÅh displays the decoded MML sequence in JSON format.
    /// </summary>

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private MMLSplitter mmlSplitter;   // Register the placed MML Splitter component.
    [SerializeField] private AudioMixer audioMixer; // Use AudioMixer to easily adjust the volume.
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private AudioSource audioSource;   // AudioSource for playing rendered clip.

    [SerializeField] private GameObject jsonPanel;
    [SerializeField] private TMP_InputField multiSeqJsonField;  // Field for JSON display.

    [SerializeField] private GameObject renderingPanel;
    [SerializeField] private RectTransform progressBar;
    private bool isAsyncRendering = false;

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
        renderingPanel.SetActive(false);
    }

    private void Update()
    {
        if (isAsyncRendering)
        {
            progressBar.localScale = new Vector3(mmlSplitter.asyncMultiRenderProgress, 1, 1);
            if (mmlSplitter.asyncMultiRenderIsDone) // Check if asynchronic rendering is done.
            {
                //mmlSplitter.PlayAllChannelsRenderedClipData();    // Play rendered clip at each channels.
                AudioClip audioClip = mmlSplitter.ExportMixedAudioClip(32000, true);  // Convert mixed rendered data to AudioClip.
                audioSource.PlayOneShot(audioClip);

                isAsyncRendering = false;
                renderingPanel.SetActive(false);
            }
        }
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

    public void OnPlayRenderedAsync()
    {
        if (audioSource.isPlaying) { audioSource.Stop(); return; }
        mmlSplitter.SplitMML(mmlString);    // Distribute the multi-channel MML to each PSG Player.
        mmlSplitter.DecodeAllChannels();    // Decode on all PSG Players.
        isAsyncRendering = mmlSplitter.RenderMultiSeqToClipDataAsync(32000, 1000);  // Start rendering asynchronous.
        if (isAsyncRendering) { renderingPanel.SetActive(true); }
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
