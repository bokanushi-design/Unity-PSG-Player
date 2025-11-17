using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class uPSGDTMFsample : MonoBehaviour
{
    /**** v0.9.6beta ****/

    /// <summary>
    /// This is a sample that generates DTMF tones using two PSG players.
    /// This script is attached to the SceneController in the DTMFsample scene.
    /// Pressing a button on the numeric keypad will generate a DTMF tone.
    /// DTMF is a method of transmitting codes using a composite signal composed of two audio frequency bands: low and high.
    /// Each time you press the button, it rewrites the MML and synthesizes the specified frequency.
    /// </summary>

    [SerializeField] MMLSplitter mmlSplitter;
    [SerializeField] TMP_InputField inputField;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider volumeSlider;

    private int[] dtmfLowTones = { 697, 770, 852, 941 };
    private int[] dtmfHighTones = { 1209, 1336, 1477, 1633 };
    private bool buttonDownFlag = false;

    private void Start()
    {
        audioMixer.GetFloat("PSG-Mix", out float _vol);
        _vol = Mathf.Clamp(_vol, -80f, 20f);
        float val = Mathf.Clamp01(Mathf.Pow(10f, _vol / 20f));
        volumeSlider.value = val;
    }

    public void OnNumButtonDown(int _buttonId)
    {
        buttonDownFlag = true;
        string lt = dtmfLowTones[_buttonId / 4].ToString();
        string ht = dtmfHighTones[_buttonId % 4].ToString();
        string mml = "AB @4l16\nA Lz" + lt + "&\nB Lz" + ht + "&";  // Sustain the sound by looping with a tie (&).
        inputField.text = mml;
        mmlSplitter.multiChMMLString = mml;
        mmlSplitter.SplitMML();
        mmlSplitter.PlayAllChannels();
    }

    public void OnNumButtonUp()
    {
        if (buttonDownFlag)
        {
            mmlSplitter.StopAllChannels();
            buttonDownFlag = false;
        }
    }

    public void OnNumButtonExit()
    {
        OnNumButtonUp();
    }

    public void OnNextButton()
    {
        SceneManager.LoadScene("SingleChannelSample");
    }

    public void OnVolumeChange(float val)
    {
        val = Mathf.Clamp(val, 0.0001f, 10f);
        audioMixer.SetFloat("PSG-Mix", 20f * Mathf.Log10(val));
    }

}
